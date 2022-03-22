using System;
using Microsoft.Extensions.Caching.Memory;
using SOS.Lib.Cache.Interfaces;

namespace SOS.Lib.Cache
{
    /// <inheritdoc />
    public class ClassCache<TClass> : IClassCache<TClass>
    {
        private static readonly object InitLock = new object();
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKey;

        /// <summary>
        /// Cache duration.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache"></param>
        public ClassCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheKey = typeof(TClass).Name;
        }

        /// <inheritdoc />
        public TClass Get()
        {
            _memoryCache.TryGetValue(_cacheKey, out var entity);
            return (TClass)entity;

        }
       

        /// <inheritdoc />
        public void Set(TClass entity)
        {
            lock (InitLock)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheDuration);
                _memoryCache.Set(_cacheKey, entity, cacheEntryOptions);
            }
        }
    }
}
