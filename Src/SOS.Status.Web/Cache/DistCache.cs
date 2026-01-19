using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SOS.Status.Web.Cache;
public interface IDistCache
{
    public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> funcDel, TimeSpan? absoluteExpirationRelativeToNow);
}
public class DistCache : IDistCache
{
    const string REDIS_PREFIX = "adb:sos-status:";
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistCache> _logger;

    public DistCache(IDistributedCache distributedCache, ILogger<DistCache> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }
    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> funcDel, TimeSpan? absoluteExpirationRelativeToNow)
    {
        var redisKey = $"{REDIS_PREFIX}{key}";
        try
        {
            var jsonResult = await _distributedCache.GetStringAsync(redisKey);
            if (jsonResult is null)
            {
                var result = await funcDel();
                await _distributedCache.SetStringAsync(redisKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow });
                return result;
            }
            return JsonSerializer.Deserialize<T>(jsonResult) ?? throw new ArgumentNullException(nameof(jsonResult));
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"REDIS FAILURE: GetOrAddAsync() failed for key: {redisKey}");
            return await funcDel();
        }
    }
}
