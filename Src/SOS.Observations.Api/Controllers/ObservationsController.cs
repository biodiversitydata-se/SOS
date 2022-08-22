using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
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
        private readonly ITaxonSearchManager _taxonSearchManager;
        private readonly int _tilesLimit;
        private readonly IEnumerable<int> _signalSearchTaxonListIds;
        private readonly ILogger<ObservationsController> _logger;

        private void AdjustEnvelopeByShape(
            IGeoShape geoShape,
            ref double? bboxLeft,
            ref double? bboxTop,
            ref double? bboxRight,
            ref double? bboxBottom,
            double? maxDistanceFromPoint)
        {
            if (geoShape == null)
            {
                return;
            }

            Envelope envelope;
            if (geoShape.Type.Equals("point", StringComparison.CurrentCultureIgnoreCase))
            {
                if (maxDistanceFromPoint.HasValue)
                {
                    var geom = geoShape.ToGeometry();
                    var sweref99TmGeom = geom.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
                    var bufferedGeomSweref99Tm = sweref99TmGeom.Buffer(maxDistanceFromPoint.Value);
                    var bufferedGeomWgs84 = bufferedGeomSweref99Tm.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
                    envelope = bufferedGeomWgs84.EnvelopeInternal;
                }
                else
                {
                    return;
                }
            }
            else
            {
                envelope = geoShape.ToGeometry().EnvelopeInternal;
            }


            if (envelope.IsNull)
            {
                return;
            }

            if (!bboxLeft.HasValue || envelope.MinX < bboxLeft)
            {
                bboxLeft = envelope.MinX;
            }
            if (!bboxRight.HasValue || envelope.MaxX > bboxRight)
            {
                bboxRight = envelope.MaxX;
            }
            if (!bboxBottom.HasValue || envelope.MinY < bboxBottom)
            {
                bboxBottom = envelope.MinY;
            }
            if (!bboxTop.HasValue || envelope.MaxY > bboxTop)
            {
                bboxTop = envelope.MaxY;
            }
        }

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tryToAutoAdjustBoundingBox"></param>
        /// <returns></returns>
        private async Task<Envelope> GetBoundingBoxAsync(
            GeographicsFilterDto filter,
            bool autoAdjustBoundingBox = true)
        {
            var bboxLeft = filter?.BoundingBox?.TopLeft?.Longitude;
            var bboxTop = filter?.BoundingBox?.TopLeft?.Latitude;
            var bboxRight = filter?.BoundingBox?.BottomRight?.Longitude;
            var bboxBottom = filter?.BoundingBox?.BottomRight?.Latitude;

            if (autoAdjustBoundingBox)
            {
                // If areas passed, adjust bounding box to them
                if (filter?.Areas?.Any() ?? false)
                {
                    var areas = await AreaManager.GetAreasAsync(filter.Areas.Select(a => (a.AreaType, a.FeatureId)));
                    var areaGeometries = areas?.Select(a => a.BoundingBox.GetPolygon().ToGeoShape());
                    //await _areaManager.GetGeometriesAsync(filter.Areas.Select(a => ((AreaType) a.AreaType, a.FeatureId)));
                    foreach (var areaGeometry in areaGeometries)
                    {
                        AdjustEnvelopeByShape(areaGeometry, ref bboxLeft, ref bboxTop, ref bboxRight, ref bboxBottom, filter.MaxDistanceFromPoint);
                    }
                }

                // If geometries passed, adjust bounding box to them
                if (filter?.Geometries?.Any() ?? false)
                {
                    foreach (var areaGeometry in filter.Geometries)
                    {
                        AdjustEnvelopeByShape(areaGeometry, ref bboxLeft, ref bboxTop, ref bboxRight, ref bboxBottom, filter.MaxDistanceFromPoint);
                    }
                }
            }

            // Get geometry of sweden economic zone
            var swedenGeometry = await AreaManager.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1");

            // Get bounding box of swedish economic zone
            var swedenBoundingBox = swedenGeometry.ToGeometry().EnvelopeInternal;

            // If bounding box misses one or more values
            if (!(bboxLeft.HasValue && bboxTop.HasValue && bboxRight.HasValue && bboxBottom.HasValue))
            {
                return autoAdjustBoundingBox ? swedenBoundingBox : null;
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

        /// <summary>
        ///  Validate signal search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="areaBuffer"></param>
        /// <returns></returns>
        private Result ValidateSignalSearch(SignalFilterDto filter, bool validateSearchFilter, int areaBuffer)
        {
            Result validateTaxonLists(TaxonFilterBaseDto filter)
            {
                if (!filter?.TaxonListIds?.Any() ?? true)
                {
                    return Result.Failure("You have to provide at least one taxon list id");
                }

                var containsMandatoryTaxonList = filter.TaxonListIds.Any(tlid => _signalSearchTaxonListIds.Contains(tlid));
                if (!containsMandatoryTaxonList)
                {
                    return Result.Failure("You have to provide at least one mandatory signal search taxon list");
                }

                return Result.Success();
            }

            return Result.Combine(
                validateSearchFilter ? ValidateTaxa(filter?.Taxon?.Ids) : Result.Success(),
                ValidateGeographicalAreaExists(filter?.Geographics),
                areaBuffer < 0 || areaBuffer > 100
                    ? Result.Failure("areaBuffer must be between 0 and 100")
                    : Result.Success(),
                filter?.StartDate > DateTime.Now.AddYears(-1) ? Result.Failure("Start date must be at least one year back in time") : Result.Success(),
                validateTaxonLists(filter?.Taxon)
            );
        }

        /// <summary>
        /// Validate metric tiles limit
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="gridCellSizeInMeters "></param>
        /// <returns></returns>
        private Result ValidateMetricTilesLimit(
            Envelope envelope,
            int gridCellSizeInMeters)
        {

            if (envelope == null)
            {
                return Result.Success();
            }

            var maxLonTiles = Math.Ceiling((envelope.MaxX - envelope.MinX) / gridCellSizeInMeters);
            var maxLatTiles = Math.Ceiling((envelope.MaxY - envelope.MinY) / gridCellSizeInMeters);
            var maxTilesTot = maxLonTiles * maxLatTiles;

            if (maxTilesTot > _tilesLimit)
            {
                return Result.Failure($"The number of cells that can be returned is too large. The limit is {_tilesLimit} cells. Try using larger grid cell size or a smaller bounding box.");
            }

            return Result.Success();
        }

        /// <summary>
        /// Validate tiles limit
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        private Result ValidateTilesLimit(
            Envelope envelope,
            int zoom)
        {
            var maxTilesTot = envelope.CalculateNumberOfTiles(zoom);

            if (maxTilesTot > _tilesLimit)
            {
                return Result.Failure($"The number of cells that can be returned is too large. The limit is {_tilesLimit} cells. Try using lower zoom or a smaller bounding box.");
            }

            return Result.Success();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationsController(
            IObservationManager observationManager,
            ITaxonSearchManager taxonSearchManager,
            ITaxonManager taxonManager,
            IAreaManager areaManager,
            ObservationApiConfiguration observationApiConfiguration,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ObservationsController> logger) : base(observationManager, areaManager, taxonManager, observationApiConfiguration)
        {
            _taxonSearchManager = taxonSearchManager ?? throw new ArgumentNullException(nameof(taxonSearchManager));
            _tilesLimit = elasticConfiguration?.MaxNrAggregationBuckets ??
                          throw new ArgumentNullException(nameof(observationApiConfiguration));

            _signalSearchTaxonListIds = (observationApiConfiguration?.SignalSearchTaxonListIds?.Any() ?? false) ? observationApiConfiguration.SignalSearchTaxonListIds : Array.Empty<int>();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a single observation.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="occurrenceId">The occurrence id of the observation to fetch.</param>
        /// <param name="id">Preferred way to pass occurrence id. Override occurrenceId passed in query if any</param>
        /// <param name="outputFieldSet">Define response output. Return Minimum, Extended or All properties</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">
        /// If true, and the requested observation is sensitive (protected), then the original data will be returned (this requires authentication and authorization).
        /// If false, and the requested observation is sensitive (protected), then diffused data will be returned.
        /// </param>
        /// <returns></returns>
        [HttpGet("{id?}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetObservationById(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromRoute] string id,
            [FromQuery] string occurrenceId,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                occurrenceId = WebUtility.UrlDecode(occurrenceId ?? id);

                var observation = await ObservationManager.GetObservationAsync(UserId, roleId, authorizationApplicationIdentifier, occurrenceId, outputFieldSet, translationCultureCode, sensitiveObservations,
                    includeInternalFields: false, false);

                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                HttpContext.LogObservationCount(1);
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
        /// Count the number of observations matching the provided search filter.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, default, public available observations will be searched.</param>
        /// <returns>The number of observations matching the provided search filter.</returns>
        [HttpPost("Count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Count(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterBaseDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success());
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, "sv-SE");
                var matchCount = await ObservationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter);

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
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("GeoGridAggregation")]
        [ProducesResponseType(typeof(GeoGridResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GeogridAggregation(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    ValidateTilesLimit(boundingBox, zoom));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, translationCultureCode);
                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await ObservationManager.GetGeogridTileAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier, searchFilter, zoom);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                var dto = result.Value.ToGeoGridResultDto(boundingBox.CalculateNumberOfTiles(zoom));
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
        /// Aggregates observations into grid cells. Each grid cell contains the number
        /// of observations and the number of unique taxa (usually species) in the grid cell.
        /// The grid cells are squares in SWEREF 99 TM coordinate system 
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="gridCellSizeInMeters">Size of grid cell in meters</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("MetricGridAggregation")]
        [ProducesResponseType(typeof(GeoGridMetricResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MetricGridAggregationAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int gridCellSizeInMeters = 100000,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateGridCellSizeInMetersArgument(gridCellSizeInMeters, minLimit: 100, maxLimit: 100000),
                    ValidateMetricTilesLimit(boundingBox.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, "en-GB");
                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await ObservationManager.GetMetricGridAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier, searchFilter, gridCellSizeInMeters);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                var dto = result.Value.ToDto();
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Metric grid aggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get observations matching the provided search filter. Permitted filter values depends on the specific filter field:
        ///     Some values are retrieved from the vocabularies endpoint. Some are defined as enum values. Some values are defined in other systems, e.g. Dyntaxa taxon id's.
        ///     Some are defined by the range of the underlying data type.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Application identifier making the request, used to get proper authorization</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of observations to return. Max is 1000 observations in each request.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
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
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    ValidateSearchPagingArguments(skip, take),
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateTranslationCultureCode(translationCultureCode));
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);
                SearchFilter searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, translationCultureCode);
                var result = await ObservationManager.GetChunkAsync(roleId, authorizationApplicationIdentifier, searchFilter, skip, take, sortBy, sortOrder);
                HttpContext.LogObservationCount(result?.Records?.Count() ?? 0);
                PagedResultDto<dynamic> dto = result?.ToPagedResultDto(result.Records);
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
        /// Aggregates observations by taxon. Each record contains TaxonId and the number of observations (ObservationCount) matching the search criteria.
        /// The records are ordered by ObservationCount in descending order.
        /// To get the first 100 taxa with the most observations, set skip to 0 and take to 100.
        /// You can only get the first 1000 taxa by using paging. To retrieve all records, set skip and take parameters to null.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("TaxonAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> TaxonAggregation(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);

                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateTaxonAggregationPagingArguments(skip, take),
                    ValidateTilesLimit(boundingBox, 1));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, translationCultureCode);

                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await _taxonSearchManager.GetTaxonAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    searchFilter,
                    skip,
                    take);
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
        /// Gets a single observation, including internal fields.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="occurrenceId">The occurrence id of the observation to fetch.</param>
        /// <param name="id">Preferred way to pass occurrence id. Override occurrenceId passed in query if any</param>
        /// <param name="outputFieldSet">Define response output. Return Minimum, Extended or All properties</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, sensitive observations index is queried if you have access rights.</param>
        /// <param name="ensureArtportalenUpdated">
        /// If true, a harvest and process job for that observation will be enqued to Hangfire, this action will wait for and return the updated result.
        /// </param>        
        /// <returns></returns>
        [HttpGet("Internal/{id?}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetObservationByIdInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromRoute] string id,
            [FromQuery] string occurrenceId,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool ensureArtportalenUpdated = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                occurrenceId = WebUtility.UrlDecode(occurrenceId ?? id);
                var observation = await ObservationManager.GetObservationAsync(
                    UserId,
                    roleId,
                    authorizationApplicationIdentifier, occurrenceId, outputFieldSet, translationCultureCode, sensitiveObservations,
                    includeInternalFields: true, ensureArtportalenUpdated);
                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                HttpContext.LogObservationCount(1);
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
        /// Count matching observations using internal filter
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/Count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> CountInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalBaseDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success());

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilterInternal(UserId, sensitiveObservations, "sv-SE");
                var matchCount = await ObservationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter);

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
        /// Count the number of present observations for the specified taxon. This endpoint uses caching to improve performance.
        /// </summary>
        /// <param name="taxonId">Count present observations for this taxon.</param>        
        /// <returns></returns>
        [HttpGet("Internal/CachedCount")]
        [ProducesResponseType(typeof(TaxonSumAggregationItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> CachedCountInternal(
            [FromQuery] int taxonId)
        {
            try
            {
                var result = await _taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(new int[] { taxonId });
                if (!result.Any())
                    return NoContent();

                return new OkObjectResult(result.First());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cached count error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count the number of present observations for the specified taxa. This endpoint uses caching to improve performance.
        /// </summary>
        /// <param name="taxonIds">Count present observations for these taxa.</param>
        /// <returns></returns>
        [HttpPost("Internal/CachedCount")]
        [ProducesResponseType(typeof(IEnumerable<TaxonSumAggregationItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> MultipleCachedCountInternal(
            [FromBody] IEnumerable<int> taxonIds)
        {
            try
            {
                var result = await _taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(taxonIds);
                if (!result.Any())
                    return NoContent();

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Multiple cached count error");
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
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridAggregation")]
        [ProducesResponseType(typeof(GeoGridResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridAggregationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);

                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    ValidateTilesLimit(boundingBox, zoom));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, sensitiveObservations, translationCultureCode);
                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await ObservationManager.GetGeogridTileAggregationAsync(roleId, authorizationApplicationIdentifier, searchFilter, zoom);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                GeoGridResultDto dto = result.Value.ToGeoGridResultDto(boundingBox.CalculateNumberOfTiles(zoom));
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
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridAggregationGeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    ValidateTilesLimit(boundingBox, zoom));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, translationCultureCode);

                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await ObservationManager.GetGeogridTileAggregationAsync(roleId, authorizationApplicationIdentifier, searchFilter, zoom);
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
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="geoTilePage">The GeoTile key used to retrieve the next next page of data. Should be null in the first request.</param>
        /// <param name="taxonIdPage">The TaxonId key used to retrieve the next page of data. Should be null in the first request.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <returns></returns>
        [HttpPost("Internal/GeoGridTaxaAggregation")]
        [ProducesResponseType(typeof(GeoGridTileTaxonPageResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GeogridTaxaAggregationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] string geoTilePage = null,
            [FromQuery] int? taxonIdPage = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE")
        {
            try
            {
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    ValidateTilesLimit(boundingBox, zoom));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, false, translationCultureCode);

                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await _taxonSearchManager.GetPageGeoTileTaxaAggregationAsync(roleId, authorizationApplicationIdentifier, filter.ToSearchFilterInternal(UserId, false, translationCultureCode), zoom, geoTilePage, taxonIdPage);
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
        /// Aggregates observations into grid cells. Each grid cell contains the number
        /// of observations and the number of unique taxa (usually species) in the grid cell.
        /// The grid cells are squares in SWEREF 99 TM coordinate system 
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="gridCellSizeInMeters">Size of grid cell in meters</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/MetricGridAggregation")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MetricGridAggregationInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int gridCellSizeInMeters = 10000,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] OutputFormatDto outputFormat = OutputFormatDto.Json)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateGridCellSizeInMetersArgument(gridCellSizeInMeters, minLimit: 100, maxLimit: 100000),
                    ValidateMetricTilesLimit(boundingBox.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, "en-GB");
                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await ObservationManager.GetMetricGridAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier, searchFilter, gridCellSizeInMeters);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                var dto = result.Value.ToDto();
                return new OkObjectResult(outputFormat == OutputFormatDto.Json ? dto : dto.ToGeoJson());
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Metric grid aggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get observations matching the provided search filter. Permitted filter values depends on the specific filter field:
        ///     Some values are retrieved from the vocabularies endpoint. Some are defined as enum values. Some values are defined in other systems, e.g. Dyntaxa taxon id's.
        ///     Some are defined by the range of the underlying data type.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of observations to return.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <param name="outputFormat">Select output format: JSON, GeoJSON with hierarchical properties, GeoJSON with flattened properties.</param>        
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
        [ProducesResponseType(typeof(GeoPagedResultDto<Observation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> ObservationsBySearchInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] OutputFormatDto outputFormat = OutputFormatDto.Json)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateSearchPagingArgumentsInternal(skip, take),
                    ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);
                if (outputFormat == OutputFormatDto.GeoJson || outputFormat == OutputFormatDto.GeoJsonFlat)
                {
                    var outPutFields = EnsureCoordinatesIsRetrievedFromDb(filter?.Output?.Fields);

                    if (outPutFields?.Any() ?? false)
                    {
                        if (filter.Output == null)
                        {
                            filter.Output = new OutputFilterDto();
                        }

                        filter.Output.Fields = EnsureCoordinatesIsRetrievedFromDb(filter?.Output?.Fields);
                    }
                }
                var result = await ObservationManager.GetChunkAsync(roleId, authorizationApplicationIdentifier, filter.ToSearchFilterInternal(UserId, sensitiveObservations, translationCultureCode), skip, take, sortBy, sortOrder);
                HttpContext.LogObservationCount(result?.Records?.Count() ?? 0);
                GeoPagedResultDto<dynamic> dto = result.ToGeoPagedResultDto(result.Records, outputFormat);
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

        private ICollection<string> EnsureCoordinatesIsRetrievedFromDb(ICollection<string> outputFields)
        {
            if (outputFields == null || !outputFields.Any())
            {
                return outputFields;
            }

            if (outputFields.Contains("location", StringComparer.OrdinalIgnoreCase) || (
                outputFields.Contains("location.decimalLongitude", StringComparer.OrdinalIgnoreCase) &&
                outputFields.Contains("location.decimalLatitude", StringComparer.OrdinalIgnoreCase)))
            {
                return outputFields;
            }

            var newList = new List<string>(outputFields);
            if (!newList.Contains("location.decimalLongitude", StringComparer.OrdinalIgnoreCase))
            {
                newList.Add("location.decimalLongitude");
            }
            if (!newList.Contains("location.decimalLatitude", StringComparer.OrdinalIgnoreCase))
            {
                newList.Add("location.decimalLatitude");
            }

            return newList;
        }

        /// <summary>
        /// Aggregate observations by the specified aggregation type.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of records to return.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/SearchAggregated")]
        [ProducesResponseType(typeof(PagedResultDto<dynamic>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> SearchAggregatedInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] AggregationType aggregationType,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    ValidateAggregationPagingArguments(skip, take, true),
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                // If it's a date histogram and default max number of buckets in elastic can be reached
                if (aggregationType.IsWeekHistogram() &&
                    ((filter?.Date?.EndDate ?? DateTime.Now) - (filter?.Date?.StartDate ?? new DateTime(1, 1, 1)))
                    .TotalDays / 7 >= 65536)
                {
                    return BadRequest(Result.Failure("You have to limit the time span. Use date.startDate and date.endDate to limit your request"));
                }

                var result = await ObservationManager.GetAggregatedChunkAsync(roleId, authorizationApplicationIdentifier, filter.ToSearchFilterInternal(UserId, sensitiveObservations, translationCultureCode), aggregationType, skip, take);
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
        ///     Get observations matching the provided search filter. This endpoint allows to retrieve up to 100 000 observations by using Elasticsearch scroll API.
        ///     Timeout between calls are two minutes.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="scrollId">The scroll id to use to get next batch. In first request scrollId should be empty.</param>
        /// <param name="take">Max number of observations to return. Max is 10 000 observations in each request.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="sensitiveObservations">If true, only protected observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
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
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] string scrollId,
            [FromQuery] int take = 5000,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                const int maxTotalCount = 100000;
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    take <= 10000 ? Result.Success() : Result.Failure("You can't take more than 10 000 at a time."),
                    ValidatePropertyExists(nameof(sortBy), sortBy),
                    ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                SearchFilter searchFilter = filter.ToSearchFilter(UserId, sensitiveObservations, translationCultureCode);
                var result = await ObservationManager.GetObservationsByScrollAsync(roleId, authorizationApplicationIdentifier, searchFilter, take, sortBy, sortOrder, scrollId);
                if (result.TotalCount > maxTotalCount)
                {
                    return BadRequest($"Scroll total count limit is maxTotalCount. Your result is {result.TotalCount}. Try use a more specific filter.");
                }
                HttpContext.LogObservationCount(result?.Records?.Count() ?? 0);
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

        /// <inheritdoc />
        [HttpPost("Internal/SignalSearch")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> SignalSearchInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SignalFilterDto filter,
            [FromQuery] bool validateSearchFilter = false, // if false, only mandatory requirements will be validated
            [FromQuery] int areaBuffer = 0,
            [FromQuery] bool onlyAboveMyClearance = true)
        {
            try
            {
                CheckAuthorization(true);

                var validationResult = Result.Combine(
                    ValidateSignalSearch(filter, validateSearchFilter, areaBuffer));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, true);
                var taxonFound = await ObservationManager.SignalSearchInternalAsync(roleId, authorizationApplicationIdentifier, searchFilter, areaBuffer, onlyAboveMyClearance);

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

        /// <summary>
        /// Aggregates observations by taxon. Each record contains TaxonId and the number of observations (ObservationCount) matching the search criteria.
        /// The records are ordered by ObservationCount in descending order.
        /// To get the first 100 taxa with the most observations, set skip to 0 and take to 100.
        /// You can only get the first 1000 taxa by using paging. To retrieve all records, set skip and take parameters to null.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <param name="sumUnderlyingTaxa">If true, the observation count will be the sum of all underlying taxa observation count, otherwise it will be the count for the specific taxon.</param>        
        /// <returns></returns>
        [HttpPost("Internal/TaxonAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> TaxonAggregationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sumUnderlyingTaxa = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var boundingBox = await GetBoundingBoxAsync(filter?.Geographics);
                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTranslationCultureCode(translationCultureCode),
                    ValidateTaxonAggregationPagingArguments(skip, take),
                    ValidateTilesLimit(boundingBox, 1));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, sensitiveObservations, translationCultureCode);
                searchFilter.OverrideBoundingBox(LatLonBoundingBox.Create(boundingBox));

                var result = await _taxonSearchManager.GetTaxonAggregationAsync(roleId,
                    authorizationApplicationIdentifier,
                    searchFilter,
                    skip,
                    take,
                    sumUnderlyingTaxa);

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
        /// Aggregates present observations by taxon (absent observations are excluded).
        /// The resulting items also contains sum of underlying taxa observation count.
        /// To get the first 100 taxa with the most observations, set skip to 0 and take to 100.
        /// To retrieve all records, set skip and take parameters to null.
        /// </summary>
        /// <param name="taxonFilter">The taxon filter.</param>        
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="sortBy">Sort by one of the following field: SumObservationCount, ObservationCount, SumProvinceCount, ProvinceCount.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <returns></returns>
        [HttpPost("Internal/TaxonSumAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonSumAggregationItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> TaxonSumAggregationInternal(
            [FromBody] TaxonFilterDto taxonFilter,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] string sortBy = "SumObservationCount",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Desc)
        {
            try
            {
                var result = await _taxonSearchManager.GetTaxonSumAggregationAsync(
                    UserId,
                    taxonFilter.ToTaxonFilterFilter(),
                    skip,
                    take,
                    sortBy,
                    sortOrder);

                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                var dto = result.Value.ToPagedResultDto(result.Value.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonSumAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get an indication of how many observations exist for the taxa specified in the search criteria filter.
        /// If protectedObservations is set to false, you must be aware of that the result can include false positives
        /// since the protected observations coordinates are generalized to a grid depending on the protection level.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/TaxonExistsIndication")]
        [ProducesResponseType(typeof(IEnumerable<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> TaxonExistsIndicationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var validationResult = Result.Combine(
                    validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success(),
                    ValidateTaxonExists(filter),
                    ValidateGeographicalAreaExists(filter?.Geographics));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, sensitiveObservations, "sv-SE");
                var taxonFound = await _taxonSearchManager.GetTaxonExistsIndicationAsync(roleId, authorizationApplicationIdentifier, searchFilter);

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

        [HttpPost("Internal/CurrentUser/YearCount")]
        [ProducesResponseType(typeof(IEnumerable<YearCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserYearCountAsync(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false)
        {
            try
            {
                var validationResult = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, true, "sv-SE");
                var yearCounts = await ObservationManager.GetUserYearCountAsync(searchFilter);

                return new OkObjectResult(yearCounts);
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
                _logger.LogError(e, "Failed to get user year count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("Internal/CurrentUser/YearMonthCount")]
        [ProducesResponseType(typeof(IEnumerable<YearMonthCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserYearMonthCountAsync(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false)
        {
            try
            {
                var validationResult = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, true, "sv-SE");
                var yearMonthCounts = await ObservationManager.GetUserYearMonthCountAsync(searchFilter);

                return new OkObjectResult(yearMonthCounts);
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
                _logger.LogError(e, "Failed to get user year month count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("Internal/CurrentUser/YearMonthDayCount")]
        [ProducesResponseType(typeof(IEnumerable<YearMonthDayCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserYearMonthDayCountAsync(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] bool validateSearchFilter = false)
        {
            try
            {
                var validationResult = validateSearchFilter ? ValidateSearchFilter(filter) : Result.Success();

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(UserId, true, "sv-SE");
                var yearMonthDayCounts = await ObservationManager.GetUserYearMonthDayCountAsync(searchFilter, skip, take);

                return new OkObjectResult(yearMonthDayCounts);
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
                _logger.LogError(e, "Failed to get user year month count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}