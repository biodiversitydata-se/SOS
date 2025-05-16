using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using SOS.Analysis.Api.Configuration;
using SOS.Analysis.Api.Controllers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Swagger;
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
        private readonly ICryptoService _cryptoService;
        private readonly IUserExportRepository _userExportRepository;
        private readonly int _defaultUserExportLimit;
        private readonly ILogger<AnalysisController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="analysisManager"></param>
        /// <param name="searchFilterUtility"></param>
        /// <param name="inputValidator"></param>
        /// <param name="analysisConfiguration"></param>
        /// <param name="cryptoService"></param>
        /// <param name="userExportRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisController(
            IAnalysisManager analysisManager,
            ISearchFilterUtility searchFilterUtility,
            IInputValidator inputValidator,
            AnalysisConfiguration analysisConfiguration,
            ICryptoService cryptoService,
            IUserExportRepository userExportRepository,
            ILogger<AnalysisController> logger) 
        {
            _analysisManager = analysisManager ?? throw new ArgumentNullException(nameof(analysisManager));
            _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
            _searchFilterUtility = searchFilterUtility ?? throw new ArgumentNullException(nameof(searchFilterUtility));
            _analysisConfiguration = analysisConfiguration ?? throw new ArgumentNullException(nameof(analysisConfiguration));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _userExportRepository =
                userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
            _defaultUserExportLimit = analysisConfiguration?.DefaultUserExportLimit ?? throw new ArgumentNullException(nameof(analysisConfiguration));
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
            [FromQuery] bool? validateFilter,
            [FromQuery] string aggregationField,
            [FromQuery] bool? aggregateOrganismQuantity,
            [FromQuery] int? precisionThreshold,
            [FromQuery] string? afterKey,
            [FromQuery] int? take = 10)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var validationResult = Result.Combine(
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
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
                    aggregateOrganismQuantity.GetValueOrDefault(false),
                    precisionThreshold,
                    afterKey,
                    take
                );

                PagedAggregationResultDto<UserAggregationResponseDto> dto = result.ToDto();
                return new OkObjectResult(dto);
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
        [ProducesResponseType(typeof(IEnumerable<AggregationItemOrganismQuantityDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> AggregateByUserFieldAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] bool? validateFilter,
            [FromQuery] string aggregationField,
            [FromQuery] bool? aggregateOrganismQuantity,
            [FromQuery] int? precisionThreshold,
            [FromQuery] int take = 10,
            [FromQuery] AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);

                var validationResult = Result.Combine(
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
                    _inputValidator.ValidateFields(new[] { aggregationField }),
                    _inputValidator.ValidateInt(take, 1, 250, "take"),
                    !aggregateOrganismQuantity.GetValueOrDefault(false) && (sortOrder == AggregationSortOrder.OrganismQuantityAscending || sortOrder == AggregationSortOrder.OrganismQuantityDescending)
                        ? Result.Failure("Sort order cannot use organism quantity when aggregateOrganismQuantity=false")
                        : Result.Success()
                    );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                IEnumerable<AggregationItemOrganismQuantity> result = await _analysisManager.AggregateByUserFieldAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    aggregationField,
                    aggregateOrganismQuantity.GetValueOrDefault(false),
                    precisionThreshold,
                    take,
                    sortOrder
                );

                var dto = result.ToDto();
                return new OkObjectResult(dto);
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

        [HttpPost("/internal/areas/aggregation")]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> AreaAggregateAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] AreaTypeAggregate areaType,
            [FromQuery] CoordinateSys? coordinateSys,
            [FromQuery] int? precisionThreshold,
            [FromQuery] bool? aggregateOrganismQuantity,
            [FromQuery] bool? validateFilter)
        {

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var validationResult = validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success();

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                var result = await _analysisManager.AreaAggregateAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    areaType,
                    precisionThreshold,
                    aggregateOrganismQuantity,
                    coordinateSys);

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
        /// <param name="validateFilter"></param>
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
            [FromQuery] bool? validateFilter,
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
                    alphaValues?.Any() ?? false ? Result.Success() : Result.Failure("You must state at least one alpha value"),
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

                (FeatureCollection FeatureCollection, List<Lib.Models.Analysis.AooEooItem> AooEooItems)? result = await _analysisManager.CalculateAooAndEooAsync(
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
                if (result == null)
                    return new OkObjectResult(null);

                return new OkObjectResult(result!.Value.FeatureCollection);
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
        /// Starts the process of creating a zip file with AOO and EOO based on the provided filter.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="searchFilter">The search filter.</param>
        /// <param name="validateFilter"></param>
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
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <returns></returns>
        [HttpPost("/internal/order/aoo_eoo")]
        [Authorize]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> OrderAooAndEooInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromQuery] string description,            
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] bool? validateFilter,
            [FromQuery] double[] alphaValues,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] bool? useCenterPoint = true,
            [FromQuery] bool? useEdgeLengthRatio = true,
            [FromQuery] bool? allowHoles = false,
            [FromQuery] bool? returnGridCells = false,
            [FromQuery] bool? includeEmptyCells = false,
            [FromQuery] MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] CoordinateSys? coordinateSystem = CoordinateSys.WGS84,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "")
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var userExports = await GetUserExportsAsync();
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
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
                    alphaValues?.Any() ?? false ? Result.Success() : Result.Failure("You must state at least one alpha value"),
                    _inputValidator.ValidateInt(gridCellSizeInMeters!.Value, minLimit: 1000, maxLimit: 100000, "Grid cell size in meters"),
                    ValidateUserExport(userExports),
                    await _inputValidator.ValidateTilesLimitMetricAsync(
                        searchFilter!.Geographics!.BoundingBox!.ToEnvelope().Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM),
                        gridCellSizeInMeters.Value,
                        _analysisManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, filter),
                        true,
                        4.0 // allow 4 times more tiles in file order.
                    ));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                int numberOfTaxa = await _analysisManager.GetNumberOfTaxaInFilterAsync(filter);
                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAooEooAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        filter,
                        gridCellSizeInMeters!.Value,
                        useCenterPoint!.Value,
                        alphaValues,
                        useEdgeLengthRatio!.Value,
                        allowHoles!.Value,
                        returnGridCells!.Value,
                        includeEmptyCells!.Value,
                        metricCoordinateSys!.Value,
                        coordinateSystem!.Value,
                        this.GetUserEmail(),
                        description,
                        ExportFormat.AooEoo,
                        sendMailFromZendTo,
                        encryptedPassword,
                        null, // performContext
                        JobCancellationToken.Null));

                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = numberOfTaxa,
                    Format = ExportFormat.AooEoo,
                    Description = description,
                    OutputFieldSet = null
                };

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);
                return new OkObjectResult(jobId);                
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Order AooAndEoo error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }       

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="validateFilter"></param>
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
            [FromQuery] bool? validateFilter,
            [FromQuery] int? maxDistance = 50000,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] CoordinateSys? coordinateSystem = CoordinateSys.WGS84)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;

                var validationResult = Result.Combine(
                    maxDistance > 0 ? Result.Success() : Result.Failure("You must state max distance"),
                    _inputValidator.ValidateInt(maxDistance.Value, minLimit: 1000, maxLimit: 50000, "Max distance in meters"),
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
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

                (FeatureCollection FeatureCollection, Lib.Models.Analysis.AooEooItem AooEooItem)? result = await _analysisManager.CalculateAooAndEooArticle17Async(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    gridCellSizeInMeters!.Value,
                    maxDistance!.Value,
                    metricCoordinateSys!.Value,
                    coordinateSystem!.Value
                );

                if (result == null)
                    return new OkObjectResult(null!);

                return new OkObjectResult(result!.Value.FeatureCollection);
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

        /// <summary>
        /// Starts the process of creating a zip file with AOO and EOO Article 17 based on the provided filter.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="searchFilter">The search filter.</param>
        /// <param name="validateFilter"></param>
        /// <param name="maxDistance">Max distance between occurrence grid cells</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system</param>
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <returns></returns>
        [HttpPost("/internal/order/aoo_eoo/article17")]
        [Authorize]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> OrderAooAndEooArticle17InternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromQuery] string description,
            [FromBody] SearchFilterInternalDto searchFilter,
            [FromQuery] bool? validateFilter,
            [FromQuery] int? maxDistance = 50000,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] CoordinateSys? coordinateSystem = CoordinateSys.WGS84,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "")
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_analysisConfiguration.ProtectedScope!, searchFilter.ProtectionFilter);
                searchFilter = await _searchFilterUtility.InitializeSearchFilterAsync(searchFilter);
                var filter = searchFilter?.ToSearchFilter(this.GetUserId(), searchFilter?.ProtectionFilter, "sv-SE")!;
                var userExports = await GetUserExportsAsync();
                var validationResult = Result.Combine(
                    maxDistance > 0 ? Result.Success() : Result.Failure("You must state max distance"),
                    _inputValidator.ValidateInt(maxDistance.Value, minLimit: 1000, maxLimit: 50000, "Max distance in meters"),
                    validateFilter ?? false ? (await _inputValidator.ValidateSearchFilterAsync(searchFilter!)) : Result.Success(),
                    _inputValidator.ValidateInt(gridCellSizeInMeters!.Value, minLimit: 1000, maxLimit: 100000, "Grid cell size in meters"),
                    ValidateUserExport(userExports),
                    await _inputValidator.ValidateTilesLimitMetricAsync(
                        searchFilter!.Geographics!.BoundingBox!.ToEnvelope().Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM),
                        gridCellSizeInMeters.Value,
                        _analysisManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, filter),
                        true,
                        4.0 // allow 4 times more tiles in file order.
                    ));
                
                int numberOfTaxa = await _analysisManager.GetNumberOfTaxaInFilterAsync(filter);
                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAooEooArticle17Async(
                        roleId,
                        authorizationApplicationIdentifier,
                        filter,
                        gridCellSizeInMeters!.Value,
                        maxDistance!.Value,
                        metricCoordinateSys!.Value,
                        coordinateSystem!.Value,
                        this.GetUserEmail(),
                        description,
                        ExportFormat.AooEoo,
                        sendMailFromZendTo,
                        encryptedPassword,
                        null, // performContext
                        JobCancellationToken.Null));                

                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = numberOfTaxa,
                    Format = ExportFormat.AooEooArticle17,
                    Description = description,
                    OutputFieldSet = null
                };

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);
                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Order AooAndEoo Article 17 error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get user export info
        /// </summary>
        /// <returns></returns>
        private async Task<UserExport> GetUserExportsAsync()
        {
            var userExport = await _userExportRepository.GetAsync(this.GetUserId());
            return userExport ?? new UserExport { Id = this.GetUserId(), Limit = _defaultUserExportLimit };
        }

        /// <summary>
        /// Update user exports
        /// </summary>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private async Task UpdateUserExportsAsync(UserExport userExport)
        {
            await _userExportRepository.AddOrUpdateAsync(userExport);
        }

        /// <summary>
        /// Validate user export
        /// </summary>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private Result ValidateUserExport(UserExport userExport)
        {
            var onGoingJobCount = userExport?.Jobs?.Where(j => new[] { ExportJobStatus.Queued, ExportJobStatus.Processing }.Contains(j.Status))?.Count() ?? 0;
            if (onGoingJobCount > (userExport?.Limit ?? 1))
            {
                return Result.Failure($"User already has {onGoingJobCount} on going exports.");
            }

            return Result.Success();
        }        
    }
}