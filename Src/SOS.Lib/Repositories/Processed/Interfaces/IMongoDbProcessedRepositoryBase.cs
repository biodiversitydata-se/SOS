﻿using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IMongoDbProcessedRepositoryBase<TEntity, in TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        ///     Add one item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity item);

        /// <summary>
        ///     Add one item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        ///     Add collection if not exists
        /// </summary>
        /// <returns></returns>
        Task<bool> AddCollectionAsync();

        /// <summary>
        ///     Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items);

        /// <summary>
        ///     Add or update item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(TKey id, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        ///     Remove collection
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        ///     Get cursor to all documents in collection
        /// </summary>
        /// <returns></returns>
        Task<IAsyncCursor<TEntity>> GetAllByCursorAsync();

        /// <summary>
        ///     Get cursor to all documents in collection
        /// </summary>
        /// <returns></returns>
        Task<IAsyncCursor<TEntity>> GetAllByCursorAsync(IMongoCollection<TEntity> mongoCollection,
            bool noCursorTimeout = false);

        /// <summary>
        ///     Get all items.
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        ///     Get item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey id);

        /// <summary>
        ///     Make sure collection exists
        /// </summary>
        /// <returns>true if new collection was created</returns>
        Task<bool> VerifyCollectionAsync();

        /// <summary>
        ///     Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> UpdateAsync(TKey id, TEntity entity);

        /// <summary>
        ///     Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(TKey id, TEntity entity, IMongoCollection<TEntity> mongoCollection);
    }
}