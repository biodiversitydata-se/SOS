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
        ///     Get 0 or 1 depending of witch instance to update
        /// </summary>
        byte InActiveInstance { get; }

        /// <summary>
        /// Name of active instance
        /// </summary>
        string ActiveInstanceName { get; }

        /// <summary>
        /// Name of inactive instance
        /// </summary>
        string InactiveInstanceName { get; }

        /// <summary>
        /// Current instance
        /// </summary>
        string CurrentInstanceName { get; }

        /// <summary>
        /// Run mode
        /// </summary>
        bool LiveMode { get; set; }

        /// <summary>
        /// True if repository is handling protected data
        /// </summary>
        bool Protected{ get; set; }

        /// <summary>
        /// Set active instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);

}
}