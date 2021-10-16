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
            IEnumerable<int> taxonIds,
            bool includeUnderlyingTaxa = false,
            int? fromYear = null,
            int? toYear = null,
            AreaTypeDto? areaType = null,
            string featureId = null,
            bool validateSearchFilter = false);

        Task<IActionResult> Count(
            string authorizationApplicationIdentifier,
            SearchFilterBaseDto filter,
            bool validateSearchFilter = false,
            bool protectedObservations = false);

        Task<IActionResult> ObservationsBySearch(
            string authorizationApplicationIdentifier,
            SearchFilterDto filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder, 
            bool validateSearchFilter, 
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> CountInternal(
            string authorizationApplicationIdentifier,
            SearchFilterInternalBaseDto filter,
            bool validateSearchFilter = false,
            bool protectedObservations = false);

        Task<IActionResult> ObservationsBySearchInternal(
            string authorizationApplicationIdentifier,
            SearchFilterInternalDto filter,
            int skip,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false,
            OutputFormatDto outputFormat = OutputFormatDto.Json);

        Task<IActionResult> SearchAggregatedInternal(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            AggregationType aggregationType,
            int skip,
            int take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregation(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregationInternal(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int zoom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonAggregation(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationDto filter,
            int? skip,
            int? take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonAggregationInternal(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
            int? skip,
            int? take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        //Task<IActionResult> TaxonExistsIndication(
        //    SearchFilterDto filter,
        //    bool validateSearchFilter = false,
        //    bool protectedObservations = false);

        Task<IActionResult> TaxonExistsIndicationInternal(
            string authorizationApplicationIdentifier,
            SearchFilterAggregationInternalDto filter,
           bool validateSearchFilter = false,
           bool protectedObservations = false);

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
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        Task<IActionResult> GetObservationById(
            string authorizationApplicationIdentifier,
            string occurrenceId, OutputFieldSet fieldSet, 
            string translationCultureCode = "sv-SE", 
            bool protectedObservations = false);

        /// <summary>
        /// Get observation by occurrence id, include internal data
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="fieldSet"></param>
        /// <param name="translationCultureCode"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        Task<IActionResult> GetObservationByIdInternal(
            string authorizationApplicationIdentifier,
            string occurrenceId,
            OutputFieldSet fieldSet,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);
    }
}