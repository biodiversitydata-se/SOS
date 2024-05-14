using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Cache object
        /// </summary>
        protected readonly ConcurrentDictionary<TKey, TEntity> Cache;

        /// <summary>
        /// Data repository
        /// </summary>
        protected readonly IRepositoryBase<TEntity, TKey> Repository;

        protected readonly ILogger<CacheBase<TKey, TEntity>> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="logger"></param>
        protected CacheBase(IRepositoryBase<TEntity, TKey> repository, ILogger<CacheBase<TKey, TEntity>> logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Cache = new ConcurrentDictionary<TKey, TEntity>();
            Logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity entity)
        {
            if (!await Repository.AddOrUpdateAsync(entity))
            {
                return false;
            }

            Cache.TryRemove(entity.Id, out var deletedEntity);
            Cache.TryAdd(entity.Id, entity);
            Logger.LogInformation($"CacheBase.AddOrUpdateAsync(). Type={GetType().Name}");
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            Cache.Clear();
            _initialized = false;
            Logger.LogInformation($"CacheBase.Clear(). Type={GetType().Name}");
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey key)
        {
            if (Cache.TryGetValue(key, out var entity))
            {
                return entity;
            }

            Stopwatch sp = Stopwatch.StartNew();
            entity = await Repository.GetAsync(key);
            if (entity != null)
            {
                Cache.TryAdd(key, entity);
            }
            sp.Stop();
            Logger.LogInformation($"CacheBase.GetAsync updated cache for type={GetType().Name}, key={key}, Time elapsed={sp.ElapsedMilliseconds}ms");
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            if (!_initialized || Cache.IsEmpty)
            {
                var entities = await Repository.GetAllAsync();

                if (entities != null)
                {
                    Clear();
                    foreach (var entity in entities)
                    {
                        Cache.TryAdd(entity.Id, entity);
                    }
                    _initialized = true;
                }
            }

            return Cache.Values;
        }
    }
}
