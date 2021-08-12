using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Observation manager interface
    /// </summary>
    public interface IObservationManager
    {
        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get observations by scroll
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId);

        /// <summary>
        /// Get aggregated data
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take);

        /// <summary>
        /// Get aggregated grid cells data.
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridResult>> GetGeogridAggregationAsync(string authorizationApplicationIdentifier, SearchFilter filter, int precision);

        /// <summary>
        /// Geo grid tile aggregation
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(string authorizationApplicationIdentifier, SearchFilter filter, int precision);

        /// <summary>
        /// A compleate geo tile taxa aggregation
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            int zoom);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <param name="geoTilePage"></param>
        /// <param name="taxonIdPage"></param>
        /// <returns></returns>
        Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int zoom,
            string geoTilePage,
            int? taxonIdPage);

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<DateTime?> GetLatestModifiedDateForProviderAsync(int providerId);

        /// <summary>
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds);

        /// <summary>
        /// Get number of matching observations
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(string authorizationApplicationIdentifier, FilterBase filter);

        /// <summary>
        /// Aggregate observations by taxon.
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int? skip,
            int? take);

        /// <summary>
        /// Get a indication if taxon exist in specified area
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TaxonAggregationItemDto>> GetTaxonExistsIndicationAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter);

        /// <summary>
        /// Get single observation
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="translationCultureCode"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="includeInternalFields"></param>
        /// <returns></returns>
        Task<dynamic> GetObservationAsync(string authorizationApplicationIdentifier, string occurrenceId, string translationCultureCode, bool protectedObservations, bool includeInternalFields);

        /// <summary>
        /// Signal search
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<bool> SignalSearchInternalAsync(
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            int areaBuffer,
            bool onlyAboveMyClearance = true);

        /// <summary>
        /// Count documents in index
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<long> IndexCountAsync(bool protectedIndex = false);
    }
}