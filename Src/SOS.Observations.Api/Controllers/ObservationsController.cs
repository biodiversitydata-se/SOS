using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NGeoHash;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using BoundingBox = NGeoHash.BoundingBox;
using FieldMapping = SOS.Lib.Models.Shared.FieldMapping;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase, IObservationsController
    {
        private const int MaxBatchSize = 1000;
        private const int ElasticSearchMaxRecords = 10000;
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationsController> _logger;
        private readonly IObservationManager _observationManager;

        /// <summary>
        /// Basic validation of search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private Tuple<bool, IEnumerable<string>> ValidateFilter(SearchFilter filter, int skip, int take)
        {
            var errors = new List<string>();

            if (!filter.IsFilterActive)
            {
                errors.Add("You must provide a filter."); ;
            }
            else
            {

                // No culture code, set default
                if (string.IsNullOrEmpty(filter?.FieldTranslationCultureCode))
                {
                    filter.FieldTranslationCultureCode = "sv-SE";
                }

                if (!new[] { "sv-SE", "en-GB" }.Contains(filter.FieldTranslationCultureCode,
                    StringComparer.CurrentCultureIgnoreCase))
                {
                    errors.Add("Unknown FieldTranslationCultureCode. Supported culture codes, sv-SE, en-GB");
                }

                //Remove the limitations if we use the internal functions
                if (!(filter is SearchFilterInternal))
                {
                    if (skip < 0 || take <= 0 || take > MaxBatchSize)
                    {
                        errors.Add($"You can't take more than {MaxBatchSize} at a time.");
                    }
                }

                if (skip + take > ElasticSearchMaxRecords)
                {
                    errors.Add($"Skip + take can't be greater than { ElasticSearchMaxRecords }" );
                }
            }


            return new Tuple<bool, IEnumerable<string>>(!errors.Any(), errors);
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="logger"></param>
        public ObservationsController(
            IObservationManager observationManager,
            IFieldMappingManager fieldMappingManager,
            ILogger<ObservationsController> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResult<ProcessedObservation>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChunkAsync([FromBody] SearchFilter filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc)
        {
            try
            {
                var validateResult = ValidateFilter(filter, skip, take);
                if (!validateResult.Item1)
                {
                    return BadRequest( string.Join(". ", validateResult.Item2));
                }

                return new OkObjectResult(
                    await _observationManager.GetChunkAsync(filter, skip, take, sortBy, sortOrder));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of sightings");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("TermDictionary")]
        [ProducesResponseType(typeof(IEnumerable<FieldMapping>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFieldMappingAsync()
        {
            try
            {
                return new OkObjectResult(await _fieldMappingManager.GetFieldMappingsAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting field mappings");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("searchinternal")]
        [ProducesResponseType(typeof(PagedResult<ProcessedObservation>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetChunkInternalAsync([FromBody] SearchFilterInternal filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc)
        {
            var validateResult = ValidateFilter(filter, skip, take);
            if (!validateResult.Item1)
            {
                return BadRequest(string.Join(". ", validateResult.Item2));
            }

            return await GetChunkAsync(filter, skip, take, sortBy, sortOrder);
        }

        /// <inheritdoc />
        [HttpPost("searchaggregatedinternal")]
        [ProducesResponseType(typeof(PagedResult<ProcessedObservation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetChunkAggregatedInternalAsync([FromBody] SearchFilterInternal filter,
            [FromQuery] AggregationType aggregationType,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100, 
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc
            )
        {
            try
            {
                var (isValid, validationErrors) = ValidateFilter(filter, 0, 1);
                if (!isValid)
                {
                    return BadRequest(string.Join(". ", validationErrors));
                }

                var result = await _observationManager.GetAggregatedChunkAsync(filter, aggregationType, skip, take, sortBy, sortOrder);

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of aggregated sightings");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregate observations into grid cells.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="precision">
        /// The zoom precision. Must be between 1 and 9.
        /// 1 =	5,009.4km x 4,992.6km
        /// 2 = 1,252.3km x 624.1km
        /// 3 = 156.5km x 156km
        /// 4 = 39.1km x 19.5km
        /// 5 = 4.9km x 4.9km
        /// 6 = 1.2km x 609.4m
        /// 7 = 152.9m x 152.4m
        /// 8 = 38.2m x 19m
        /// 9 = 4.8m x 4.8m
        /// </param>
        /// <param name="bboxGeoHash">Bounding box as a GeoHash. E.g. "u6sc".</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <returns></returns>
        [HttpPost("geogridsearch")]
        [ProducesResponseType(typeof(GeoGridResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGeogridAsync([FromBody] SearchFilter filter,
            [FromQuery] int precision = 1,
            [FromQuery] string bboxGeoHash = null,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null
        )
        {
            try
            {
                var (isValid, validationErrors) = ValidateFilter(filter, 0, 1);
                if (!isValid)
                {
                    return BadRequest(string.Join(". ", validationErrors));
                }

                if (precision < 1 || precision > 9)
                {
                    return BadRequest("precision must be between 1 and 9");
                }

                Result<LatLonBoundingBox> bbox = GetBoundingBox(bboxGeoHash, bboxLeft, bboxTop, bboxRight, bboxBottom);
                if (bbox.IsFailure)
                {
                    return BadRequest(bbox.Error);
                }

                var result = await _observationManager.GetGeogridAggregationAsync(filter, precision, bbox.Value);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                return new OkObjectResult(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of aggregated sightings");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates observations into grid cells. Each grid cell contains the number
        /// of observations and the number of unique taxa (usually species) in the grid cell.
        /// The grid cells are squares in WGS84 coordinate system which means that they also
        /// will be squares in the Web Mercator coordinate system.
        /// </summary>
        /// <remarks>
        /// If you choose to convert the coordinates into SWEREF99TM the squares will be of
        /// different size in different places in Sweden.
        ///
        /// The following table shows the grid cell size (width) in different
        /// coordinate systems for the different zoom levels.
        /// +------------+----------+---------------+-------------------+----------------+
        /// | Zoom level | WGS84    | Web Mercator  |     SWEREF99TM    |   SWEREF99TM   |
        /// |            |          |               | (Southern Sweden) | (North Sweden) |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 1          |      180 |       20038km |            7813km |        11656km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 2          |       90 |       10019km |            3906km |         5828km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 3          |       45 |        5009km |            1953km |         2914km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 4          |     22.5 |        2505km |             977km |         1457km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 5          |    11.25 |        1252km |             488km |          729km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 6          |    5.625 |         626km |             244km |          364km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 7          |   2.8125 |         313km |             122km |          182km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 8          | 1.406250 |         157km |              61km |           91km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 9          | 0.703125 |          78km |              30km |           46km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 10         | 0.351563 |          39km |              15km |           23km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 11         | 0.175781 |          20km |             7630m |           11km |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 12         | 0.087891 |          10km |             3815m |          5692m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 13         | 0.043945 |         4892m |             1907m |          2846m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 14         | 0.021973 |         2446m |              954m |          1423m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 15         | 0.010986 |         1223m |              477m |           711m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 16         | 0.005493 |          611m |              238m |           356m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 17         | 0.002747 |          306m |              119m |           178m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 18         | 0.001373 |          153m |               60m |            89m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 19         | 0.000687 |           76m |               30m |            44m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 20         | 0.000343 |           38m |               15m |            22m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// | 21         | 0.000172 |           19m |                7m |            11m |
        /// +------------+----------+---------------+-------------------+----------------+
        /// </remarks>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxGeoHash">Bounding box as a GeoHash. E.g. "u6sc".</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <returns></returns>
        [HttpPost("geogridtilesearch")]
        [ProducesResponseType(typeof(GeoGridTileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGeogridTileAsync([FromBody] SearchFilter filter,
            [FromQuery] int zoom = 1,
            [FromQuery] string bboxGeoHash = null,
            [FromQuery] double? bboxLeft = null,
            [FromQuery] double? bboxTop = null,
            [FromQuery] double? bboxRight = null,
            [FromQuery] double? bboxBottom = null
        )
        {
            try
            {
                var (isValid, validationErrors) = ValidateFilter(filter, 0, 1);
                if (!isValid)
                {
                    return BadRequest(string.Join(". ", validationErrors));
                }

                if (zoom < 1 || zoom > 21)
                {
                    return BadRequest("Zoom must be between 1 and 21");
                }

                Result<LatLonBoundingBox> bbox = GetBoundingBox(bboxGeoHash, bboxLeft, bboxTop, bboxRight, bboxBottom);
                if (bbox.IsFailure)
                {
                    return BadRequest(bbox.Error);
                }

                var result = await _observationManager.GetGeogridTileAggregationAsync(filter, zoom, bbox.Value);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                return new OkObjectResult(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of aggregated sightings");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates observations into grid cells and returns a GeoJSON file with all grid cells.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="zoom">A zoom level between 1 and 21.</param>
        /// <param name="bboxGeoHash">Bounding box as a GeoHash. E.g. "u6sc".</param>
        /// <param name="bboxLeft">Bounding box left (longitude) coordinate in WGS84.</param>
        /// <param name="bboxTop">Bounding box top (latitude) coordinate in WGS84.</param>
        /// <param name="bboxRight">Bounding box right (longitude) coordinate in WGS84.</param>
        /// <param name="bboxBottom">Bounding box bottom (latitude) coordinate in WGS84.</param>
        /// <returns></returns>
        [HttpPost("geogridgeojson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetGeogridGeoJsonInternalAsync([FromBody] SearchFilter filter,
                    [FromQuery] int zoom = 1,
                    [FromQuery] string bboxGeoHash = null,
                    [FromQuery] double? bboxLeft = null,
                    [FromQuery] double? bboxTop = null,
                    [FromQuery] double? bboxRight = null,
                    [FromQuery] double? bboxBottom = null
                )
        {
            try
            {
                var (isValid, validationErrors) = ValidateFilter(filter, 0, 1);
                if (!isValid)
                {
                    return BadRequest(string.Join(". ", validationErrors));
                }

                if (zoom < 1 || zoom > 21)
                {
                    return BadRequest("Zoom must be between 1 and 21");
                }

                Result<LatLonBoundingBox> bbox = GetBoundingBox(bboxGeoHash, bboxLeft, bboxTop, bboxRight, bboxBottom);
                if (bbox.IsFailure)
                {
                    return BadRequest(bbox.Error);
                }

                var result = await _observationManager.GetGeogridTileAggregationAsync(filter, zoom, bbox.Value);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                string strJson = result.Value.GetFeatureCollectionGeoJson();
                var bytes = Encoding.UTF8.GetBytes(strJson);
                return File(bytes, "application/json", "grid.geojson");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of aggregated sightings");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private static Result<LatLonBoundingBox> GetBoundingBox(string bboxGeoHash, double? bboxLeft, double? bboxTop,
            double? bboxRight, double? bboxBottom)
        {
            LatLonBoundingBox bbox;
            if (bboxLeft.HasValue && bboxTop.HasValue && bboxRight.HasValue && bboxBottom.HasValue)
            {
                bbox = new LatLonBoundingBox
                {
                    TopLeft = new LatLonCoordinate(bboxTop.Value, bboxLeft.Value),
                    BottomRight = new LatLonCoordinate(bboxBottom.Value, bboxRight.Value)
                };
            }
            else if (!string.IsNullOrWhiteSpace(bboxGeoHash))
            {
                BoundingBox geoHashBbox;
                try
                {
                    geoHashBbox = GeoHash.DecodeBbox(bboxGeoHash);
                }
                catch (Exception)
                {
                    return Result.Failure<LatLonBoundingBox>("bboxGeoHash is invalid");
                }

                bbox = new LatLonBoundingBox
                {
                    GeoHash = bboxGeoHash,
                    TopLeft = new LatLonCoordinate(geoHashBbox.Maximum.Lat, geoHashBbox.Minimum.Lon),
                    BottomRight = new LatLonCoordinate(geoHashBbox.Minimum.Lat, geoHashBbox.Maximum.Lon)
                };
            }
            else
            {
                bbox = new LatLonBoundingBox
                {
                    TopLeft = new LatLonCoordinate(90, -180),
                    BottomRight = new LatLonCoordinate(-90, 180)
                };
            }

            return Result.Success(bbox);
        }
    }
}