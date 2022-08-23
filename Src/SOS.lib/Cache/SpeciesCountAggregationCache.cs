using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver.Linq;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Statistics;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Species count aggregation cache.
    /// </summary>
    public class SpeciesCountAggregationCacheManager
    {
        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(4));
        private const int NumberOfEntriesCleanupLimit = 1000; // The cache will grow to approximately 250MB with limit=2000000.
        private const string CacheKey = "SpeciesCountAggregationCache";
        private readonly object _lockObject = new object();

        public SpeciesCountAggregationCache GetCache()
        {
            if (_memoryCache.TryGetValue(CacheKey, out SpeciesCountAggregationCache cache))
            {
                return cache;
            }
            
            return CreateCache();
        }

        public void CheckCleanUp()
        {
            var cache = GetCache();
            if (cache.UserStatisticsItemsCache.Count > NumberOfEntriesCleanupLimit)
            {
                ClearCache();
            }
        }

        public void ClearCache()
        {
            CreateCache();
        }

        private SpeciesCountAggregationCache CreateCache()
        {
            var cache = new SpeciesCountAggregationCache();
            var cacheEntryOptions = _cacheEntryOptions;
            _memoryCache.Set(CacheKey, cache, cacheEntryOptions);
            return cache;
        }
    }

    public class SpeciesCountAggregationCache
    {
        public ConcurrentDictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> UserStatisticsItemsCache = new();
        public ConcurrentDictionary<SpeciesCountUserStatisticsQuery, Dictionary<int, UserStatisticsItem>> UserStatisticsByUserIdCache = new();
    }
}