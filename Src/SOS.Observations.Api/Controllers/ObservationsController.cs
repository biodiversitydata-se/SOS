using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Swagger;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ObservationBaseController, IObservationsController
    {
        private readonly IAreaManager _areaManager;
        private readonly int _tilesLimit;
        private readonly ILogger<ObservationsController> _logger;

        private void AdjustEnvelopeByShape(IGeoShape geoShape, ref double? bboxLeft,
            ref double? bboxTop,
            ref double? bboxRight,
            ref double? bboxBottom)
        {
            if (geoShape == null || geoShape.Type.Equals("point", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var envelope = geoShape.ToGeometry().EnvelopeInternal;

            if (envelope.IsNull)
            {
                return;
            }

            if (!bboxLeft.HasValue || envelope.MinX > bboxLeft)
            {
                bboxLeft = envelope.MinX;
            }
            if (!bboxRight.HasValue || envelope.MaxX < bboxRight)
            {
                bboxRight = envelope.MaxX;
            }
            if (!bboxBottom.HasValue || envelope.MinY > bboxBottom)
            {
                bboxBottom = envelope.MinY;
            }
            if (!bboxTop.HasValue || envelope.MaxY < bboxTop)
            {
                bboxTop = envelope.MaxY;
            }
        }

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="bboxLeft"></param>
        /// <param name="bboxTop"></param>
        /// <param name="bboxRight"></param>
        /// <param name="bboxBottom"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<Envelope> GetBoundingBox(double? bboxLeft = null,
            double? bboxTop = null,
            double? bboxRight = null,
            double? bboxBottom = null,
            SearchFilterBaseDto filter = null)
        {
            // If areas passed, adjust bounding box to them
            if (filter.Areas?.Any() ?? false)
            {
                var areas = await _areaManager.GetAreasAsync(filter.Areas.Select(a => (a.AreaType, a.FeatureId)));
                var areaGeometries = areas?.Select(a => a.BoundingBox.GetPolygon().ToGeoShape());
                //await _areaManager.GetGeometriesAsync(filter.Areas.Select(a => ((AreaType) a.AreaType, a.FeatureId)));
                foreach (var areaGeometry in areaGeometries)
                {
                    AdjustEnvelopeByShape(areaGeometry, ref bboxLeft, ref bboxTop, ref bboxRight, ref bboxBottom);
                }
            }

            // If geometries passed, adjust bounding box to them
            if (filter.Geometry?.Geometries?.Any() ?? false)
            {
                foreach (var areaGeometry in filter.Geometry.Geometries)
                {
                    AdjustEnvelopeByShape(areaGeometry, ref bboxLeft, ref bboxTop, ref bboxRight, ref bboxBottom);
                }
            }

            // Get geometry of sweden economic zone
            var swedenGeometry = await _areaManager.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1");

            // Todo remove. Only used when area source was AP
            if (swedenGeometry == null)
            {
                swedenGeometry = await _areaManager.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "100");
            }

            // Get bounding box of swedish economic zone
            var swedenBoundingBox = swedenGeometry.ToGeometry().EnvelopeInternal;

            if (!(bboxLeft.HasValue && bboxTop.HasValue && bboxRight.HasValue && bboxBottom.HasValue))
            {
                return swedenBoundingBox;
            }

            // Create a bound box using user passed values
            var boundingBox = Geometry.DefaultFactory.CreatePolygon(new LinearRing(new[]
            {
                new Coordinate(bboxLeft.Value, bboxTop.Value),
                new Coordinate(bboxLeft.Value, bboxBottom.Value),
                new Coordinate(bboxRight.Value, bboxBottom.Value),
                new Coordinate(bboxRight.Value, bboxTop.Value),
                new Coordinate(bboxLeft.Value, bboxTop.Value),
            })).EnvelopeInternal;

            // Try to intersect sweden and user defined bb
            boundingBox = swedenBoundingBox.Intersection(boundingBox);

            // If user bb outside of sweden, use sweden
            if (boundingBox.IsNull)
            {
                boundingBox = swedenBoundingBox;
            }

            return boundingBox;
        }
        private async Task<Result<Envelope>> ValidateBoundingBoxAsync(
            double? left,
            double? top,
            double? right,
            double? bottom,
            int zoom,
            SearchFilterBaseDto filter,
            bool checkNrTilesLimit = true)
        {
            var boundingBox = await GetBoundingBox(left, top, right, bottom, filter);

            if (boundingBox.MinX >= boundingBox.MaxX)
            {
                return Result.Failure<Envelope>("Bbox left value is >= right value.");
            }

            if (boundingBox.MinY >= boundingBox.MaxY)
            {
                return Result.Failure<Envelope>("Bbox bottom value is >= top value.");
            }

            var tileWidthInDegrees = 360 / Math.Pow(2, zoom);
            var tileHeightInDegrees = tileWidthInDegrees * Math.Cos(boundingBox.Centre.Y.ToRadians());

            var lonDiff = Math.Abs(boundingBox.MaxX - boundingBox.MinX);
            var latDiff = Math.Abs(boundingBox.MaxY - boundingBox.MinY);
            var maxLonTiles = Math.Ceiling(lonDiff / tileWidthInDegrees);
            var maxLatTiles = Math.Ceiling(latDiff / tileHeightInDegrees);
            var maxTilesTot = maxLonTiles * maxLatTiles;

            if (checkNrTilesLimit && maxTilesTot > _tilesLimit)
            {
                return Result.Failure<Envelope>($"The number of cells that can be returned is too large. The limit is {_tilesLimit} cells. Try using lower zoom or a smaller bounding box.");
            }

            return Result.Success(boundingBox);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        public ObservationsController(
            IObservationManager observationManager,
            ITaxonManager taxonManager,
            IAreaManager areaManager,
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<ObservationsController> logger) : base(observationManager, taxonManager)
        {
            _areaManager = areaManager ?? throw new ArgumentNullException(nameof(areaManager));
            _tilesLimit = observationApiConfiguration?.TilesLimit ??
                          throw new ArgumentNullException(nameof(observationApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get observations matching the provided search filter. Permitted filter values depends on the specific filter field:
        ///     Some values are retrieved from the vocabularies endpoint. Some are defined as enum values. Some values are defined in other systems, e.g. Dyntaxa taxon id's.
        ///     Some are defined by the range of the underlying data type.
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of observations to return. Max is 1000 observations in each request.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns>List of observations matching the provided search filter.</returns>
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
        [HttpPost("Search")]
        [ProducesResponseType(typeof(PagedResultDto<Observation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ObservationsBySearch(
            [FromBody] SearchFilterDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var validationResult = Result.Combine(
                    ValidateSearchPagingArguments(skip, take),
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateTranslationCultureCode(translationCultureCode));
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                SearchFilter searchFilter = filter.ToSearchFilter(translationCultureCode, protectedObservations);
                var result = await ObservationManager.GetChunkAsync(searchFilter, skip, take, sortBy, sortOrder);
                PagedResultDto<dynamic> dto = result.ToPagedResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Search error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count the number of observations matching the provided search filter.
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="protectedObservations">If true only protected observations will be searched (this requires authentication and authorization). If false, default, public available observations will be searched.</param>
        /// <returns>The number of observations matching the provided search filter.</returns>
        [HttpPost("Count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Count(
            [FromBody] SearchFilterDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var validationResult = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilter("sv-SE", protectedObservations);
                var matchCount = await ObservationManager.GetMatchCountAsync(searchFilter);

                return new OkObjectResult(matchCount);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Count error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
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
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("GeoGridAggregation")]
        [ProducesResponseType(typeof(GeoGridResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GeogridAggregation(
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, zoom, filter);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var zoomOrError = ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21);

                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, zoomOrError,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);
                var result = await ObservationManager.GetGeogridTileAggregationAsync(filter.ToSearchFilter(translationCultureCode, protectedObservations), zoom, bbox);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                GeoGridResultDto dto = result.Value.ToGeoGridResultDto();
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }


        ///// <summary>
        ///// Aggregates observations into grid cells. Each grid cell contains a list of all taxa (usually species)
        ///// in the grid cell and the number of observations.
        ///// The grid cells are squares in WGS84 coordinate system which means that they also
        ///// will be squares in the WGS84 Web Mercator coordinate system.
        ///// </summary>
        ///// <remarks>
        ///// The following table shows the approximate grid cell size (width) in different
        ///// coordinate systems for the different zoom levels.
        ///// | Zoom level | WGS84    | Web Mercator  |  SWEREF99TM(Southern Sweden) |  SWEREF99TM(North Sweden) |
        ///// |------------|----------|---------------|:----------------------------:|:-------------------------:|
        ///// | 1          |      180 |       20000km |                       8000km |                   12000km |
        ///// | 2          |       90 |       10000km |                       4000km |                    6000km |
        ///// | 3          |       45 |        5000km |                       2000km |                    3000km |
        ///// | 4          |     22.5 |        2500km |                       1000km |                    1500km |
        ///// | 5          |    11.25 |        1250km |                        500km |                     750km |
        ///// | 6          |    5.625 |         600km |                        250km |                     360km |
        ///// | 7          |   2.8125 |         300km |                        120km |                     180km |
        ///// | 8          | 1.406250 |         150km |                         60km |                      90km |
        ///// | 9          | 0.703125 |          80km |                         30km |                      45km |
        ///// | 10         | 0.351563 |          40km |                         15km |                      23km |
        ///// | 11         | 0.175781 |          20km |                          8km |                      11km |
        ///// | 12         | 0.087891 |          10km |                          4km |                       6km |
        ///// | 13         | 0.043945 |           5km |                          2km |                       3km |
        ///// | 14         | 0.021973 |         2500m |                        1000m |                     1400m |
        ///// | 15         | 0.010986 |         1200m |                         500m |                      700m |
        ///// | 16         | 0.005493 |          600m |                         240m |                      350m |
        ///// | 17         | 0.002747 |          300m |                         120m |                      180m |
        ///// | 18         | 0.001373 |          150m |                          60m |                       90m |
        ///// | 19         | 0.000687 |           80m |                          30m |                       45m |
        ///// | 20         | 0.000343 |           40m |                          15m |                       22m |
        ///// | 21         | 0.000172 |           19m |                           7m |                       11m |
        ///// </remarks>
        ///// <param name="filter">The search filter.</param>
        ///// <param name="zoom">A zoom level between 1 and 21.</param>
        ///// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        ///// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        ///// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        ///// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        ////// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        ///// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        ///// <returns></returns>
        //[HttpPost("GeoGridTaxaAggregationCompleteInternal")]
        //[ProducesResponseType(typeof(IEnumerable<GeoGridTileTaxaCellDto>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //[InternalApi]
        //public async Task<IActionResult> InternalCompleteGeogridTaxaAggregationAsync(
        //    [FromBody] SearchFilterAggregationInternalDto filter,
        //    [FromQuery] int zoom = 1,
        //    [FromQuery] double? bboxLeft = null,
        //    [FromQuery] double? bboxTop = null,
        //    [FromQuery] double? bboxRight = null,
        //    [FromQuery] double? bboxBottom = null,
        //    [FromQuery] bool validateSearchFilter = false,
        //    [FromQuery] string translationCultureCode = "sv-SE")
        //{
        //    try
        //    {
        //        var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, zoom, filter, false);
        //        var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
        //        var zoomOrError = ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21);

        //        var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, zoomOrError,
        //            ValidateTranslationCultureCode(translationCultureCode));
        //        if (paramsValidationResult.IsFailure)
        //        {
        //            return BadRequest(paramsValidationResult.Error);
        //        }

        //        var bbox = LatLonBoundingBox.Create(bboxValidation.Value);
        //        var result = await ObservationManager.GetCompleteGeoTileTaxaAggregationAsync(filter.ToSearchFilterInternal(translationCultureCode), zoom, bbox);
        //        if (result.IsFailure)
        //        {
        //            return BadRequest(result.Error);
        //        }

        //        IEnumerable<GeoGridTileTaxaCellDto> dto = result.Value.Select(m => m.ToGeoGridTileTaxaCellDto());
        //        return new OkObjectResult(dto);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "GeoGridAggregation error.");
        //        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        //    }
        //}

        /// <summary>
        /// Aggregates observations by taxon. Each record contains TaxonId and the number of observations (ObservationCount) matching the search criteria.
        /// The records are ordered by ObservationCount in descending order.
        /// To get the first 100 taxa with the most observations, set skip to 0 and take to 100.
        /// You can only get the first 1000 taxa by using paging. To retrieve all records, set skip and take parameters to null.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("TaxonAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TaxonAggregation(
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int? skip = 0,
            [FromQuery] int? take = 100,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, 1, filter);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var pagingArgumentsValidation = ValidateTaxonAggregationPagingArguments(skip, take);
                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, pagingArgumentsValidation,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);

                var result = await ObservationManager.GetTaxonAggregationAsync(
                    filter.ToSearchFilter(translationCultureCode, protectedObservations), 
                    bbox, 
                    skip, 
                    take);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                PagedResultDto<TaxonAggregationItemDto> dto = result.Value.ToPagedResultDto(result.Value.Records.ToTaxonAggregationItemDtos());
                return new OkObjectResult(dto);
            }
            catch(AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        ///// <summary>
        ///// Get an indication of how many observations exist for the taxa specified in the search criteria filter.
        ///// If protectedObservations is set to false, you must be aware of that the result can include false positives
        ///// since the protected observations coordinates are generalized to a grid depending on the protection level.
        ///// </summary>
        ///// <param name="filter">Search criteria filter used to limit the search.</param>
        ///// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        ///// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        ///// <returns></returns>
        //[HttpPost("TaxonExistsIndication")]
        //[ProducesResponseType(typeof(IEnumerable<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> TaxonExistsIndication(
        //    [FromBody] SearchFilterDto filter,
        //    [FromQuery] bool validateSearchFilter = false,
        //    [FromQuery] bool protectedObservations = false)
        //{
        //    try
        //    {
        //        var validationResult = Result.Combine(validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(), 
        //            ValidateTaxonExists(filter),
        //            ValidateGeographicalAreaExists(filter));

        //        if (validationResult.IsFailure)
        //        {
        //            return BadRequest(validationResult.Error);
        //        }

        //        var searchFilter = filter.ToSearchFilter("sv-SE", protectedObservations);
        //       // Force area geometry search in order to use point with buffer
        //        searchFilter.AreaGeometrySearchForced = true;
        //        var taxonFound = await ObservationManager.GetTaxonExistsIndicationAsync(searchFilter);

        //        return new OkObjectResult(taxonFound);
        //    }
        //    catch (AuthenticationRequiredException e)
        //    {
        //        return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        //    }
        //    catch (TaxonValidationException e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "Get indication if taxon exists error");
        //        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        //    }
        //}

        /// <summary>
        /// Gets a single observation.
        /// </summary>
        /// <param name="occurrenceId">The occurence id of the observation to fetch.</param>
        /// <param name="protectedObservations">
        /// If true, and the requested observation is protected, then the original data will be returned (this requires authentication and authorization).
        /// If false, and the requested observation is protected, then diffused data will be returned.
        /// </param>
        /// <returns></returns>
        [HttpGet("{occurrenceId}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetObservationById([FromRoute] string occurrenceId, [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var observation = await ObservationManager.GetObservationAsync(occurrenceId, protectedObservations,
                    includeInternalFields: false);

                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(observation);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting observation {occurrenceId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get observations matching the provided search filter. Permitted filter values depends on the specific filter field:
        ///     Some values are retrieved from the vocabularies endpoint. Some are defined as enum values. Some values are defined in other systems, e.g. Dyntaxa taxon id's.
        ///     Some are defined by the range of the underlying data type.
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of observations to return.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns>List of observations matching the provided search filter.</returns>
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
        [HttpPost("Internal/Search")]
        [ProducesResponseType(typeof(PagedResultDto<Observation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> ObservationsBySearchInternal(
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var validationResult = Result.Combine(
                    ValidateSearchPagingArgumentsInternal(skip, take),
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var result = await ObservationManager.GetChunkAsync(filter.ToSearchFilterInternal(translationCultureCode, protectedObservations), skip, take, sortBy, sortOrder);
                PagedResultDto<dynamic> dto = result.ToPagedResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SearchInternal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get observations matching the provided search filter. This endpoint allows to retrieve up to 100 000 observations by using Elasticsearch scroll API.
        ///     Timeout between calls are two minutes.
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="scrollId">The scroll id to use to get next batch. In first request scrollId should be empty.</param>
        /// <param name="take">Max number of observations to return. Max is 10 000 observations in each request.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns>List of observations matching the provided search filter.</returns>
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
        [HttpPost("Internal/SearchScroll")]
        [ProducesResponseType(typeof(ScrollResultDto<Observation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> ObservationsScroll(
            [FromBody] SearchFilterDto filter,
            [FromQuery] string scrollId,
            [FromQuery] int take = 5000,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                const int maxTotalCount = 100000;
                var takeValidation = take <= 10000
                    ? Result.Success()
                    : Result.Failure("You can't take more than 10 000 at a time.");
                
                var validationResult = Result.Combine(
                    takeValidation,
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateTranslationCultureCode(translationCultureCode));
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                SearchFilter searchFilter = filter.ToSearchFilter(translationCultureCode, protectedObservations);
                var result = await ObservationManager.GetObservationsByScrollAsync(searchFilter, take, sortBy, sortOrder, scrollId);
                if (result.TotalCount > maxTotalCount)
                {
                    return BadRequest($"Scroll total count limit is maxTotalCount. Your result is {result.TotalCount}. Try use a more specific filter.");
                }
                ScrollResultDto<dynamic> dto = result.ToScrollResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Search error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count matching observations using internal filter
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/Count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> CountInternal(
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var validationResult = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilterInternal("sv-SE", protectedObservations);
                var matchCount = await ObservationManager.GetMatchCountAsync(searchFilter);

                return new OkObjectResult(matchCount);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Internal count error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregate observations by the specified aggregation type.
        /// </summary>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of records to return.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/SearchAggregated")]
        [ProducesResponseType(typeof(PagedResultDto<Observation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> SearchAggregatedInternal(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] AggregationType aggregationType,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var paramsValidationResult = Result.Combine(
                    ValidateAggregationPagingArguments(skip, take),
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode));

                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var result = await ObservationManager.GetAggregatedChunkAsync(filter.ToSearchFilterInternal(translationCultureCode, protectedObservations), aggregationType, skip, take);
                PagedResultDto<dynamic> dto = result.ToPagedResultDto(result?.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SearchAggregatedInternal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
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
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridAggregation")]
        [ProducesResponseType(typeof(GeoGridResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridAggregationInternal(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, zoom, filter);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var zoomOrError = ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21);

                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, zoomOrError,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);
                var result = await ObservationManager.GetGeogridTileAggregationAsync(filter.ToSearchFilterInternal(translationCultureCode, protectedObservations), zoom, bbox);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                GeoGridResultDto dto = result.Value.ToGeoGridResultDto();
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates observations into grid cells and returns them as a GeoJSON file.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridAggregationGeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, zoom, filter);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var zoomOrError = ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21);

                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, zoomOrError,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);
                var result = await ObservationManager.GetGeogridTileAggregationAsync(filter.ToSearchFilter(translationCultureCode, protectedObservations), zoomOrError.Value, bbox);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                string strJson = result.Value.GetFeatureCollectionGeoJson();
                var bytes = Encoding.UTF8.GetBytes(strJson);
                return File(bytes, "application/json", "grid.geojson");
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregationGeoJson error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates observations into grid cells and taxa. Each grid cell contains a list of all taxa (usually species)
        /// in the grid cell and the number of observations. 
        /// The grid cells are squares in WGS84 coordinate system which means that they also
        /// will be squares in the WGS84 Web Mercator coordinate system.
        /// </summary>
        /// <remarks>
        /// Due to paging, the last grid cell in the result usually does not contain all its taxa.
        /// The remaining taxa will be retrieved in the next page.
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
        /// <param name="geoTilePage">The GeoTile key used to retrieve the next next page of data. Should be null in the first request.</param>
        /// <param name="taxonIdPage">The TaxonId key used to retrieve the next page of data. Should be null in the first request.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridTaxaAggregation")]
        [ProducesResponseType(typeof(GeoGridTileTaxonPageResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridTaxaAggregationInternal(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] string geoTilePage = null,
            [FromQuery] int? taxonIdPage = null,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE")
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, zoom, filter, false);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var zoomOrError = ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21);
                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, zoomOrError,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);
                var result = await ObservationManager.GetPageGeoTileTaxaAggregationAsync(filter.ToSearchFilterInternal(translationCultureCode, false), zoom, bbox, geoTilePage, taxonIdPage);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                GeoGridTileTaxonPageResultDto dto = result.Value.ToGeoGridTileTaxonPageResultDto();
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates observations by taxon. Each record contains TaxonId and the number of observations (ObservationCount) matching the search criteria.
        /// The records are ordered by ObservationCount in descending order.
        /// To get the first 100 taxa with the most observations, set skip to 0 and take to 100.
        /// You can only get the first 1000 taxa by using paging. To retrieve all records, set skip and take parameters to null.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/TaxonAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> TaxonAggregationInternal(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int? skip = 0,
            [FromQuery] int? take = 100,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var bboxValidation = await ValidateBoundingBoxAsync(bboxLeft, bboxTop, bboxRight, bboxBottom, 1, filter);
                var filterValidation = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();
                var pagingArgumentsValidation = ValidateTaxonAggregationPagingArguments(skip, take);
                var paramsValidationResult = Result.Combine(bboxValidation, filterValidation, pagingArgumentsValidation,
                    ValidateTranslationCultureCode(translationCultureCode));
                if (paramsValidationResult.IsFailure)
                {
                    return BadRequest(paramsValidationResult.Error);
                }

                var bbox = LatLonBoundingBox.Create(bboxValidation.Value);

                var result = await ObservationManager.GetTaxonAggregationAsync(filter.ToSearchFilterInternal(translationCultureCode, protectedObservations), bbox, skip, take);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                PagedResultDto<TaxonAggregationItemDto> dto = result.Value.ToPagedResultDto(result.Value.Records.ToTaxonAggregationItemDtos());
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get an indication of how many observations exist for the taxa specified in the search criteria filter.
        /// If protectedObservations is set to false, you must be aware of that the result can include false positives
        /// since the protected observations coordinates are generalized to a grid depending on the protection level.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="protectedObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/TaxonExistsIndication")]
        [ProducesResponseType(typeof(IEnumerable<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> TaxonExistsIndicationInternal(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTaxonExists(filter),
                    ValidateGeographicalAreaExists(filter));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal("sv-SE", protectedObservations);
                var taxonFound = await ObservationManager.GetTaxonExistsIndicationAsync(searchFilter);

                return new OkObjectResult(taxonFound);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get indication if taxon exists Internal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets a single observation, including internal fields.
        /// </summary>
        /// <param name="occurrenceId">The occurence id of the observation to fetch.</param>
        /// <param name="protectedObservations">
        /// If true, and the requested observation is protected, then the original data will be returned (this requires authentication and authorization).
        /// If false, and the requested observation is protected, then diffused data will be returned.
        /// </param>
        /// <returns></returns>
        [HttpGet("Internal/{occurrenceId}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetObservationByIdInternal([FromRoute] string occurrenceId, [FromQuery] bool protectedObservations = false)
        {
            try
            {
                var observation = await ObservationManager.GetObservationAsync(occurrenceId, protectedObservations,
                    includeInternalFields: true);
                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(observation);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting observation {occurrenceId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("Internal/SignalSearch")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> SignalSearchInternalAsync(
            SearchFilterAggregationInternalDto filter,
            bool validateSearchFilter = false,
            bool onlyAboveMyClearance = true)
        {
            try
            {
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateGeographicalAreaExists(filter),
                    ValidateSignalSearchDate(filter.Date));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal("sv-SE", true);
                var taxonFound = await ObservationManager.SignalSearchInternalAsync(searchFilter, onlyAboveMyClearance);

                return new OkObjectResult(taxonFound);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Signal search Internal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}