using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;

namespace SOS.Process.Repositories.Source.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IVerbatimBaseRepository<TEntity, TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Get cursor to all documents in collection
        /// </summary>
        /// <returns></returns>
        Task<IAsyncCursor<TEntity>> GetAllAsync();

        /// <summary>
        /// Get document batch
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId);

        /// <summary>
        /// Get entity batch by skip.
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchBySkipAsync(int skip);

        /// <summary>
        /// Get min and max id in collection
        /// </summary>
        /// <returns></returns>
        Task<Tuple<TKey, TKey>> GetIdSpanAsync();
    }
}
