using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IDarwinCoreRepository : IProcessBaseRepository<DarwinCore<DynamicProperties>>
    {
        /// <summary>
        /// Create search index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        /// Toggle active instance
        /// </summary>
        /// <returns></returns>
        Task<bool> ToggleInstanceAsync();

        /// <summary>
        /// Delete provider data
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task<bool> DeleteProviderDataAsync(DataProviderId provider);
    }
}
