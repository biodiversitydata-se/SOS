using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessedProtectedObservationRepository : IProcessedObservationRepositoryBase
    {
        /// <summary>
        ///  Get random observations
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<Observation>> GetRandomObservationsAsync(int take);
    }
}