using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
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
        /// <param name="protectedIndex"></param>
        /// <param name="maxReturnedItems"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool protectedIndex,
            int maxReturnedItems);

        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

        /// <summary>
        /// Get chunk of sightings
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(int? userId, int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get observations by scroll
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            int? userId,
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
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(int? userId, int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take);

        /// <summary>
        /// Geo grid tile aggregation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(int? userId, int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision);

        /// <summary>
        /// Get metric tiles aggregation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <returns></returns>
        Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(
            int? userId,
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter, int gridCellSizeInMeters);

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<DateTime?> GetLatestModifiedDateForProviderAsync(int providerId);

        Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(SearchFilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch);

        /// <summary>
        /// Get number of matching observations
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(
            int? userId,
            int? roleId,
            string authorizationApplicationIdentifier, SearchFilterBase filter);

        /// <summary>
        /// Get single observation
        /// </summary>
        /// <param name="userId"></param>
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
            int? userId,
            int? roleId,
            string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet, string translationCultureCode, bool protectedObservations, bool includeInternalFields, bool ensureArtportalenUpdated);

        /// <summary>
        /// Get user year counts
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearCountResultDto>> GetUserYearCountAsync(int? userId, SearchFilter filter);

        /// <summary>
        /// Get user year month counts
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthCountResultDto>> GetUserYearMonthCountAsync(int? userId, SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year, month and day
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthDayCountResultDto>> GetUserYearMonthDayCountAsync(int? userId, SearchFilter filter, int skip, int take);

        /// <summary>
        /// Signal search
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<bool> SignalSearchInternalAsync(
            int? userId,
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