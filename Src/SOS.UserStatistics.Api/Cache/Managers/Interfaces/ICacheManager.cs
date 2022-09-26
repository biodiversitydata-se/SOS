namespace SOS.UserStatistics.Api.Cache.Managers.Interfaces;

public interface ICacheManager<TKey, TValue>
{
    Cache<TKey, TValue> GetCache();
    void CheckCleanup();
    void ClearCache();
}