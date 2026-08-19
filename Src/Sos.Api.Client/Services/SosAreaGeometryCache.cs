using Microsoft.Extensions.Caching.Memory;
using SOS.Api.Contracts.Areas.Responses;
using System.Collections.Concurrent;

namespace SOS.Api.Client.Services;

internal sealed class SosAreaGeometryCache
{
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly ConcurrentDictionary<string, Lazy<Task<AreaWithGeometryDto>>> _inflight = [];

    public SosAreaGeometryCache(IMemoryCache cache, int maxConcurrency)
    {
        _cache = cache;
        _concurrencyLimiter = new SemaphoreSlim(Math.Max(1, maxConcurrency));
    }

    public async Task<AreaWithGeometryDto> GetOrCreateAsync(
        string key,
        TimeSpan slidingExpiration,
        Func<CancellationToken, Task<AreaWithGeometryDto>> factory,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<AreaWithGeometryDto>(key, out var cached) && cached is not null)
        {
            return cached;
        }

        var lazy = _inflight.GetOrAdd(
            key,
            _ => new Lazy<Task<AreaWithGeometryDto>>(
                // A caller may stop waiting without cancelling the one shared
                // export that other concurrent callers still depend on.
                () => LoadAsync(key, slidingExpiration, factory, CancellationToken.None),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return await lazy.Value.WaitAsync(cancellationToken);
    }

    private async Task<AreaWithGeometryDto> LoadAsync(
        string key,
        TimeSpan slidingExpiration,
        Func<CancellationToken, Task<AreaWithGeometryDto>> factory,
        CancellationToken cancellationToken)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<AreaWithGeometryDto>(key, out var cached) && cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = slidingExpiration
            });
            return value;
        }
        finally
        {
            _inflight.TryRemove(key, out _);
            _concurrencyLimiter.Release();
        }
    }
}
