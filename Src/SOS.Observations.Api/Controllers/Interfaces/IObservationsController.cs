using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Observations controller interface.
    /// </summary>
    public interface IObservationsController
    {
        Task<IActionResult> CachedCount(
            int taxonId,
            bool includeUnderlyingTaxa = false,
            int? fromYear = null,
            int? toYear = null,
            AreaTypeDto? areaType = null,
            string featureId = null,
            int? dataProviderId = null,
            bool validateSearchFilter = false);

        Task<IActionResult> MultipleCachedCount(
            IEnumerable<int> taxonIds,
            bool includeUnderlyingTaxa = false,
            int? fromYear = null,
            int? toYear = null,
            AreaTypeDto? areaType = null,
            string featureId = null,
            int? dataProviderId = null,
            bool validateSearchFilter = false);

        Task<IActionResult> Count(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterBaseDto filter,
            bool validateSearchFilter = false,
            bool sensitiveObservations = false);

        Task<IActionResult> ObservationsBySearch(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterDto filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder, 
            bool validateSearchFilter, 
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> CountInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterInternalBaseDto filter,
            bool validateSearchFilter = false,
            bool sensitiveObservations = false);

        Task<IActionResult> ObservationsBySearchInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterInternalDto filter,
            int skip,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false,
            OutputFormatDto outputFormat = OutputFormatDto.Json);

        Task<IActionResult> SearchAggregatedInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            AggregationType aggregationType,
            int skip,
            int take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> GeogridAggregation(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> GeogridAggregationInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> GetMetricGridAggregationAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int gridCellSizeInMeters = 10000,
            bool validateSearchFilter = false,
            bool sensitiveObservations = false);

        Task<IActionResult> TaxonAggregation(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int? skip,
            int? take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);

        Task<IActionResult> TaxonAggregationInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            int? skip,
            int? take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);        

        Task<IActionResult> TaxonExistsIndicationInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
           bool validateSearchFilter = false,
           bool sensitiveObservations = false);

        /// <summary>
        ///  Signal search
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<IActionResult> SignalSearchInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            SignalFilterDto filter,
            bool validateSearchFilter = false,
            int areaBuffer = 0,
            bool onlyAboveMyClearance = true);

        /// <summary>
        /// Get observation by occurrence id
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="fieldSet"></param>
        /// <param name="translationCultureCode"></param>
        /// <param name="sensitiveObservations"></param>
        /// <returns></returns>
        Task<IActionResult> GetObservationById(
            int? roleId,
            string authorizationApplicationIdentifier,
            string occurrenceId, OutputFieldSet fieldSet, 
            string translationCultureCode = "sv-SE", 
            bool sensitiveObservations = false);

        /// <summary>
        /// Get observation by occurrence id, include internal data
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="fieldSet"></param>
        /// <param name="translationCultureCode"></param>
        /// <param name="sensitiveObservations"></param>
        /// <returns></returns>
        Task<IActionResult> GetObservationByIdInternal(
            int? roleId,
            string authorizationApplicationIdentifier,
            string occurrenceId,
            OutputFieldSet fieldSet,
            string translationCultureCode = "sv-SE",
            bool sensitiveObservations = false);        
    }
}