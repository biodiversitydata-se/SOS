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
        Task<IActionResult> CountAsync(
            [FromBody] SearchFilterDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false);

        Task<IActionResult> SearchAsync(SearchFilterDto filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder, 
            bool validateSearchFilter, 
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> CountInternalAsync(
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false);

        Task<IActionResult> SearchInternalAsync(
            SearchFilterInternalDto filter,
            int skip,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> SearchAggregatedInternalAsync(
            SearchFilterAggregationInternalDto filter,
            AggregationType aggregationType,
            int skip,
            int take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridSearchTileBasedAggregationAsync(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom, 
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> InternalGeogridSearchTileBasedAggregationAsync(
            SearchFilterAggregationInternalDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> GeogridSearchTileBasedAggregationAsGeoJsonAsync(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE",
            bool protectedObservations = false);

        Task<IActionResult> TaxonAggregationAsync(
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

        Task<IActionResult> TaxonAggregationInternalAsync(
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

        Task<IActionResult> GetTaxonExistsIndicationAsync(
            SearchFilterDto filter,
            bool validateSearchFilter = false,
            bool protectedObservations = false);

        Task<IActionResult> GetTaxonExistsIndicationInternalAsync(
           SearchFilterInternalDto filter,
           bool validateSearchFilter = false,
           bool protectedObservations = false);
    }
}