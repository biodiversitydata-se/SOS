using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Events;

namespace SOS.Observations.Api.Services;

public sealed class FusionCacheUsageMonitorSnapshot
{
    public FusionCacheUsageMonitorSnapshot(
        string cacheName,
        long totalWrites,
        int trackedCacheKeys,
        long usedSizeUnits,
        int sizeLimitUnits,
        long capacityEvictions,
        string? key = null,
        int? entrySizeUnits = null)
    {
        CacheName = cacheName;
        TotalWrites = totalWrites;
        TrackedCacheKeys = trackedCacheKeys;
        UsedSizeUnits = usedSizeUnits;
        SizeLimitUnits = sizeLimitUnits;
        CapacityEvictions = capacityEvictions;
        Key = key;
        EntrySizeUnits = entrySizeUnits;
    }

    public string CacheName { get; }
    public long TotalWrites { get; }
    public int TrackedCacheKeys { get; }
    public long UsedSizeUnits { get; }
    public int SizeLimitUnits { get; }
    public double UsagePercent => SizeLimitUnits <= 0 ? 0 : Math.Round(UsedSizeUnits * 100d / SizeLimitUnits, 1);
    public long CapacityEvictions { get; }
    public string? Key { get; }
    public int? EntrySizeUnits { get; }
}

public sealed class FusionCacheUsageMonitorState
{
    private readonly string _cacheName;
    private readonly int _sizeLimitUnits;
    private readonly Dictionary<string, int> _entrySizeUnitsByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();
    private long _capacityEvictions;
    private long _totalWrites;
    private long _usedSizeUnits;

    public FusionCacheUsageMonitorState(string cacheName, int sizeLimitUnits)
    {
        _cacheName = cacheName ?? throw new ArgumentNullException(nameof(cacheName));
        _sizeLimitUnits = sizeLimitUnits;
    }

    public FusionCacheUsageMonitorSnapshot TrackWrite(string key, int sizeUnits)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key must not be empty.", nameof(key));
        }

        sizeUnits = NormalizeSizeUnits(sizeUnits);

        lock (_syncRoot)
        {
            _entrySizeUnitsByKey.TryGetValue(key, out var oldSizeUnits);
            _entrySizeUnitsByKey[key] = sizeUnits;
            _usedSizeUnits += sizeUnits - oldSizeUnits;
            _totalWrites++;

            return CreateSnapshot(key, sizeUnits);
        }
    }

    public FusionCacheUsageMonitorSnapshot TrackCapacityEviction(string key, IEnumerable<string>? alternateKeys = null)
    {
        lock (_syncRoot)
        {
            _capacityEvictions++;

            foreach (var keyCandidate in CreateDistinctKeyCandidates(key, alternateKeys))
            {
                if (!_entrySizeUnitsByKey.Remove(keyCandidate, out var removedSizeUnits))
                {
                    continue;
                }

                _usedSizeUnits -= removedSizeUnits;
                return CreateSnapshot(keyCandidate, removedSizeUnits);
            }

            return CreateSnapshot(key);
        }
    }

    public FusionCacheUsageMonitorSnapshot Snapshot()
    {
        lock (_syncRoot)
        {
            return CreateSnapshot();
        }
    }

    public static bool ShouldLogPeriodicWrite(FusionCacheUsageMonitorSnapshot snapshot, int writeLogInterval)
    {
        return writeLogInterval > 0 &&
               snapshot.TotalWrites > 0 &&
               snapshot.TotalWrites % writeLogInterval == 0;
    }

    private static int NormalizeSizeUnits(int sizeUnits)
    {
        return Math.Max(1, sizeUnits);
    }

    private static IEnumerable<string> CreateDistinctKeyCandidates(string key, IEnumerable<string>? alternateKeys)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            yield return key;
        }

        if (alternateKeys == null)
        {
            yield break;
        }

        foreach (var alternateKey in alternateKeys.Where(alternateKey => !string.IsNullOrWhiteSpace(alternateKey)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!string.Equals(alternateKey, key, StringComparison.OrdinalIgnoreCase))
            {
                yield return alternateKey;
            }
        }
    }

    private FusionCacheUsageMonitorSnapshot CreateSnapshot(string? key = null, int? entrySizeUnits = null)
    {
        return new FusionCacheUsageMonitorSnapshot(
            _cacheName,
            _totalWrites,
            _entrySizeUnitsByKey.Count,
            _usedSizeUnits,
            _sizeLimitUnits,
            _capacityEvictions,
            key,
            entrySizeUnits);
    }
}

