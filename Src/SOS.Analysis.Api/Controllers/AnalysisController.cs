using Microsoft.AspNetCore.Mvc;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Analysis.Api.Configuration;
using SOS.Analysis.Api.Controllers.Interfaces;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Analysis.Api.Dtos.Filter.Enums;
using SOS.Analysis.Api.Extensions.Dto;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System.Net;
using Result = CSharpFunctionalExtensions.Result;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SOS.Analysis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase, IAnalysisController
    {
        private readonly IAnalysisManager _analysisManager;
        private readonly IAreaCache _areaCache;
        private readonly AnalysisConfiguration _analysisConfiguration;
        private readonly ILogger<AnalysisController> _logger;
        private readonly int _tilesLimit;

        /// <summary>
        /// Make envelope as small as possible
        /// </summary>
        /// <param name="geoShape"></param>
        /// <param name="bboxLeft"></param>
        /// <param name="bboxTop"></param>
        /// <param name="bboxRight"></param>
        /// <param name="bboxBottom"></param>
        /// <param name="maxDistanceFromPoint"></param>
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

        private void CheckAuthorization(bool sensitiveObservations)
        {
            if (sensitiveObservations && (!User?.HasAccessToScope(_analysisConfiguration.ProtectedScope) ?? true))
            {
                throw new AuthenticationRequiredException("Not authorized");
            }
        }

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="autoAdjustBoundingBox"></param>
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
                    var areas = await _areaCache.GetAreasAsync(filter.Areas.Select(a => ((AreaType)a.AreaType, a.FeatureId)));
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
            var swedenGeometry = await _areaCache.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1");

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
        /// Get id of current user
        /// </summary>
        private int UserId => int.Parse(User?.Claims?.FirstOrDefault(c => c.Type.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value ?? "0");

        /// <summary>
        /// Validate areas
        /// </summary>
        /// <param name="areaIds"></param>
        /// <returns></returns>
        private async Task<Result> ValidateAreasAsync(IEnumerable<AreaFilterDto> areaKeys)
        {
            if (!areaKeys?.Any() ?? true)
            {
                return Result.Success();
            }
           
            var existingAreaIds = (await _areaCache.GetAreasAsync(areaKeys.Select(a => ((AreaType)a.AreaType, a.FeatureId))))
                .Select(a => new AreaFilterDto { AreaType = (AreaTypeDto)a.AreaType, FeatureId = a.FeatureId });

            var missingAreas = areaKeys?
                .Where(aid => !existingAreaIds.Any(a => a.AreaType.Equals(aid.AreaType) && a.FeatureId.Equals(aid.FeatureId, StringComparison.CurrentCultureIgnoreCase)))
                .Select(aid => $"Area doesn't exist (AreaType: {aid.AreaType}, FeatureId: {aid.FeatureId})");

            return missingAreas?.Any() ?? false ?
                Result.Failure(string.Join(". ", missingAreas))
                :
                Result.Success();
        }

        /// <summary>
        /// Validate bounding box
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        protected Result ValidateBoundingBox(
            LatLonBoundingBoxDto boundingBox,
            bool mandatory = false)
        {
            if (boundingBox == null)
            {
                return mandatory ? Result.Failure("Bounding box is missing.") : Result.Success();
            }

            if (boundingBox.TopLeft == null || boundingBox.BottomRight == null)
            {
                return Result.Failure("Bounding box is incomplete.");
            }

            if (boundingBox.TopLeft.Longitude >= boundingBox.BottomRight.Longitude)
            {
                return Result.Failure("Bounding box left longitude value is >= right longitude value.");
            }

            if (boundingBox.BottomRight.Latitude >= boundingBox.TopLeft.Latitude)
            {
                return Result.Failure("Bounding box bottom latitude value is >= top latitude value.");
            }

            return Result.Success();
        }

        private Result ValidateInt(int value, int minLimit, int maxLimit, string paramName)
        {
            if (value < minLimit || value > maxLimit)
            {
                return Result.Failure($"{paramName} must be between {minLimit} and {maxLimit}");
            }

            return Result.Success();
        }

        private Result ValidateDouble(double value, double minLimit, double maxLimit, string paramName)
        {
            if (value < minLimit || value > maxLimit)
            {
                return Result.Failure($"{paramName} must be between {minLimit} and {maxLimit}");
            }

            return Result.Success();
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
       /// Validate serach filter
       /// </summary>
       /// <param name="filter"></param>
       /// <param name="bboxMandatory"></param>
       /// <returns></returns>
        private Result ValidateSearchFilter(SearchFilterDto filter, bool bboxMandatory = false)
        {
            var errors = new List<string>();

            var areaValidationResult = ValidateAreasAsync(filter?.Geographics?.Areas).Result;
            if (areaValidationResult.IsFailure)
            {
                errors.Add(areaValidationResult.Error);
            }

            var bboxResult = ValidateBoundingBox(filter?.Geographics?.BoundingBox, bboxMandatory);
            if (bboxResult.IsFailure)
            {
                errors.Add(bboxResult.Error);
            }

            if (filter?.ModifiedDate?.From > filter?.ModifiedDate?.To)
            {
                errors.Add("Modified from date can't be greater tha to date");
            }

            if (filter.Taxon?.RedListCategories?.Any() ?? false)
            {
                errors.AddRange(filter.Taxon.RedListCategories
                    .Where(rc => !new[] { "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE" }.Contains(rc, StringComparer.CurrentCultureIgnoreCase))
                    .Select(rc => $"Red list category doesn't exist ({rc})"));
            }
            if (filter.Date?.DateFilterType == DateFilterTypeDto.OnlyStartDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyStartDate as filter both StartDate and EndDate need to be specified");
            }
            if (filter.Date?.DateFilterType == DateFilterTypeDto.OnlyEndDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyEndDate as filter both StartDate and EndDate need to be specified");
            }

            return errors.Any() ? Result.Failure(string.Join(". ", errors)) : Result.Success();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="analysisManager"></param>
        /// <param name="areaCache"></param>
        /// <param name="analysisConfiguration"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisController(
            IAnalysisManager analysisManager,
            IAreaCache areaCache,
            AnalysisConfiguration analysisConfiguration,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<AnalysisController> logger)
        {
            _analysisManager = analysisManager ?? throw new ArgumentNullException(nameof(analysisManager));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _analysisConfiguration = analysisConfiguration ?? throw new ArgumentNullException(nameof(analysisConfiguration));
            _tilesLimit = elasticConfiguration?.MaxNrAggregationBuckets ??
                         throw new ArgumentNullException(nameof(elasticConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        [HttpPost("AOO_EOO")]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CalculateAooAndEooAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto searchFilter,
            [FromQuery] bool? sensitiveObservations = false,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] bool? useCenterPoint = true,
            [FromQuery] double? edgeLength = 1000,
            [FromQuery] bool? useEdgeLengthRatio = false,
            [FromQuery] bool? allowHoles = false,
            CoordinateSys? coordinateSystem = CoordinateSys.ETRS89)
        {
            try
            {
                CheckAuthorization(sensitiveObservations!.Value);

                var boundingBox = await GetBoundingBoxAsync(searchFilter?.Geographics!);
                var validationResult = Result.Combine(
                    useEdgeLengthRatio ?? false ? ValidateDouble(edgeLength!.Value, 0.0, 1.0, "Edge length") : Result.Success(),
                    ValidateSearchFilter(searchFilter!),
                    ValidateInt(gridCellSizeInMeters!.Value, minLimit: 100, maxLimit: 100000, "Grid cell size in meters"),
                    ValidateMetricTilesLimit(boundingBox.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters.Value));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _analysisManager.CalculateAooAndEooAsync(
                    searchFilter.ToSearchFilter(UserId, sensitiveObservations.Value, "sv-SE"), 
                    gridCellSizeInMeters!.Value, 
                    useCenterPoint!.Value, 
                    edgeLength!.Value, 
                    useEdgeLengthRatio!.Value, 
                    allowHoles!.Value, 
                    coordinateSystem!.Value
                );
                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAooAndEoo error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
