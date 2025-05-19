using SOS.Lib.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessRepositoryBase<TEntity, TKey> : IDisposable where TEntity : IEntity<TKey>
    {
        /// <summary>
        ///     Get 0 or 1 depending of witch instance to update
        /// </summary>
        byte ActiveInstance { get; }

        /// <summary>
        /// Add many documents to index
        /// </summary>
        /// <param name="items"></param>
        /// <param name="refreshIndex"></param>
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<TEntity> items, bool refreshIndex = false);

        /// <summary>
        /// Return current instance
        /// </summary>
        byte CurrentInstance { get; }
        
        /// <summary>
        /// Clear configuration cache
        /// </summary>
        /// <returns></returns>
        Task ClearConfigurationCacheAsync();

        /// <summary>
        /// Delete alla documents in a Index
        /// </summary>
        /// <param name="waitForCompletion"></param>
        /// <returns></returns>
        Task<bool> DeleteAllDocumentsAsync(bool waitForCompletion = false);

        /// <summary>
        /// Delete a collection
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync();

        /// <summary>
        /// Disable indexing
        /// </summary>
        /// <returns></returns>
        Task<bool> DisableIndexingAsync();

        /// <summary>
        /// Enable indexing
        /// </summary>
        /// <returns></returns>
        Task EnableIndexingAsync();

        /// <summary>
        /// Get free disk space
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, int>> GetDiskUsageAsync();

        /// <summary>
        /// Count documents in index
        /// </summary>
        /// <returns></returns>
        Task<long> IndexCountAsync();

        /// <summary>
        ///     Get 0 or 1 depending of witch instance to update
        /// </summary>
        byte InActiveInstance { get; }

        /// <summary>
        /// Run mode
        /// </summary>
        bool LiveMode { get; set; }

        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

        /// <summary>
        /// Batch size used for read
        /// </summary>
        int ReadBatchSize { get; }

        /// <summary>
        /// Set active instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);

        /// <summary>
        /// Batch size used for write
        /// </summary>
        int WriteBatchSize { get; }

        /// <summary>
        /// Get all records.
        /// </summary>
        /// <param name="take">The max number of records to get.</param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(int take = 10000);

        /// <summary>
        /// Get field mapping
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetMappingAsync();
    }
}