using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
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
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        protected readonly ILogger<CacheBase<TKey, TEntity>> Logger;

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        private string _cacheKey = "Cache";
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        protected CacheBase(IRepositoryBase<TEntity, TKey> repository, IMemoryCache memoryCache, ILogger<CacheBase<TKey, TEntity>> logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            MemoryCache = memoryCache;
            Logger = logger;
            _cacheKey = GetType().Name + "-" + Guid.NewGuid().ToString();
            Logger.LogInformation($"Cache created. Type={GetType().Name}");
        }

        protected async Task<ConcurrentDictionary<TKey, TEntity>> GetCacheAsync()
        {
            ConcurrentDictionary<TKey, TEntity> dictionary;
            await semaphore.WaitAsync();
            try
            {
                MemoryCache.TryGetValue(_cacheKey, out dictionary);
                if (dictionary == null)
                {
                    dictionary = new ConcurrentDictionary<TKey, TEntity>();                    
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(CacheDuration)                        
                        .RegisterPostEvictionCallback(callback: OnCacheEviction, state: this);
                    MemoryCache.Set(_cacheKey, dictionary, cacheEntryOptions);
                    Logger.LogInformation($"Cache set. Type={GetType().Name}");
                }
            }            
            finally
            {
                semaphore.Release();
            }

            return dictionary;
        }

        private void OnCacheEviction(object key, object value, EvictionReason reason, object state)
        {
            Logger.LogDebug($"{GetType().Name}.OnCacheEviction() raised, Reason={reason}");            
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity entity)
        {
            var cache = await GetCacheAsync();
            if (!await Repository.AddOrUpdateAsync(entity))
            {
                return false;
            }

            cache.TryRemove(entity.Id, out var deletedEntity);
            cache.TryAdd(entity.Id, entity);
            Logger.LogDebug($"{GetType().Name}.AddOrUpdateAsync()");
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            var cache = GetCacheAsync().Result;
            cache.Clear();
            _initialized = false;
            Logger.LogDebug($"{GetType().Name}.Clear()");
        }

        public async Task ClearAsync()
        {
            var cache = await GetCacheAsync();
            cache.Clear();
            _initialized = false;
            Logger.LogDebug($"{GetType().Name}.ClearAsync()");
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TKey key)
        {
            var cache = await GetCacheAsync();
            if (cache.TryGetValue(key, out var entity))
            {
                return entity;
            }

            Stopwatch sp = Stopwatch.StartNew();
            entity = await Repository.GetAsync(key);
            if (entity != null)
            {
                cache.TryAdd(key, entity);
            }
            sp.Stop();
            Logger.LogDebug($"{GetType().Name}.GetAsync() updated cache, Time elapsed={sp.ElapsedMilliseconds}ms");
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var cache = await GetCacheAsync();

            if (!_initialized || cache.IsEmpty)
            {
                Stopwatch sp = Stopwatch.StartNew();
                var entities = await Repository.GetAllAsync();

                if (entities != null)
                {
                    await ClearAsync();
                    foreach (var entity in entities)
                    {
                        cache.TryAdd(entity.Id, entity);
                    }
                    _initialized = true;
                }
                sp.Stop();
                Logger.LogDebug($"CacheBase.GetAllAsync updated cache for type={GetType().Name}, Time elapsed={sp.ElapsedMilliseconds}ms");
            }

            return cache.Values;
        }
    }
}
