using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessedProtectedObservationRepository : IProcessedObservationRepositoryBase
    {
        /// <summary>
        /// Get occurrences id's
        /// </summary>
        /// <param name="noOfOccurrences"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetOccurrenceIdsAsync(int noOfOccurrences);
    }
}