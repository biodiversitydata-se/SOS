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
    public abstract class CacheBase<TKey, TEntity> : ICache<TKey, TEntity> where TEntity : IEntity<TKey>
    {
        public abstract TimeSpan CacheDuration { get; set; }

        /// <summary>
        /// Flag indicating that the cache has been fully populated
        /// </summary>
        private bool _initialized;
        protected readonly IRepositoryBase<TEntity, TKey> Repository;
        private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly Timer _reloadTimer;
        private ConcurrentDictionary<TKey, TEntity> _cache = new ConcurrentDictionary<TKey, TEntity>();                
        protected readonly ILogger<CacheBase<TKey, TEntity>> Logger;

        protected CacheBase(IRepositoryBase<TEntity, TKey> repository, ILogger<CacheBase<TKey, TEntity>> logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));            
            Logger = logger;
            _reloadTimer = new Timer(_ => OnCacheEviction(), null, CacheDuration, CacheDuration);
            Logger.LogInformation($"Cache created. Type={GetType().Name}");
        }

        private void OnCacheEviction()
        {
            Logger.LogInformation($"{GetType().Name}.OnCacheEviction() raised");
            _initialized = false; // force reload next time
        }

        private async Task InitializeCacheAsync()
        {
            await _reloadSemaphore.WaitAsync();
            try
            {
                _cache.Clear();
                _initialized = true;
            }
            finally
            {
                _reloadSemaphore.Release();
            }

            Logger.LogTrace($"{GetType().Name} cache initialized");
        }

        public async Task<bool> AddOrUpdateAsync(TEntity entity)
        {
            if (!_initialized)
            {
                await InitializeCacheAsync();
            }

            // Update in repository first
            if (!await Repository.AddOrUpdateAsync(entity))
                return false;

            // Then in cache
            _cache[entity.Id] = entity;
            Logger.LogDebug($"{GetType().Name}.AddOrUpdateAsync()");
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
            if (!_initialized)
            {
                await InitializeCacheAsync();               
            }
                        
            if (_cache.TryGetValue(key, out var cachedEntity))
                return cachedEntity;
  
            Stopwatch sp = Stopwatch.StartNew();
            var entity = await Repository.GetAsync(key);
            if (entity != null)
                _cache[key] = entity;

            sp.Stop();
            Logger.LogDebug($"{GetType().Name}.GetAsync() updated cache, Time elapsed={sp.ElapsedMilliseconds}ms");
            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            // If cache already initialized → return directly
            if (_initialized && !_cache.IsEmpty)
            {
                Logger.LogTrace($"{GetType().Name}.GetAllAsync(). Already initialized. Count={_cache?.Count}.");
                return _cache.Values;
            }

            await _reloadSemaphore.WaitAsync();
            try
            {
                // Check again inside the lock (some other thread may have loaded it)
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

                // Change reference atomically
                _cache = newCache;
                _initialized = true;                

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
            }

            return _cache;
        }
    }
}
