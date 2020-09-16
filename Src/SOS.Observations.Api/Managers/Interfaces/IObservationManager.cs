using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Observation manager interface
    /// </summary>
    public interface IObservationManager
    {
        /// <summary>
        ///     Get chunk of sightings
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
        ///     Get aggregated data
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="skip"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take, string sortBy, SearchSortOrder sortOrder);

        /// <summary>
        /// Get aggregated grid cells data.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        Task<Result<GeoGridResult>> GetGeogridAggregationAsync(SearchFilter filter, int precision, LatLonBoundingBox bbox);

        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(SearchFilter filter, int precision, LatLonBoundingBox bbox);

        /// <summary>
        /// Get number of matching observations
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(FilterBase filter);
    }
}