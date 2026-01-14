using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;
using System.Text.Json.Nodes;
using static SOS.Observations.Api.Managers.ObservationManager;

namespace SOS.Observations.Api.Managers.Interfaces;

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
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<PagedResult<JsonObject>> GetChunkAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take);

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

    Task<(long Count, LatLonBoundingBox? Extent)> GetCountAndExtentAsync(
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
    /// <param name="protectedIndex"></param>
    /// <returns></returns>
    Task<long> IndexCountAsync(bool protectedIndex = false);

    Task<Dictionary<string, ObservationStatistics>> CalculateObservationStatisticsAsync(DateTime fromDate, DateTime toDate);

    Task<byte[]> CreateObservationStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate);

    Task<List<NetTopologySuite.Features.Feature>?> GetAreaFiltersAsGeoJsonFeaturesAsync(IEnumerable<AreaFilter>? areaFilters);
    Task<List<NetTopologySuite.Features.Feature>?> GetGeoGraphicsFiltersAsGeoJsonFeaturesAsync(SearchFilter searchFilter);
    Task<List<(Lib.Models.Shared.Area area, Geometry geometry)>?> GetAreaFiltersAsAreaTuplesAsync(IEnumerable<AreaFilter>? areaFilters);
    Task<List<(Lib.Models.Shared.Area area, Geometry geometry)>?> GetGeographicsFiltersAsAreaTuplesAsync(SearchFilter searchFilter);
}