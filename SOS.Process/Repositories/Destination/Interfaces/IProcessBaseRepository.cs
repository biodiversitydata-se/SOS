using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessBaseRepository<T>: IDisposable 
    {
        /// <summary>
        /// Add collection if not exists
        /// </summary>
        /// <returns></returns>
        Task<bool> AddCollectionAsync();

        /// <summary>
        /// Add many items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<T> items);

        /// <summary>
        /// Remove collection
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        /// Make sure collection exists
        /// </summary>
        Task VerifyCollectionAsync();
    }
}
