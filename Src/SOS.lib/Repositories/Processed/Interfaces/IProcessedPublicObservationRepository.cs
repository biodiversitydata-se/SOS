using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessedPublicObservationRepository : IProcessedObservationRepositoryBase
    {
        /// <summary>
        /// Get observations by occurrence id
        /// </summary>
        /// <param name="occurrenceIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds);
    }
}