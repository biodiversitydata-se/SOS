using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Observations controller interface.
    /// </summary>
    public interface IObservationsController
    {
        Task<IActionResult> Count(
            [FromBody] SearchFilterDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false);

        Task<IActionResult> ObservationsBySearch(SearchFilterDto filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder, 
            bool validateSearchFilter, 
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> CountInternal(
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false);

        Task<IActionResult> ObservationsBySearchInternal(
            SearchFilterInternalDto filter,
            int skip,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> SearchAggregatedInternal(
            SearchFilterAggregationInternalDto filter,
            AggregationType aggregationType,
            int skip,
            int take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregation(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom, 
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregationInternal(
            SearchFilterAggregationInternalDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonAggregation(
            SearchFilterAggregationDto filter,
            int? skip,
            int? take,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonAggregationInternal(
            SearchFilterAggregationInternalDto filter,
            int? skip,
            int? take,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonExistsIndication(
            SearchFilterDto filter,
            bool validateSearchFilter = false,
            bool protectedObservations = false);

        Task<IActionResult> TaxonExistsIndicationInternal(
           SearchFilterInternalDto filter,
           bool validateSearchFilter = false,
           bool protectedObservations = false);
    }
}