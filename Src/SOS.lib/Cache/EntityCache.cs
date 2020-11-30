using System;
using Microsoft.Extensions.Caching.Memory;
using SOS.Lib.Cache.Interfaces;

namespace SOS.Lib.Cache
{
    /// <inheritdoc />
    public class EntityCache<TEntity> : IEntityCache<TEntity>
    {
        private static readonly object InitLock = new object();
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKey;
        private TEntity _entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache"></param>
        public EntityCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _cacheKey = typeof(TEntity).Name;
        }

        /// <inheritdoc />
        public TEntity Get()
        {
            _memoryCache.TryGetValue(_cacheKey, out var entity);
            return (TEntity)entity;

        }
       

        /// <inheritdoc />
        public void Set(TEntity entity)
        {
            lock (InitLock)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                _memoryCache.Set(_cacheKey, entity, cacheEntryOptions);
            }
        }
    }
}
