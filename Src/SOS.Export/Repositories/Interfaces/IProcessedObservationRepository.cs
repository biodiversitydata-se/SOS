using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, string>
    {
        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedProject>> GetProjectParameters(FilterBase filter, int skip, int take);

        /// <summary>
        /// Get observation by scroll
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<ProcessedObservation>> ScrollAsync(FilterBase filter, string scrollId);
    }
}
