using System;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessRepositoryBase<TEntity> : IDisposable
    {
        /// <summary>
        /// Batch size
        /// </summary>
        int BatchSize { get; }

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
        /// Get name of index
        /// </summary>
        /// <param name="instace"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        string GetIndexName(byte instace, bool protectedObservations = false);

        /// <summary>
        /// Run mode
        /// </summary>
        bool LiveMode { get; set; }

        /// <summary>
        /// Set active instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);

}
}