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

        private ConcurrentDictionary<TaxonObservationCountCacheKey, TaxonCount> Cache
        {
            get
            {
                ConcurrentDictionary<TaxonObservationCountCacheKey, TaxonCount> dictionary;
                if (_memoryCache.TryGetValue(CacheKey, out dictionary))
                {
                    return dictionary;
                }

                dictionary = new ConcurrentDictionary<TaxonObservationCountCacheKey, TaxonCount>();
                _memoryCache.Set(CacheKey, dictionary, _cacheEntryOptions);
                return dictionary;
            }
        }

        public void Add(TaxonObservationCountCacheKey taxonObservationCountCacheKey, TaxonCount taxonCount)
        {
            lock(_lockObject)
            {
                _entriesCounter++;
                if (_entriesCounter > NumberOfEntriesCleanupLimit)
                {
                    _entriesCounter = 0;
                    CleanUp();
                }
                Cache.AddOrUpdate(taxonObservationCountCacheKey, taxonCount, (key, oldValue) => taxonCount);
            }
        }

        public bool TryGetCount(TaxonObservationCountCacheKey taxonObservationCountCacheKey, out TaxonCount taxonCount)
        {
            lock (_lockObject)
            {
                return Cache.TryGetValue(taxonObservationCountCacheKey, out taxonCount);
            }
        }

        private void CleanUp()
        {
            Cache.Clear();
        }
    }
}