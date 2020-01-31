using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;

namespace SOS.Process.Repositories.Source.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IVerbatimBaseRepository<TEntity, in TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Get entity batch
        /// </summary>
        /// <param name="startId"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId);

        /// <summary>
        /// Get entity batch by skip.
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetBatchBySkipAsync(int skip);
    }
}