internal sealed class FusionCacheUsageMonitor
{
    private readonly IFusionCache _cache;
    private readonly string _cacheDisplayName;
    private readonly bool _isEnabled;
    private readonly ILogger _logger;
    private readonly FusionCacheUsageMonitorState _state;
    private readonly int _writeLogInterval;
    private int _isStarted;

    public FusionCacheUsageMonitor(
        IFusionCache cache,
        ILogger logger,
        int sizeLimitUnits,
        int writeLogInterval,
        string cacheDisplayName,
        bool isEnabled)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _writeLogInterval = writeLogInterval;
        _cacheDisplayName = cacheDisplayName ?? throw new ArgumentNullException(nameof(cacheDisplayName));
        _isEnabled = isEnabled;
        _state = new FusionCacheUsageMonitorState(_cache.CacheName, sizeLimitUnits);
    }

    public void StartMonitoring()
    {
        if (!_isEnabled)
        {
            return;
        }

        if (Interlocked.Exchange(ref _isStarted, 1) == 1)
        {
            return;
        }

        _cache.Events.Memory.Eviction += OnMemoryEviction;
    }

    public void TrackWrite(string key, int sizeUnits)
    {
        if (!_isEnabled)
        {
            return;
        }

        var snapshot = _state.TrackWrite(key, sizeUnits);
        if (!FusionCacheUsageMonitorState.ShouldLogPeriodicWrite(snapshot, _writeLogInterval))
        {
            return;
        }

        LogUsage(LogLevel.Information, "FusionCache periodic usage.", snapshot);
    }

    private void OnMemoryEviction(object? sender, FusionCacheEntryEvictionEventArgs e)
    {
        if (!_isEnabled)
        {
            return;
        }

        if (e.Reason != EvictionReason.Capacity)
        {
            return;
        }

        var key = e.Key ?? string.Empty;
        var snapshot = _state.TrackCapacityEviction(key, CreateEvictionKeyCandidates(key));
        LogUsage(LogLevel.Warning, "FusionCache capacity eviction.", snapshot);
    }

    private IEnumerable<string> CreateEvictionKeyCandidates(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            yield break;
        }

        var namedCachePrefix = $"{_cache.CacheName}:";
        if (key.StartsWith(namedCachePrefix, StringComparison.OrdinalIgnoreCase))
        {
            yield return key[namedCachePrefix.Length..];
        }
        else
        {
            yield return $"{namedCachePrefix}{key}";
        }
    }

    private void LogUsage(LogLevel logLevel, string message, FusionCacheUsageMonitorSnapshot snapshot)
    {
        _logger.Log(
            logLevel,
            "{Message} CacheDisplayName={CacheDisplayName}, CacheName={CacheName}, Key=\"{Key}\", EntrySizeUnits={EntrySizeUnits}, TotalWrites={TotalWrites}, TrackedCacheKeys={TrackedCacheKeys}, UsedSizeUnits={UsedSizeUnits}, SizeLimitUnits={SizeLimitUnits}, UsagePercent={UsagePercent}, CapacityEvictions={CapacityEvictions}, MemoryUsageSummary=\"{MemoryUsageSummary}\"",
            message,
            _cacheDisplayName,
            snapshot.CacheName,
            snapshot.Key,
            snapshot.EntrySizeUnits,
            snapshot.TotalWrites,
            snapshot.TrackedCacheKeys,
            snapshot.UsedSizeUnits,
            snapshot.SizeLimitUnits,
            snapshot.UsagePercent,
            snapshot.CapacityEvictions,
            LogHelper.GetMemoryUsageSummary());
    }
}
