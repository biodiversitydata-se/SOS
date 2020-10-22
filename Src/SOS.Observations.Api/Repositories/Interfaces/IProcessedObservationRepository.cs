using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationRepository : IProcessRepositoryBase<Observation>
    {
        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

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

        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(SearchFilter filter, int zoom, LatLonBoundingBox bbox);

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId);

        /// <summary>
        /// Get number of matches for query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(FilterBase filter);

        Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            LatLonBoundingBox bbox,
            int skip,
            int take);
    }
}