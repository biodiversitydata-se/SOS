using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessedRepository: IDisposable 
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
        Task<bool> AddAsync(DarwinCore<DynamicProperties> entity);

        /// <summary>
        /// Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<DarwinCore<DynamicProperties>> items);

        /// <summary>
        /// Create search index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);

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
        Task<bool> DeleteManyAsync(IEnumerable<string> ids);

        /// <summary>
        /// Initialize repository
        /// </summary>
        /// <param name="databaseName"></param>
        void Initialize(string databaseName);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(string id, DarwinCore<DynamicProperties> entity);
    }
}
