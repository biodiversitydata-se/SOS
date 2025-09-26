using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Cache base class
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class CacheBase<TKey, TEntity> : ICache<TKey, TEntity> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Flag indicating that the cache has been fully populated
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Data repository
        /// </summary>
        protected readonly IRepositoryBase<TEntity, TKey> Repository;
        protected readonly IMemoryCache MemoryCache;
        protected readonly ILogger<CacheBase<TKey, TEntity>> Logger;

        private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<TKey, TEntity> _cache = new ConcurrentDictionary<TKey, TEntity>();

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        private readonly string _cacheKey;

        protected CacheBase(IRepositoryBase<TEntity, TKey> repository, IMemoryCache memoryCache, ILogger<CacheBase<TKey, TEntity>> logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            MemoryCache = memoryCache;
            Logger = logger;
            _cacheKey = GetType().Name + "-" + Guid.NewGuid();
            Logger.LogInformation($"Cache created. Type={GetType().Name}");
        }

        private void OnCacheEviction(object key, object value, EvictionReason reason, object state)
        {
            Logger.LogInformation($"{GetType().Name}.OnCacheEviction() raised, Reason={reason}");
            _initialized = false; // force reload next time
        }

        public async Task<bool> AddOrUpdateAsync(TEntity entity)
        {
            // uppdatera i repository först
            if (!await Repository.AddOrUpdateAsync(entity))
                return false;

            // sen i cache
            _cache[entity.Id] = entity;
            Logger.LogInformation($"{GetType().Name}.AddOrUpdateAsync()");
            return true;
        }

        public void Clear()
        {
            _cache.Clear();
            _initialized = false;
            Logger.LogInformation($"{GetType().Name}.Clear()");
        }

        public async Task ClearAsync()
        {
            await Task.Run(() => Clear());
        }

        public virtual async Task<TEntity> GetAsync(TKey key)
        {
            if (_cache.TryGetValue(key, out var entity))
                return entity;

            Stopwatch sp = Stopwatch.StartNew();
            entity = await Repository.GetAsync(key);
            if (entity != null)
                _cache[key] = entity;

            sp.Stop();
            Logger.LogInformation($"{GetType().Name}.GetAsync() updated cache, Time elapsed={sp.ElapsedMilliseconds}ms");
            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            // Om cache redan finns och är initierad → returnera direkt
            if (_initialized && !_cache.IsEmpty)
            {
                Logger.LogTrace($"{GetType().Name}.GetAllAsync(). Already initialized. Count={_cache?.Count}.");
                return _cache.Values;
            }

            await _reloadSemaphore.WaitAsync();
            try
            {
                // dubbelkolla efter låset (någon annan kan ha hunnit ladda)
                if (_initialized && !_cache.IsEmpty)
                    return _cache.Values;

                Stopwatch sp = Stopwatch.StartNew();
                var entities = await Repository.GetAllAsync();
                Logger.LogInformation($"{GetType().Name}.GetAllAsync(). Repository.GetAllAsync().Count={entities?.Count}.");
                if (entities == null || entities.Count == 0)
                {
                    Logger.LogError($"{GetType().Name}.GetAllAsync() repository returned no entities, keeping old cache.");
                    return _cache.Values; // fallback
                }

                var newCache = new ConcurrentDictionary<TKey, TEntity>();
                foreach (var entity in entities)
                    newCache[entity.Id] = entity;

                // byt referens atomärt
                _cache = newCache;
                _initialized = true;
                RegisterInMemoryCache();

                sp.Stop();
                Logger.LogInformation($"CacheBase.GetAllAsync() updated cache for type={GetType().Name}, Count={_cache.Count}, Time={sp.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{GetType().Name}.GetAllAsync() failed, keeping old cache.");
            }
            finally
            {
                _reloadSemaphore.Release();
            }

            return _cache.Values;
        }

        protected ConcurrentDictionary<TKey, TEntity> GetCache()
        {
            if (_cache == null)
            {
                _cache = new ConcurrentDictionary<TKey, TEntity>();
                RegisterInMemoryCache();
            }
            return _cache;
        }

        private void RegisterInMemoryCache()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .RegisterPostEvictionCallback(OnCacheEviction, this);

            MemoryCache.Set(_cacheKey, _cache, cacheEntryOptions);
            Logger.LogInformation($"Cache registered in MemoryCache. Type={GetType().Name}");
        }
    }
}
