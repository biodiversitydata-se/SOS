using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;

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


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository"></param>
        protected CacheBase(IRepositoryBase<TEntity, TKey> repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Cache = new ConcurrentDictionary<TKey, TEntity>();
        }

        /// <inheritdoc />
        public void Clear()
        {
            Cache.Clear();
            _initialized = false;
        }

        /// <inheritdoc />
        public async Task<TEntity> GetAsync(TKey key)
        {
            if (Cache.TryGetValue(key, out var value))
            {
                return value;
            }

            var entity = await Repository.GetAsync(key);

            if (entity != null)
            {
                Cache.TryAdd(key, entity);
            }

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
