using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using SOS.Analysis.Api.Configuration;
using SOS.Analysis.Api.Controllers.Interfaces;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Swagger;
using SOS.Shared.Api.Configuration;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Dtos.Search;
using SOS.Shared.Api.Extensions.Controller;
using SOS.Shared.Api.Extensions.Dto;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Validators.Interfaces;
using System.Net;
using Result = CSharpFunctionalExtensions.Result;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SOS.Analysis.Api.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IAnalysisController"/>.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase, IAnalysisController
    {
        private readonly IAnalysisManager _analysisManager;
        private readonly ISearchFilterUtility _searchFilterUtility;
        private readonly IInputValidator _inputValidator;
        private readonly AnalysisConfiguration _analysisConfiguration;
        private readonly ILogger<AnalysisController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="analysisManager"></param>
        /// <param name="searchFilterUtility"></param>
        /// <param name="inputValidator"></param>
        /// <param name="analysisConfiguration"></param>
        /// <param name="inputValaidationConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisController(
            IAnalysisManager analysisManager,
            ISearchFilterUtility searchFilterUtility,
            IInputValidator inputValidator,
            AnalysisConfiguration analysisConfiguration,
            InputValaidationConfiguration inputValaidationConfiguration,
            ILogger<AnalysisController> logger) 
        {
            _analysisManager = analysisManager ?? throw new ArgumentNullException(nameof(analysisManager));
            _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
            _searchFilterUtility = searchFilterUtility ?? throw new ArgumentNullException(nameof(searchFilterUtility));
            _analysisConfiguration = analysisConfiguration ?? throw new ArgumentNullException(nameof(analysisConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("/internal/aggregation")]
        [ProducesResponseType(typeof(PagedAggregationResultDto<UserAggregationResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> AggregateAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] string aggregationField,
            [FromQuery] int? precisionThreshold,
            [FromQuery] string? afterKey,
            [FromQuery] int? take = 10)
        {
            try
            {
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var validationResult = Result.Combine(
                    _inputValidator.ValidateSearchFilter(searchFilter!), 
                    _inputValidator.ValidateFields(new[] { aggregationField })
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;
                var result = await _analysisManager.AggregateByUserFieldAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    aggregationField,
                    precisionThreshold,
                    afterKey,
                    take
                );

                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Aggregate by user field error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("/internal/aggregation_simple")]
        [ProducesResponseType(typeof(IEnumerable<AggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> AggregateByUserFieldAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] string aggregationField,
            [FromQuery] int? precisionThreshold,
            [FromQuery] int take = 10,
            [FromQuery] AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            try
            {
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);

                var validationResult = Result.Combine(
                    _inputValidator.ValidateSearchFilter(searchFilter!),
                    _inputValidator.ValidateFields(new[] { aggregationField }),
                    _inputValidator.ValidateInt(take, 1, 250, "take"));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                var result = await _analysisManager.AggregateByUserFieldAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    aggregationField,
                    precisionThreshold,
                    take,
                    sortOrder
                );

                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Aggregate by user field error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("/internal/atlas")]
        [ProducesResponseType(typeof(PagedAggregationResultDto<UserAggregationResponseDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> AtlasAggregateAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] AtlasAreaSizeDto atlasSize = AtlasAreaSizeDto.Km10x10)
        {
            try
            {
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var validationResult = _inputValidator.ValidateSearchFilter(searchFilter!);

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;
               
                var result = await _analysisManager.AtlasAggregateAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    atlasSize
                );

                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Atlas square aggregation failed.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="alphaValues">One or more alpha values used to calculate AOO and EEO</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="useCenterPoint">If true, grid cell center point will be used, else grid cell corner points will be used.</param>
        /// <param name="useEdgeLengthRatio">Change behavior of alpha values. When true: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum alpha ratio. 
        /// The alpha factor is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum edge length.</param>
        /// <param name="allowHoles">Gets or sets whether holes are allowed in the concave hull polygon.</param>
        /// <param name="returnGridCells">Return grid cells features</param>
        /// <param name="includeEmptyCells">Include grid cells with no observations</param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system</param>
        /// <returns></returns>
        [HttpPost("/internal/aoo_eoo")]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> CalculateAooAndEooInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] double[] alphaValues,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] bool? useCenterPoint = true,
            [FromQuery] bool? useEdgeLengthRatio = true,
            [FromQuery] bool? allowHoles = false,
            [FromQuery] bool? returnGridCells = false,
            [FromQuery] bool? includeEmptyCells = false,
            [FromQuery] MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] CoordinateSys? coordinateSystem = CoordinateSys.WGS84)
        {
            try
            {
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var edgeLengthValidation = Result.Success();
                if ((useEdgeLengthRatio ?? false) && (alphaValues?.Any() ?? false))
                {
                    foreach (var edgeLength in alphaValues)
                    {
                        edgeLengthValidation = _inputValidator.ValidateDouble(edgeLength, 0.0, 1.0, "Alpha value");
                        if (edgeLengthValidation.IsFailure)
                        {
                            break;
                        }
                    }
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                var validationResult = Result.Combine(
                    edgeLengthValidation,
                    alphaValues?.Any() ?? false ? Result.Success() : Result.Failure("You must state at least one alpha value"),
                    _inputValidator.ValidateSearchFilter(searchFilter!),
                    _inputValidator.ValidateInt(gridCellSizeInMeters!.Value, minLimit: 1000, maxLimit: 100000, "Grid cell size in meters"),
                    await _inputValidator.ValidateTilesLimitMetricAsync(
                        searchFilter!.Geographics!.BoundingBox!.ToEnvelope().Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), 
                        gridCellSizeInMeters.Value,
                        _analysisManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, filter),
                        true
                    ));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _analysisManager.CalculateAooAndEooAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    gridCellSizeInMeters!.Value,
                    useCenterPoint!.Value,
                    alphaValues!,
                    useEdgeLengthRatio!.Value,
                    allowHoles!.Value,
                    returnGridCells!.Value,
                    includeEmptyCells!.Value,
                    metricCoordinateSys!.Value,
                    coordinateSystem!.Value
                );
                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAooAndEoo error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="maxDistance">Max distance between occurrence grid cells</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system</param>
        /// <returns></returns>
        [HttpPost("/internal/aoo_eoo/article17")]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> CalculateAooAndEooArticle17InternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] int? maxDistance = 50000,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] CoordinateSys? coordinateSystem = CoordinateSys.WGS84)
        {
            try
            {
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                var validationResult = Result.Combine(

                    maxDistance > 0 ? Result.Success() : Result.Failure("You must state max distance"),
                    _inputValidator.ValidateInt(maxDistance.Value, minLimit: 1000, maxLimit: 50000, "Max distance in meters"),
                    _inputValidator.ValidateSearchFilter(searchFilter!),
                    _inputValidator.ValidateInt(gridCellSizeInMeters!.Value, minLimit: 1000, maxLimit: 100000, "Grid cell size in meters"),
                    await _inputValidator.ValidateTilesLimitMetricAsync(
                        searchFilter!.Geographics!.BoundingBox!.ToEnvelope().Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM),
                        gridCellSizeInMeters.Value,
                        _analysisManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, filter),
                        true
                    ));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _analysisManager.CalculateAooAndEooArticle17Async(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    gridCellSizeInMeters!.Value,
                    maxDistance!.Value,
                    metricCoordinateSys!.Value,
                    coordinateSystem!.Value
                );
                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAooAndEoo article 17 error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
