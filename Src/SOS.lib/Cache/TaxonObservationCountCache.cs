using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Cache;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Taxon observation count cache.
    /// </summary>
    public class TaxonObservationCountCache : ITaxonObservationCountCache
    {
        private readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(4));
        private long _entriesCounter;
        private const int NumberOfEntriesCleanupLimit = 2000000; // The cache will grow to approximately 250MB with limit=2000000.
        private const string CacheKey = "TaxonObservationCountCache";
        private readonly object _lockObject = new object();

        private ConcurrentDictionary<TaxonObservationCountCacheKey, int> Cache
        {
            get
            {
                ConcurrentDictionary<TaxonObservationCountCacheKey, int> dictionary;
                if (_memoryCache.TryGetValue(CacheKey, out dictionary))
                {
                    return dictionary;
                }

                dictionary = new ConcurrentDictionary<TaxonObservationCountCacheKey, int>();
                _memoryCache.Set(CacheKey, dictionary, _cacheEntryOptions);
                return dictionary;
            }
        }

        public void Add(TaxonObservationCountCacheKey taxonObservationCountCacheKey, int count)
        {
            lock(_lockObject)
            {
                _entriesCounter++;
                if (_entriesCounter > NumberOfEntriesCleanupLimit)
                {
                    _entriesCounter = 0;
                    CleanUp();
                }
                Cache.AddOrUpdate(taxonObservationCountCacheKey, count, (key, oldValue) => count);
            }
        }

        public bool TryGetCount(TaxonObservationCountCacheKey taxonObservationCountCacheKey, out int count)
        {
            lock (_lockObject)
            {
                return Cache.TryGetValue(taxonObservationCountCacheKey, out count);
            }
        }

        private void CleanUp()
        {
            Cache.Clear();
        }
    }
}