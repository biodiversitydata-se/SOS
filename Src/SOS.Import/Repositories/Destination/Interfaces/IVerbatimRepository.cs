using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// <returns></returns>
        Task<bool> AddCollectionAsync();

        /// <summary>
        /// Add entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity entity);

        /// <summary>
        /// Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<TEntity> items);

        /// <summary>
        /// Add or update existing entity
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> AddOrUpdateAsync(TEntity item);

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        /// Remove collection
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        /// Delete many
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(IEnumerable<TKey> ids);

        /// <summary>
        /// Get entity batch
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(int skip);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(TKey id, TEntity entity);
    }
}
