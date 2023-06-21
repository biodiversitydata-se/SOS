using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Repositories.Interfaces
{
    /// <summary>
    /// Repository base
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IRepositoryBase<TEntity, TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        ///     Add entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity entity);

        /// <summary>
        ///     Add entity
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        ///     Add collection if not exists
        /// </summary>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        Task<bool> AddCollectionAsync();

        /// <summary>
        ///     Add collection if not exists
        /// </summary>
        /// <returns></returns>
        Task<bool> AddCollectionAsync(string collectionName);

        /// <summary>
        /// Create indexes
        /// </summary>
        /// <param name="indexModels"></param>
        /// <returns></returns>
        Task AddIndexes(IEnumerable<CreateIndexModel<TEntity>> indexModels);

        /// <summary>
        ///     Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items);

        /// <summary>
        ///     Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection);


        /// <summary>
        ///     Add or update existing entity
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item);

        /// <summary>
        ///     Add or update existing entity
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Batch size when read
        /// </summary>
        int BatchSizeRead { get; set; }

        /// <summary>
        /// Batch size when write
        /// </summary>
        int BatchSizeWrite { get; set; }

        /// <summary>
        ///     Checks if the collection exists.
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckIfCollectionExistsAsync();

        /// <summary>
        ///     Checks if the specified collection exists.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        Task<bool> CheckIfCollectionExistsAsync(string collectionName);

        /// <summary>
        /// Count the number of documents in the collection.
        /// </summary>
        /// <returns></returns>
        Task<long> CountAllDocumentsAsync();

        /// <summary>
        /// Count the number of documents in the collection.
        /// </summary>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<long> CountAllDocumentsAsync(IMongoCollection<TEntity> mongoCollection);

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
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        ///     Remove collection
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync(string collectionName);

        /// <summary>
        ///     Delete many
        /// </summary>
        /// <param name="ids"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids);

        /// <summary>
        ///     Delete many
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Get item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey id);

        /// <summary>
        /// Get multiple items by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TKey> ids);

        /// <summary>
        ///     Get entity batch
        /// </summary>
        /// <param name="skip"></param>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(int skip);

        /// <summary>
        ///     Get entity batch
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(int skip, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        ///     Get all entities
        /// </summary>
        /// <remarks>Uses typeof(TEntity).Name as MongoDb collection name.</remarks>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        ///     Get all entities
        /// </summary>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection);

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
        /// Retrieves all documents from the repository Mongo collection with the specified projection.
        /// </summary>
        /// <typeparam name="TProjection">The type to which the documents are projected.</typeparam>        
        /// <param name="projectionDefinition">The projection definition used to project the documents.</param>
        /// <param name="noCursorTimeout">A flag indicating whether the cursor should have a timeout.</param>
        /// <returns></returns>
        Task<List<TProjection>> GetAllAsync<TProjection>(ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false);

        /// <summary>
        /// Retrieves all documents from a Mongo collection with the specified projection.
        /// </summary>
        /// <typeparam name="TProjection">The type to which the documents are projected.</typeparam>
        /// <param name="mongoCollection">The Mongo collection to retrieve documents from.</param>
        /// <param name="projectionDefinition">The projection definition used to project the documents.</param>
        /// <param name="noCursorTimeout">A flag indicating whether the cursor should have a timeout.</param>
        /// <returns></returns>
        Task<List<TProjection>> GetAllAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false);

        /// <summary>
        /// Retrieves all documents from the repository Mongo collection as an asynchronous streaming cursor with the specified projection.
        /// </summary>
        /// <typeparam name="TProjection">The type to which the documents are projected.</typeparam>        
        /// <param name="projectionDefinition">The projection definition used to project the documents.</param>
        /// <param name="noCursorTimeout">A flag indicating whether the cursor should have a timeout.</param>
        /// <returns>An asynchronous streaming cursor representing the result of the search.</returns>
        Task<IAsyncCursor<TProjection>> GetAllByCursorAsync<TProjection>(ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false);

        /// <summary>
        /// Retrieves all documents from a Mongo collection as an asynchronous streaming cursor with the specified projection.
        /// </summary>
        /// <typeparam name="TProjection">The type to which the documents are projected.</typeparam>
        /// <param name="mongoCollection">The Mongo collection to retrieve documents from.</param>
        /// <param name="projectionDefinition">The projection definition used to project the documents.</param>
        /// <param name="noCursorTimeout">A flag indicating whether the cursor should have a timeout.</param>
        /// <returns>An asynchronous streaming cursor representing the result of the search.</returns>
        Task<IAsyncCursor<TProjection>> GetAllByCursorAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false);

        /// <summary>
        /// Retrieves documents from a Mongo collection using a filter and returns an asynchronous streaming cursor with the specified projection.
        /// </summary>
        /// <typeparam name="TProjection">The type to which the documents are projected.</typeparam>
        /// <param name="mongoCollection">The Mongo collection to retrieve documents from.</param>
        /// <param name="filterDefinition">The filter definition used to filter the documents.</param>
        /// <param name="projectionDefinition">The projection definition used to project the documents.</param>
        /// <param name="noCursorTimeout">A flag indicating whether the cursor should have a timeout.</param>
        /// <returns>An asynchronous streaming cursor representing the result of the search.</returns>
        Task<IAsyncCursor<TProjection>> GetByCursorAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            FilterDefinition<TEntity> filterDefinition,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false);

        /// <summary>
        ///     Get document batch
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId);

        /// <summary>
        ///     Get document batch
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="mongoCollection"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId, IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        ///     Get  max id in collection
        /// </summary>
        /// <returns></returns>
        Task<TKey> GetMaxIdAsync();

        /// <summary>
        ///     Get max id in collection
        /// </summary>
        /// <returns></returns>
        Task<TKey> GetMaxIdAsync(IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Query collection
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> QueryAsync(FilterDefinition<TEntity> filter);

        /// <summary>
        /// Set run mode
        /// </summary>
        JobRunModes Mode { get; set; }

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
