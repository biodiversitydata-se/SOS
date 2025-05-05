﻿using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static SOS.Observations.Api.Managers.ObservationManager;

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
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<JsonObject>> GetChunkAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int skip, int take);

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
        /// Aggregate on geometry area
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaType"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="coordinateSys"></param>
        /// <returns></returns>
        Task<FeatureCollection> GetAreaAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            AreaTypeAggregateDto areaType,
            bool aggregateOrganismQuantity,
            CoordinateSys coordinateSys);

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
    }
}