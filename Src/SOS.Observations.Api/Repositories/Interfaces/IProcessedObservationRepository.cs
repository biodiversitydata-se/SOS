using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, string>
    {
        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take, string sortBy, SearchSortOrder sortOrder);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        Task<Result<GeoGridResult>> GetGeogridAggregationAsync(SearchFilter filter, int precision, LatLonBoundingBox bbox);
    }
}