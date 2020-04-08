using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;

namespace SOS.Import.Repositories.Destination.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IVerbatimRepository<TEntity, in TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Add collection if not exists
        /// </summary>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        Task<bool> AddCollectionAsync();

        /// <summary>
        /// Add collection if not exists
        /// </summary>
        /// <returns></returns>
        Task<bool> AddCollectionAsync(string collectionName);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity entity);

        /// <summary>
        /// Add entity
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items);

        /// <summary>
        /// Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Add or update existing entity
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item);

        /// <summary>
        /// Add or update existing entity
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(TKey id, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Remove collection
        /// </summary>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        /// Remove collection
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync(string collectionName);

        /// <summary>
        /// Delete many
        /// </summary>
        /// <param name="ids"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids);

        /// <summary>
        /// Delete many
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Get entity batch
        /// </summary>
        /// <param name="skip"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(int skip);

        /// <summary>
        /// Get entity batch
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(int skip, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> UpdateAsync(TKey id, TEntity entity);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(TKey id, TEntity entity, IMongoCollection<TEntity> mongoCollection);
    }
}
