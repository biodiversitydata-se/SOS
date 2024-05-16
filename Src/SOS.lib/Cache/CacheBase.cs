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

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(1);

        private readonly string _cacheKey = "Cache";
        public event EventHandler CacheReleased;
        
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
                    var expirationToken = new CancellationChangeToken(
                        new CancellationTokenSource(CacheDuration).Token);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                        .AddExpirationToken(expirationToken)
                        .RegisterPostEvictionCallback(callback: OnCacheEviction, state: this);
                    MemoryCache.Set(_cacheKey, dictionary, cacheEntryOptions);
                    Logger.LogInformation($"Cache set. Key=\"{_cacheKey}\", Type={GetType().Name}");
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
            // Sometimes event is raised even if entity exists in cache.
            // In order to not bubble event when not needed, check if entity exists in cache
            if (CacheReleased == null || MemoryCache.TryGetValue(_cacheKey, out var entity))
            {
                return;
            }
            Logger.LogInformation($"Cache evicted. Key=\"{key}\", Type={GetType().Name}");
            CacheReleased.Invoke(this, EventArgs.Empty);
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
            Logger.LogInformation($"CacheBase.AddOrUpdateAsync(). Type={GetType().Name}");
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            var cache = GetCacheAsync().Result;
            cache.Clear();
            _initialized = false;
            Logger.LogInformation($"CacheBase.Clear(). Type={GetType().Name}");
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey key)
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
            Logger.LogInformation($"CacheBase.GetAsync updated cache for type={GetType().Name}, key={key}, Time elapsed={sp.ElapsedMilliseconds}ms");
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
                    Clear();
                    foreach (var entity in entities)
                    {
                        cache.TryAdd(entity.Id, entity);
                    }
                    _initialized = true;
                }
                sp.Stop();
                Logger.LogInformation($"CacheBase.GetAllAsync updated cache for type={GetType().Name}, Time elapsed={sp.ElapsedMilliseconds}ms");
            }

            return cache.Values;
        }
    }
}
