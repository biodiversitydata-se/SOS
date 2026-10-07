using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Gis;
using SOS.Observations.Api.Helpers;
using ZiggyCreatures.Caching.Fusion;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationsCache
{
    string CreateCacheKey(int? roleId, string? authorizationApplicationIdentifier, bool sensitiveObservations, SearchFilterInternalDto filter);
    bool TryGet(string cacheKey, LatLonBoundingBoxDto? requestedBbox, out List<CachedLocationAggregate> locations, bool requireExactBoundingBox = false);
    void Set(string cacheKey, LatLonBoundingBoxDto? bbox, List<CachedLocationAggregate> locations);
    LatLonBoundingBoxDto? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations);
}

internal sealed class AdaptiveSearchLocationsCache : IAdaptiveSearchLocationsCache
{
    internal const string CacheKeyPrefix = "adaptive-search-locations:v5:";
    internal const int MaxEntriesPerKey = 8;
    internal const int MaxLocationsPerBucket = 10000;
    internal static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    // Shared across scoped instances. Bounded stripes avoid retaining one lock per cache key.
    private static readonly object[] WriteLocks = Enumerable.Range(0, 64).Select(_ => new object()).ToArray();

    private readonly IFusionCache _cache;
    private readonly AdaptiveSearchLocationsCacheMonitor _cacheMonitor;
    private readonly TimeProvider _timeProvider;

    public AdaptiveSearchLocationsCache(
        IFusionCacheProvider fusionCacheProvider,
        AdaptiveSearchLocationsCacheMonitor cacheMonitor,
        TimeProvider? timeProvider = null)
    {
        _cache = (fusionCacheProvider ?? throw new ArgumentNullException(nameof(fusionCacheProvider)))
            .GetCache(Controllers.ObservationsController.AdaptiveSearchLocationsCacheName);
        _cacheMonitor = cacheMonitor ?? throw new ArgumentNullException(nameof(cacheMonitor));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string CreateCacheKey(int? roleId, string? authorizationApplicationIdentifier, bool sensitiveObservations, SearchFilterInternalDto filter)
    {
        var cacheKeyParameters = new[]
        {
            "version=5",
            $"roleId={roleId}",
            $"authorizationApplicationIdentifier={authorizationApplicationIdentifier}",
            $"sensitiveObservations={sensitiveObservations}"
        };
        var hash = SearchFilterCacheKeyHelper.CreateKey(string.Join("&", cacheKeyParameters) + ":", filter, ignoreBoundingBox: true);
        return $"{CacheKeyPrefix}{hash}";
    }

    public bool TryGet(string cacheKey, LatLonBoundingBoxDto? requestedBbox, out List<CachedLocationAggregate> locations, bool requireExactBoundingBox = false)
    {
        locations = new List<CachedLocationAggregate>();
        var cachedBucket = _cache.TryGet<AdaptiveSearchLocationsCacheBucket>(cacheKey);
        var bucket = cachedBucket.HasValue ? cachedBucket.Value : null;
        if (bucket?.Entries?.Any() != true)
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var activeEntries = bucket.Entries.Where(entry => now - entry.CreatedUtc < CacheDuration)
            .OrderByDescending(entry => entry.CreatedUtc).ToList();
        var exactEntry = activeEntries.FirstOrDefault(entry => AreSameBoundingBox(entry.BoundingBox, requestedBbox));
        if (exactEntry != null)
        {
            // Preserve the original matches, including buffers whose centers lie outside the bbox.
            locations = exactEntry.Locations.ToList();
            return true;
        }

        if (requireExactBoundingBox) return false;

        // Only reuse a single complete result. Combining summaries from multiple requests cannot
        // deduplicate observations, even when the underlying LocationIds are globally unique.
        var cacheEntry = activeEntries.FirstOrDefault(entry =>
            ContainsBoundingBox(entry.BoundingBox, requestedBbox) &&
            entry.Locations.All(location => location.LocationIds.Count <= 1));
        if (cacheEntry == null) return false;

        // A summary combining multiple LocationIds may straddle the new bbox after coordinate rounding.
        locations = FilterByBoundingBox(cacheEntry.Locations, requestedBbox).ToList();
        return true;
    }

    public void Set(string cacheKey, LatLonBoundingBoxDto? bbox, List<CachedLocationAggregate> locations)
    {
        if (locations.Count > MaxLocationsPerBucket)
        {
            _cacheMonitor.LogSkippedTooLargeEntry(cacheKey, locations.Count, MaxLocationsPerBucket);
            return;
        }

        var writeLock = WriteLocks[(uint)StringComparer.Ordinal.GetHashCode(cacheKey) % (uint)WriteLocks.Length];
        lock (writeLock)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var cachedBucket = _cache.TryGet<AdaptiveSearchLocationsCacheBucket>(cacheKey);
            // Publish a new entry list so readers can safely finish using the previous snapshot.
            var bucket = new AdaptiveSearchLocationsCacheBucket
            {
                Entries = cachedBucket.HasValue && cachedBucket.Value != null
                    ? cachedBucket.Value.Entries.Where(entry => now - entry.CreatedUtc < CacheDuration &&
                        !AreSameBoundingBox(entry.BoundingBox, bbox)).ToList()
                    : new()
            };
            bucket.Entries.Insert(0, new AdaptiveSearchLocationsCacheEntry
            {
                BoundingBox = bbox == null ? null : LatLonBoundingBoxDto.Create(
                    bbox.TopLeft.Longitude, bbox.TopLeft.Latitude, bbox.BottomRight.Longitude, bbox.BottomRight.Latitude),
                Locations = locations.ToList(),
                CreatedUtc = now
            });
            var trimResult = Trim(bucket);
            _cacheMonitor.LogBucketTrim(
                cacheKey,
                trimResult.RemovedByEntryLimit,
                trimResult.RemovedBySizeLimit,
                trimResult.EntryCountBefore,
                trimResult.EntryCountAfter,
                trimResult.SizeUnitsBefore,
                trimResult.SizeUnitsAfter);

            var cacheOptions = new FusionCacheEntryOptions
            {
                Duration = CacheDuration,
                IsFailSafeEnabled = false
            }.SetSize(bucket.EstimatedSizeUnits);
            _cache.Set(cacheKey, bucket, cacheOptions);
            _cacheMonitor.TrackBucketWrite(cacheKey, bucket.EstimatedSizeUnits);
        }
    }

