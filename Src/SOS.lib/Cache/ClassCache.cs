using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SOS.Lib.Cache.Interfaces;
using System;
using System.Threading;

namespace SOS.Lib.Cache
{
    /// <inheritdoc />
    public class ClassCache<TClass> : IClassCache<TClass>
    {
        private static readonly object InitLock = new object();
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKey;
        protected readonly ILogger Logger;           

        private void OnCacheEviction(object key, object value, EvictionReason reason, object state)
        {
            // Sometimes event is raised even if entity exists in cache.
            // In order to not bubble event when not needed, check if entity exists in cache
            if (CacheReleased == null || _memoryCache.TryGetValue(_cacheKey, out var entity))
            {
                return;
            }
            Logger.LogInformation($"Cache evicted. Key=\"{key}\"");
            CacheReleased.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache"></param>
        public ClassCache(IMemoryCache memoryCache, ILogger logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheKey = typeof(TClass).Name;
            Logger = logger;
        }

        /// <summary>
        /// Cache duration.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(1);

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
                _memoryCache.Set(_cacheKey, entity, cacheEntryOptions);
                Logger.LogInformation($"Cache set. Key=\"{_cacheKey}\"");
            }
        }
    }
}
