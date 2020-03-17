using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, ObjectId>
    {
        /// <summary>
        /// Get filtered chunk
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedObservation>> GetChunkAsync(FilterBase filter, int skip, int take);

        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedProject>> GetProjectParameters(FilterBase filter, int skip, int take);
    }
}