    public LatLonBoundingBoxDto? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations)
    {
        double? left = null;
        double? top = null;
        double? right = null;
        double? bottom = null;

        foreach (var locationAggregate in locations)
        {
            var location = locationAggregate.Location;
            if (!location.DecimalLongitude.HasValue || !location.DecimalLatitude.HasValue)
            {
                continue;
            }

            var longitude = location.DecimalLongitude.Value;
            var latitude = location.DecimalLatitude.Value;
            left = left.HasValue ? Math.Min(left.Value, longitude) : longitude;
            right = right.HasValue ? Math.Max(right.Value, longitude) : longitude;
            bottom = bottom.HasValue ? Math.Min(bottom.Value, latitude) : latitude;
            top = top.HasValue ? Math.Max(top.Value, latitude) : latitude;
        }

        return left.HasValue && top.HasValue && right.HasValue && bottom.HasValue
            ? LatLonBoundingBoxDto.Create(left, top, right, bottom)
            : null;
    }

    private static TrimResult Trim(AdaptiveSearchLocationsCacheBucket bucket)
    {
        var entryCountBefore = bucket.Entries.Count;
        var sizeUnitsBefore = bucket.EstimatedSizeUnits;
        var removedByEntryLimit = 0;
        var removedBySizeLimit = 0;

        while (bucket.Entries.Count > MaxEntriesPerKey)
        {
            bucket.Entries.RemoveAt(bucket.Entries.Count - 1);
            removedByEntryLimit++;
        }

        while (bucket.EstimatedSizeUnits > MaxLocationsPerBucket && bucket.Entries.Count > 0)
        {
            bucket.Entries.RemoveAt(bucket.Entries.Count - 1);
            removedBySizeLimit++;
        }

        return new TrimResult(
            removedByEntryLimit,
            removedBySizeLimit,
            entryCountBefore,
            bucket.Entries.Count,
            sizeUnitsBefore,
            bucket.EstimatedSizeUnits);
    }

    private readonly record struct TrimResult(
        int RemovedByEntryLimit,
        int RemovedBySizeLimit,
        int EntryCountBefore,
        int EntryCountAfter,
        int SizeUnitsBefore,
        int SizeUnitsAfter);

    private static IEnumerable<CachedLocationAggregate> FilterByBoundingBox(
        IEnumerable<CachedLocationAggregate> locations,
        LatLonBoundingBoxDto? bbox)
    {
        if (bbox?.TopLeft == null || bbox.BottomRight == null)
        {
            return locations;
        }

        return locations.Where(location =>
            location.Location.DecimalLongitude.HasValue &&
            location.Location.DecimalLatitude.HasValue &&
            IsCoordinateWithinBoundingBox(
                location.Location.DecimalLongitude.Value,
                location.Location.DecimalLatitude.Value,
                bbox));
    }

    private static bool ContainsBoundingBox(LatLonBoundingBoxDto? cachedBbox, LatLonBoundingBoxDto? requestedBbox)
    {
        if (requestedBbox?.TopLeft == null || requestedBbox.BottomRight == null)
        {
            return cachedBbox?.TopLeft == null || cachedBbox?.BottomRight == null;
        }

        if (cachedBbox?.TopLeft == null || cachedBbox.BottomRight == null)
        {
            return false;
        }

        return cachedBbox.TopLeft.Longitude <= requestedBbox.TopLeft.Longitude &&
               cachedBbox.BottomRight.Longitude >= requestedBbox.BottomRight.Longitude &&
               cachedBbox.TopLeft.Latitude >= requestedBbox.TopLeft.Latitude &&
               cachedBbox.BottomRight.Latitude <= requestedBbox.BottomRight.Latitude;
    }

    private static bool AreSameBoundingBox(LatLonBoundingBoxDto? left, LatLonBoundingBoxDto? right)
    {
        if (left?.TopLeft == null || left.BottomRight == null || right?.TopLeft == null || right.BottomRight == null)
        {
            return left?.TopLeft == null && left?.BottomRight == null && right?.TopLeft == null && right?.BottomRight == null;
        }

        return left.TopLeft.Longitude.Equals(right.TopLeft.Longitude) &&
               left.TopLeft.Latitude.Equals(right.TopLeft.Latitude) &&
               left.BottomRight.Longitude.Equals(right.BottomRight.Longitude) &&
               left.BottomRight.Latitude.Equals(right.BottomRight.Latitude);
    }

    private static bool IsCoordinateWithinBoundingBox(double longitude, double latitude, LatLonBoundingBoxDto bbox)
    {
        return longitude >= bbox.TopLeft.Longitude &&
               longitude <= bbox.BottomRight.Longitude &&
               latitude <= bbox.TopLeft.Latitude &&
               latitude >= bbox.BottomRight.Latitude;
    }
}
