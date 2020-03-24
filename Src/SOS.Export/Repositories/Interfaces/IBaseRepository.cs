using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IBaseRepository<TEntity, in TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Get active database instance
        /// </summary>
        byte ActiveInstance { get; }

        /// <summary>
        /// Get current collection name
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// Get entity
        /// </summary>
        /// <returns></returns>
        Task<TEntity> GetAsync(TKey id);

        /// <summary>
        /// Get all objects in repository
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync();
    }
}
