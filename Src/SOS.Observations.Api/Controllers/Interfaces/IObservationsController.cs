using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Filter;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Sighting controller interface
    /// </summary>
    public interface IObservationsController
    {
        /// <summary>
        ///     Search for observations by the provided filter. All permitted values are either specified in the Field Mappings
        ///     object
        ///     retrievable from the Field Mappings endpoint or by the range of the underlying data type. All fields containing
        ///     the substring "Id" (but not exclusively) are mapped in the Field Mappings object.
        /// </summary>
        /// <param name="filter">Filter used to limit the search</param>
        /// <param name="skip">Start index of returned observations</param>
        /// <param name="take">Max number of observations to return</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="sortOrder">Sort order (ASC, DESC)</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns>List of matching observations</returns>
        /// <example>
        ///     Get all observations within 100m of provided point
        ///     "geometryFilter": {
        ///     "maxDistanceFromPoint": 100,
        ///     "geometry": {
        ///     "coordinates": [ 12.3456(lon), 78.9101112(lat) ],
        ///     "type": "Point"
        ///     },
        ///     "usePointAccuracy": false
        ///     }
        /// </example>
        Task<IActionResult> SearchAsync(SearchFilterDto filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder, 
            bool validateSearchFilter, 
            string translationCultureCode = "sv-SE");

        /// <summary>
        ///     Search for observations by the provided filter. All permitted values are either specified in the Field Mappings
        ///     object
        ///     retrievable from the Field Mappings endpoint or by the range of the underlying data type. All fields containing
        ///     the substring "Id" (but not exclusively) are mapped in the Field Mappings object.
        /// </summary>
        /// <param name="filter">Filter used to limit the search</param>
        /// <param name="skip">Start index of returned observations</param>
        /// <param name="take">Max number of observations to return</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="sortOrder">Sort order (ASC, DESC)</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns>List of matching observations</returns>
        /// <example>
        ///     Get all observations within 100m of provided point
        ///     "geometryFilter": {
        ///     "maxDistanceFromPoint": 100,
        ///     "geometry": {
        ///     "coordinates": [ 12.3456(lon), 78.9101112(lat) ],
        ///     "type": "Point"
        ///     },
        ///     "usePointAccuracy": false
        ///     }
        /// </example>
        Task<IActionResult> SearchInternalAsync(
            SearchFilterInternalDto filter,
            int skip,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");


        Task<IActionResult> SearchAggregatedInternalAsync(
            SearchFilterAggregationInternalDto filter,
            AggregationType aggregationType,
            int skip,
            int take,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Aggregates observations into grid cells. Each grid cell contains the number
        /// of observations and the number of unique taxa (usually species) in the grid cell.
        /// The grid cells are squares in WGS84 coordinate system which means that they also
        /// will be squares in the WGS84 Web Mercator coordinate system.
        /// </summary>
        /// <remarks>
        /// The following table shows the approximate grid cell size (width) in different
        /// coordinate systems for the different zoom levels.
        /// | Zoom level | WGS84    | Web Mercator  |  SWEREF99TM(Southern Sweden) |  SWEREF99TM(North Sweden) |
        /// |------------|----------|---------------|:----------------------------:|:-------------------------:|
        /// | 1          |      180 |       20000km |                       8000km |                   12000km |
        /// | 2          |       90 |       10000km |                       4000km |                    6000km |
        /// | 3          |       45 |        5000km |                       2000km |                    3000km |
        /// | 4          |     22.5 |        2500km |                       1000km |                    1500km |
        /// | 5          |    11.25 |        1250km |                        500km |                     750km |
        /// | 6          |    5.625 |         600km |                        250km |                     360km |
        /// | 7          |   2.8125 |         300km |                        120km |                     180km |
        /// | 8          | 1.406250 |         150km |                         60km |                      90km |
        /// | 9          | 0.703125 |          80km |                         30km |                      45km |
        /// | 10         | 0.351563 |          40km |                         15km |                      23km |
        /// | 11         | 0.175781 |          20km |                          8km |                      11km |
        /// | 12         | 0.087891 |          10km |                          4km |                       6km |
        /// | 13         | 0.043945 |           5km |                          2km |                       3km |
        /// | 14         | 0.021973 |         2500m |                        1000m |                     1400m |
        /// | 15         | 0.010986 |         1200m |                         500m |                      700m |
        /// | 16         | 0.005493 |          600m |                         240m |                      350m |
        /// | 17         | 0.002747 |          300m |                         120m |                      180m |
        /// | 18         | 0.001373 |          150m |                          60m |                       90m |
        /// | 19         | 0.000687 |           80m |                          30m |                       45m |
        /// | 20         | 0.000343 |           40m |                          15m |                       22m |
        /// | 21         | 0.000172 |           19m |                           7m |                       11m |
        /// </remarks>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        Task<IActionResult> GeogridSearchTileBasedAggregationAsync(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom, 
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Aggregates observations into grid cells. Each grid cell contains the number
        /// of observations and the number of unique taxa (usually species) in the grid cell.
        /// The grid cells are squares in WGS84 coordinate system which means that they also
        /// will be squares in the WGS84 Web Mercator coordinate system.
        /// </summary>
        /// <remarks>
        /// The following table shows the approximate grid cell size (width) in different
        /// coordinate systems for the different zoom levels.
        /// | Zoom level | WGS84    | Web Mercator  |  SWEREF99TM(Southern Sweden) |  SWEREF99TM(North Sweden) |
        /// |------------|----------|---------------|:----------------------------:|:-------------------------:|
        /// | 1          |      180 |       20000km |                       8000km |                   12000km |
        /// | 2          |       90 |       10000km |                       4000km |                    6000km |
        /// | 3          |       45 |        5000km |                       2000km |                    3000km |
        /// | 4          |     22.5 |        2500km |                       1000km |                    1500km |
        /// | 5          |    11.25 |        1250km |                        500km |                     750km |
        /// | 6          |    5.625 |         600km |                        250km |                     360km |
        /// | 7          |   2.8125 |         300km |                        120km |                     180km |
        /// | 8          | 1.406250 |         150km |                         60km |                      90km |
        /// | 9          | 0.703125 |          80km |                         30km |                      45km |
        /// | 10         | 0.351563 |          40km |                         15km |                      23km |
        /// | 11         | 0.175781 |          20km |                          8km |                      11km |
        /// | 12         | 0.087891 |          10km |                          4km |                       6km |
        /// | 13         | 0.043945 |           5km |                          2km |                       3km |
        /// | 14         | 0.021973 |         2500m |                        1000m |                     1400m |
        /// | 15         | 0.010986 |         1200m |                         500m |                      700m |
        /// | 16         | 0.005493 |          600m |                         240m |                      350m |
        /// | 17         | 0.002747 |          300m |                         120m |                      180m |
        /// | 18         | 0.001373 |          150m |                          60m |                       90m |
        /// | 19         | 0.000687 |           80m |                          30m |                       45m |
        /// | 20         | 0.000343 |           40m |                          15m |                       22m |
        /// | 21         | 0.000172 |           19m |                           7m |                       11m |
        /// </remarks>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        Task<IActionResult> InternalGeogridSearchTileBasedAggregationAsync(
            SearchFilterAggregationInternalDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Aggregates observations into grid cells and returns a GeoJSON file with all grid cells.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        Task<IActionResult> GeogridSearchTileBasedAggregationAsGeoJsonAsync(
            SearchFilterAggregationDto filter,
            int zoom,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Aggregates observation by taxon. Each item contains the number of observations for the specific taxon.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. Skip + Take must be less than or equal to 65535.</param>
        /// <param name="take">Max number of taxa to return. Skip + Take must be less than or equal to 65535.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        Task<IActionResult> TaxonAggregationAsync(
            SearchFilterAggregationDto filter,
            int skip,
            int take,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Aggregates observation by taxon. Each item contains the number of observations for the specific taxon.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. Skip + Take must be less than or equal to 65535.</param>
        /// <param name="take">Max number of taxa to return. Skip + Take must be less than or equal to 65535.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">Validation of filter properties will ONLY be made if this is set to true</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        Task<IActionResult> TaxonAggregationInternalAsync(
            SearchFilterAggregationInternalDto filter,
            int skip,
            int take,
            double? bboxLeft,
            double? bboxTop,
            double? bboxRight,
            double? bboxBottom,
            bool validateSearchFilter,
            string translationCultureCode = "sv-SE");

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<IActionResult> GetLatestModifiedDateForProviderAsync(int providerId);
    }
}