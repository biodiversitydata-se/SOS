using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessedPublicObservationRepository : IProcessedObservationRepositoryBase
    {
        /// <summary>
        /// Make sure that no observation with passed occurrence id's exists in index
        /// </summary>
        /// <param name="occurrenceIds"></param>
        /// <returns></returns>
        Task<bool?> CheckAbsenceByOccurrenceIdAsync(IEnumerable<string> occurrenceIds);
    }
}