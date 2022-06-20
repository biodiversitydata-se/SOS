using SOS.Lib.Models.Interfaces;
using System;
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
        /// Return current instance
        /// </summary>
        byte CurrentInstance { get; }

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
    }
}