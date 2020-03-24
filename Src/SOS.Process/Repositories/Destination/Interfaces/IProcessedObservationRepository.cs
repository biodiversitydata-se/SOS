using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessedObservationRepository : IProcessBaseRepository<ProcessedObservation, ObjectId>
    {
        /// <summary>
        /// Add many items 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        new Task<int> AddManyAsync(IEnumerable<ProcessedObservation> items);

        /// <summary>
        /// Copy provider data from active instance to inactive instance
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task<bool> CopyProviderDataAsync(ObservationProvider provider);

        /// <summary>
        /// Create search index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();

        /// <summary>
        /// Delete provider data
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task<bool> DeleteProviderDataAsync(ObservationProvider provider);

        /// <summary>
        /// Drop current indexes
        /// </summary>
        /// <returns></returns>
        Task DropIndexAsync();
    }
}
