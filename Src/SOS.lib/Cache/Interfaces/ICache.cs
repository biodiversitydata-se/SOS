using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// Cache interface
    /// </summary>
    public interface ICache<TKey, TEntity>
    {
        /// <summary>
        /// Clear current cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Try to get cached entity
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey key);

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync();
    }
}
