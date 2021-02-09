using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IMongoDbProcessedRepositoryBase<TEntity, in TKey> : IProcessRepositoryBase<TEntity> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Name of active instance
        /// </summary>
        string ActiveInstanceName { get; }

        /// <summary>
        /// Current instance
        /// </summary>
        string CurrentInstanceName { get; }

        /// <summary>
        /// Name of inactive instance
        /// </summary>
        string InactiveInstanceName { get; }

        /// <summary>
        /// Collection contains protected data
        /// </summary>
        bool Protected { get; set; }

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
        ///     Set active instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);

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