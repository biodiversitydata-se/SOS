using System;
using System.Threading;
using Microsoft.Extensions.Primitives;
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

        private void OnCacheEviction(object key, object value, EvictionReason reason, object state)
        {
            // Sometimes event is raised even if entity exists in cache.
            // In order to not bubble event when not needed, check if entity exists in cache
            if (CacheReleased == null || _memoryCache.TryGetValue(_cacheKey, out var entity))
            {
                return;
            }

            CacheReleased.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache"></param>
        public ClassCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheKey = typeof(TClass).Name;
        }

        /// <summary>
        /// Cache duration.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <inheritdoc />
        public event EventHandler CacheReleased;

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
                var expirationToken = new CancellationChangeToken(
                    new CancellationTokenSource(CacheDuration).Token);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove)
                    .AddExpirationToken(expirationToken)
                    .RegisterPostEvictionCallback(callback: OnCacheEviction, state: this);
    ;
                _memoryCache.Set(_cacheKey, entity, cacheEntryOptions);
            }
        }
    }
}
