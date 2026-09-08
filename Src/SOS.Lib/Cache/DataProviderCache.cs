using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Xml.Linq;

namespace SOS.Lib.Cache;

/// <summary>
/// Data provider cache
/// </summary>
public class DataProviderCache : CacheBase<int, DataProvider>, IDataProviderCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _emlLocks = new();
    private static readonly TimeSpan EmlCacheDuration = TimeSpan.FromHours(1);

    public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataProviderRepository"></param>        
    /// <param name="logger"></param>
    /// <param name="memoryCache"></param>
    public DataProviderCache(IDataProviderRepository dataProviderRepository, ILogger<CacheBase<int, DataProvider>> logger,
        IMemoryCache memoryCache) : base(dataProviderRepository, logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<IEnumerable<int>> GetDefaultIdsAsync()
    {
        var allProviders = await GetAllAsync();
        return allProviders?.Where(p => p.IsActive && p.IncludeInSearchByDefault).Select(p => p.Id).ToArray(); 
    }

    /// <inheritdoc />
    public async Task<XDocument> GetEmlAsync(int providerId)
    {
        var key = (typeof(DataProviderCache), "Eml", providerId);
        var semaphore = _emlLocks.GetOrAdd(providerId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            if (!_memoryCache.TryGetValue(key, out XDocument eml))
            {
                eml = await ((IDataProviderRepository)Repository).GetEmlAsync(providerId);
                // The repository also returns null on read failures; allow the next call to retry.
                if (eml == null) return null;
                eml = new XDocument(eml);
                _memoryCache.Set(key, eml, EmlCacheDuration);
            }

            // Consumers may edit the document before saving it.
            return new XDocument(eml);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> StoreEmlAsync(int providerId, XDocument eml)
    {
        var semaphore = _emlLocks.GetOrAdd(providerId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            var stored = await ((IDataProviderRepository)Repository).StoreEmlAsync(providerId, eml);
            if (stored)
                _memoryCache.Remove((typeof(DataProviderCache), "Eml", providerId));
            return stored;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
