using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Dtos;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using System.Text.Json.Nodes;
using static SOS.Observations.Api.Managers.ObservationManager;

namespace SOS.Observations.Api.Managers.Interfaces;

/// <summary>
///     Observation manager interface
/// </summary>
public interface IObservationManager
{
    /// <summary>Prepares the filter and authorization context once for a sequence of queries in one request.</summary>
    Task<PreparedObservationFilter> PrepareObservationFilterAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter);
    Task<PagedResult<JsonObject>> GetChunkAsync(PreparedObservationFilter filter, int skip, int take, long? knownTotalCount = null);
    Task<LocationRepresentativeResult<JsonObject>> GetLocationRepresentativeChunkAsync(PreparedObservationFilter filter, int take, string collapseField);
    Task<GeoGridTileResult> GetGeogridTileAggregationAsync(PreparedObservationFilter filter, int precision);
    Task<(long Count, LatLonBoundingBox? Extent, long EstimatedLocationCount)> GetCountAndExtentAsync(PreparedObservationFilter filter, bool includeExtent = true);

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
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="knownTotalCount">Count already obtained for this filter. When supplied, skips hit counting and reuses this count.</param>
    /// <returns></returns>
    Task<PagedResult<JsonObject>> GetChunkAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take, long? knownTotalCount = null);

    Task<LocationRepresentativeResult<JsonObject>> GetLocationRepresentativeChunkAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        int take,
        string collapseField);

    /// <summary>
    /// Get chunk of sightings using search_after for deep pagination.  
    /// </summary>
    /// <remarks>
    /// This function doesn't use point in time (PIT) for the search in order to use minimal server resources. 
    /// Therefore, it is not suitable for cases where data consistency is required
    /// </remarks>
    /// <param name="roleId">Limit user authorization to specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="filter">Filter used to limit the search.</param>
    /// <param name="take">Max number of observations to return.</param>
    /// <param name="searchAfter">Base64-encoded sort values from the previous page's last document. Pass null for the first request.</param>
    /// <returns>A result containing the records and the searchAfter values for the next page.</returns>
    Task<SearchAfterResult<JsonObject, string>> GetChunkBySearchAfterAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        int take,
        string? searchAfter);

    /// <summary>
    /// Get observations by scroll
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="take"></param>
    /// <param name="scrollId"></param>
    /// <returns></returns>
    Task<ScrollResult<JsonObject>> GetObservationsByScrollAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        int take,
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
    /// Get aggregated data
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="timeSeriesType"></param>
    /// <returns></returns>
    Task<IEnumerable<TimeSeriesHistogramResult>> GetTimeSeriesHistogramAsync(
        int? roleId, 
        string authorizationApplicationIdentifier, 
        SearchFilter filter, 
        TimeSeriesType timeSeriesType);

    /// <summary>
    /// Geo grid tile aggregation
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    Task<GeoGridTileResult> GetGeogridTileAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision);

    /// <summary>
    /// Get metric tiles aggregation
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="gridCellSizeInMeters"></param>
    /// <param name="metricCoordinateSys"></param>
    /// <returns></returns>
    Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        int gridCellSizeInMeters,
        MetricCoordinateSys metricCoordinateSys);

    /// <summary>
    /// Get latest data modified date for passed provider 
    /// </summary>
    /// <param name="providerId"></param>
    /// <returns></returns>
    Task<DateTime?> GetLatestModifiedDateForProviderAsync(int providerId);

    Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(SearchFilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch);

    /// <summary>
    /// Asynchronously retrieves the total number of matches that satisfy the specified filter criteria and
    /// authorization constraints.
    /// </summary>
    /// <param name="userId">The optional user identifier used to filter matches. If null, matches for all users are included.</param>
    /// <param name="roleId">The optional role identifier used to filter matches. If null, matches for all roles are included.</param>
    /// <param name="authorizationApplicationIdentifier">The identifier of the authorization application used to validate access to the matches.</param>
    /// <param name="filter">The filter criteria used to narrow down the matches. Cannot be null.</param>
    /// <param name="cultureCode">The culture code used for localization. Defaults to "en-GB" if not specified.</param>
    /// <param name="protectionFilter">The optional protection filter to apply to the results. Defaults to <see cref="ProtectionFilterDto.Public"/> if
    /// not specified.</param>
    /// <param name="skipAuthorizationFilters">Indicates whether to bypass authorization checks. Set to <see langword="true"/> to skip authorization filters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total count of matches that meet
    /// the specified criteria.</returns>
    Task<long> GetMatchCountAsync(int? userId,
        int? roleId,
        string? authorizationApplicationIdentifier,
        SearchFilterBaseDto filter,
        string? cultureCode = "en-GB",
        ProtectionFilterDto? protectionFilter = ProtectionFilterDto.Public,
        bool skipAuthorizationFilters = false);

    /// <summary>
    /// Get number of matching observations
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="skipAuthorizationFilters"></param>
    /// <returns></returns>
    Task<long> GetMatchCountAsync(
        int? roleId,
        string authorizationApplicationIdentifier, 
        SearchFilterBase filter,
        bool skipAuthorizationFilters = false);

    /// <summary>
    /// Asynchronously retrieves the total number of items and their combined geographical extent that match the
    /// specified search criteria and authorization context.
    /// </summary>
    /// <param name="roleId">The optional identifier of the role used to filter the results. If null, results are not restricted by role.</param>
    /// <param name="authorizationApplicationIdentifier">The identifier of the application context used for authorization. This value determines which items are
    /// accessible to the caller.</param>
    /// <param name="filter">The search criteria used to filter the items included in the count and extent calculation. Cannot be null.</param>
    /// <param name="skipAuthorizationFilters">true to bypass authorization checks and return results regardless of access permissions; otherwise, false to
    /// apply authorization filters.</param>
    /// <returns>The total count of matching items, an optional bounding box representing the geographical extent of those items,
    /// and an estimated count of locations. The extent is null if no items match the criteria.</returns>    
    Task<(long Count, LatLonBoundingBox? Extent, long EstimatedLocationCount)> GetCountAndExtentAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBase filter,
        bool skipAuthorizationFilters = false);

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
    /// <param name="resolveGeneralizedObservations"></param>
    /// <returns></returns>
    Task<JsonObject> GetObservationAsync(
        int? userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        string occurrenceId,
        OutputFieldSet outputFieldSet,
        string translationCultureCode,
        bool protectedObservations,
        bool includeInternalFields,
        bool resolveGeneralizedObservations);

    /// <summary>
    /// Get user year counts
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IEnumerable<YearCountResultDto>> GetUserYearCountAsync(SearchFilter filter);

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
    /// <param name="validateGeographic"></param>
    /// <returns></returns>
    Task<SignalSearchResult> SignalSearchInternalAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        int areaBuffer,
        bool onlyAboveMyClearance = true,
        bool validateGeographic = false);

    /// <summary>
    /// Count documents in index
    /// </summary>
    /// <param name="protectedIndex"></param>geogrid
    /// <returns></returns>
    Task<long> IndexCountAsync(bool protectedIndex = false);

    Task<Dictionary<string, ObservationStatistics>> CalculateObservationStatisticsAsync(DateTime fromDate, DateTime toDate);

    Task<byte[]> CreateObservationStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate);

    Task<List<NetTopologySuite.Features.Feature>?> GetAreaFiltersAsGeoJsonFeaturesAsync(IEnumerable<AreaFilter>? areaFilters);
    Task<List<NetTopologySuite.Features.Feature>?> GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(SearchFilter searchFilter);
    Task<List<(Lib.Models.Shared.Area area, Geometry geometry)>?> GetAreaFiltersAsAreaTuplesAsync(IEnumerable<AreaFilter>? areaFilters);
    Task<List<(Lib.Models.Shared.Area area, Geometry geometry)>?> GetGeographicsFiltersAsAreaTuplesAsync(SearchFilter searchFilter);
}
