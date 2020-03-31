using System;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, Guid>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take);
    }
}
