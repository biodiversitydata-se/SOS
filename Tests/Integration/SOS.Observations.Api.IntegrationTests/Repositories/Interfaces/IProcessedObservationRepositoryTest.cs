using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Observations.Api.IntegrationTests.Repositories.Interfaces
{
    public interface IProcessedObservationRepositoryTest
    {
        /// <summary>
        /// Get data for Naturalis report
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pointInTimeId"></param>
        /// <param name="searchAfter"></param>
        /// <returns></returns>
        Task<SearchAfterResult<Observation>> GetNaturalisChunkAsync(SearchFilterInternal filter, string pointInTimeId = null,
           IEnumerable<object> searchAfter = null);
    }
}
