using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Analysis.Api.Repositories.Interfaces
{
    public interface IProcessedObservationRepository : IProcessedObservationCoreRepository
    {
        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<SearchAfterResult<dynamic>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, string? afterKey, int? take = 10);
    }
}
