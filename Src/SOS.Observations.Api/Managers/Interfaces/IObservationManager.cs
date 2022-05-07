using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Cache;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
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
        /// Look for duplicates
        /// </summary>
        /// <param name="activeInstance"></param>
        /// <param name="protectedIndex"></param>
        /// <param name="maxReturnedItems"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool activeInstance, bool protectedIndex,
            int maxReturnedItems);

        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get observations by scroll
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId);

        /// <summary>
        /// Get aggregated data
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take);

        /// <summary>
        /// Get aggregated grid cells data.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridResult>> GetGeogridAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision);

        /// <summary>
        /// Geo grid tile aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision);

        /// <summary>
        /// Get metric tiles aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <returns></returns>
        Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter, int gridCellSizeInMeters);

        /// <summary>
        /// A compleate geo tile taxa aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            int zoom);

        /// <summary>
        /// Geo tile aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="zoom"></param>
        /// <param name="geoTilePage"></param>
        /// <param name="taxonIdPage"></param>
        /// <returns></returns>
        Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            int? roleId,
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
        Task<IEnumerable<LocationDto>> GetLocationsAsync(IEnumerable<string> locationIds);

        Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(SearchFilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch);

        /// <summary>
        /// Get cached TaxonSumAggregationItems
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <returns></returns>
        Task<IEnumerable<TaxonSumAggregationItem>> GetCachedTaxonSumAggregationItemsAsync(IEnumerable<int> taxonIds);
        
        /// <summary>
        /// Get number of matching observations
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(
            int? roleId,
            string authorizationApplicationIdentifier, SearchFilterBase filter);

        /// <summary>
        /// Aggregate observations by taxon.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sumUnderlyingTaxa"></param>
        /// <returns></returns>
        Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false);

        /// <summary>
        /// Aggregates present observations by taxon (absent observations are excluded).
        /// The resulting items also contains sum of underlying taxa observation count.
        /// </summary>
        /// <param name="taxonFilter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<Result<PagedResult<TaxonSumAggregationItem>>> GetTaxonSumAggregationAsync(
            TaxonFilter taxonFilter,
            int? skip,
            int? take,
            string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get a indication if taxon exist in specified area
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TaxonAggregationItemDto>> GetTaxonExistsIndicationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter);

        /// <summary>
        /// Get single observation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="outputFieldSet"></param>
        /// <param name="translationCultureCode"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="includeInternalFields"></param>
        /// <param name="ensureArtportalenUpdated"></param>
        /// <returns></returns>
        Task<dynamic> GetObservationAsync(
            int? roleId,
            string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet, string translationCultureCode, bool protectedObservations, bool includeInternalFields, bool ensureArtportalenUpdated);

        /// <summary>
        /// Get user year month counts
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthCountResultDto>> GetUserYearMonthCountAsync(SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year, month and day
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthDayCountResultDto>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Signal search
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<bool> SignalSearchInternalAsync(
            int? roleId,
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