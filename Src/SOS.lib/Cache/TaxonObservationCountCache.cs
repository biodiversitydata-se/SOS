using System;
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
            .SetAbsoluteExpiration(TimeSpan.FromHours(4))
            .SetSize(1);
        private long _entriesCounter;
        private const int NumberOfEntriesCleanupLimit = 100000; // The cache will grow to approximately 200MB with limit=100000.

        public void Add(TaxonObservationCountCacheKey taxonObservationCountCacheKey, int count)
        {
            try
            {
                _memoryCache.Set(taxonObservationCountCacheKey, count, _cacheEntryOptions);
                _entriesCounter++;
                if (_entriesCounter > NumberOfEntriesCleanupLimit)
                {
                    _entriesCounter = 0;
                    _memoryCache.Compact(0.5);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool TryGetCount(TaxonObservationCountCacheKey taxonObservationCountCacheKey, out int count)
        {
            return _memoryCache.TryGetValue(taxonObservationCountCacheKey, out count);
        }
    }
}