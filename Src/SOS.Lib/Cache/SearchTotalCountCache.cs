using Microsoft.Extensions.Caching.Memory;

namespace SOS.Lib.Cache;

/// <summary>
/// A small, process-local cache for observation search total counts.
/// </summary>
public sealed class SearchTotalCountCache : IDisposable
{
    public const int MaxEntries = 50_000;

    private readonly MemoryCache _cache;

    public SearchTotalCountCache() : this(new MemoryCacheOptions())
    {
    }

    public SearchTotalCountCache(MemoryCacheOptions options)
    {
        options.SizeLimit ??= MaxEntries;
        _cache = new MemoryCache(options);
    }

    public bool TryGetValue(object key, out long totalCount) =>
        _cache.TryGetValue(key, out totalCount);

    public void Set(object key, long totalCount, TimeSpan duration) =>
        _cache.Set(key, totalCount, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration,
            Size = 1
        });

    public void Dispose() => _cache.Dispose();

    /// <summary>Invalidates counts when the underlying dataset is explicitly reset.</summary>
    public void Clear() => _cache.Clear();
}
