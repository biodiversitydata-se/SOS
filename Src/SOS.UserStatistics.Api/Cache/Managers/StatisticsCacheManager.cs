namespace SOS.UserStatistics.Api.Cache.Managers;

public class StatisticsCacheManager : IStatisticsCacheManager
{
    private readonly static MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    readonly static List<string> _cacheKeys = new();
    private readonly MemoryCacheEntryOptions _cacheEntryoptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
    private const int NumberOfEntriesCleanupLimit = 1000;

    public Cache<TKey, TValue> GetCache<TKey, TValue>(string cacheKey)
    {
        if (_memoryCache.TryGetValue(cacheKey, out Cache<TKey, TValue> cache))
        {
            return cache;
        }
        return CreateCache<TKey, TValue>(cacheKey);
    }

    public void CheckCleanup<TKey, TValue>(string cacheKey)
    {
        var cache = GetCache<TKey, TValue>(cacheKey);
        if (cache.NrOfItems() > NumberOfEntriesCleanupLimit)
        {
            ClearCache<TKey, TValue>(cacheKey);
        }
    }

    public void ClearCache<TKey, TValue>(string cacheKey)
    {
        CreateCache<TKey, TValue>(cacheKey);
    }

    public void ClearAllCache()
    {
        foreach (var key in _cacheKeys)
        {
            _memoryCache.Remove(key);
        }
    }

    private Cache<TKey, TValue> CreateCache<TKey, TValue>(string cacheKey)
    {
        var cache = new Cache<TKey, TValue>();
        var cacheEntryOptions = _cacheEntryoptions;
        if (!_cacheKeys.Contains(cacheKey))
        {
            _cacheKeys.Add(cacheKey);
        }
        else
        {
            throw new Exception("CacheKey already exists");
        }
        _memoryCache.Set(cacheKey, cache, cacheEntryOptions);
        return cache;
    }
}