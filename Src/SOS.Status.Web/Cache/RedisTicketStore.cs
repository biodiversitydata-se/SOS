using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace SOS.Status.Web.Cache;

public class RedisTicketStore : ITicketStore
{
    private readonly IDistributedCache _cache;
    private const string KeyPrefix = "adb:sos-status-tickets:";

    public RedisTicketStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(KeyPrefix + key);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await StoreAsync(key, ticket);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await _cache.GetAsync(KeyPrefix + key);
        return bytes == null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        await StoreAsync(key, ticket);
        return key;
    }

    private async Task StoreAsync(string key, AuthenticationTicket ticket)
    {
        var bytes = TicketSerializer.Default.Serialize(ticket);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8) // session lifetime
        };

        await _cache.SetAsync(KeyPrefix + key, bytes, options);
    }
}
