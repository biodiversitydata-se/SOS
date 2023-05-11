namespace SOS.UserStatistics.Api.Cache.Managers.Interfaces;

public interface IUserStatisticsCacheManager
{
    Cache<TKey, TValue> GetCache<TKey, TValue>(string cacheKey);
    void CheckCleanup<TKey, TValue>(string cacheKey);
    void ClearCache<TKey, TValue>(string cacheKey);
    void ClearAllCache();
}
