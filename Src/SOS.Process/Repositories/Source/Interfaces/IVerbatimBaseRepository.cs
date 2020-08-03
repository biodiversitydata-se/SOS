using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;

namespace SOS.Process.Repositories.Source.Interfaces
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IVerbatimBaseRepository<TEntity, TKey> : IDisposable where TEntity : IEntity<TKey>
    {
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
        ///     Get all documents in collection.
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync();

        /// <summary>
        ///     Get all documents in collection.
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection);

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
        ///     Get min and max id in collection
        /// </summary>
        /// <returns></returns>
        Task<Tuple<TKey, TKey>> GetIdSpanAsync();

        /// <summary>
        ///     Get min and max id in collection
        /// </summary>
        /// <returns></returns>
        Task<Tuple<TKey, TKey>> GetIdSpanAsync(IMongoCollection<TEntity> mongoCollection);

        /// <summary>
        /// Set repository mode
        /// </summary>
        bool IncrementalMode { get; set; }
    }
}