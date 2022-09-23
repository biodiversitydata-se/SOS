namespace SOS.UserStatistics.Api.Cache.Managers;

public class CacheManager<TKey, TValue> : ICacheManager<TKey, TValue>
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly MemoryCacheEntryOptions _cacheEntryoptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
    private const int NumberOfEntriesCleanupLimit = 1000;
    private readonly string _cacheKey;

    public CacheManager(string cacheKey)
    {
        _cacheKey = cacheKey;
    }

    public Cache<TKey, TValue> GetCache()
    {
        if (_memoryCache.TryGetValue(_cacheKey, out Cache<TKey, TValue> cache))
        {
            return cache;
        }
        return CreateCache();
    }

    public void CheckCleanup()
    {
        var cache = GetCache();
        if (cache.NrOfItems() > NumberOfEntriesCleanupLimit)
        {
            ClearCache();
        }
    }

    public void ClearCache()
    {
        CreateCache();
    }

    private Cache<TKey, TValue> CreateCache()
    {
        var cache = new Cache<TKey, TValue>();
        var cacheEntryOptions = _cacheEntryoptions;
        _memoryCache.Set(_cacheKey, cache, cacheEntryOptions);
        return cache;
    }
}