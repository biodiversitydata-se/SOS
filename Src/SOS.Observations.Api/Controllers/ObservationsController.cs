using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Helpers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Dtos.Observation;
using SOS.Shared.Api.Extensions.Controller;
using SOS.Shared.Api.Extensions.Dto;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Validators.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase
    {
        private readonly IObservationManager _observationManager;
        private readonly ITaxonSearchManager _taxonSearchManager;
        private readonly ISearchFilterUtility _searchFilterUtility;
        private readonly IInputValidator _inputValidator;
        private readonly ObservationApiConfiguration _observationApiConfiguration;
        private readonly IClassCache<ConcurrentDictionary<string, CacheEntry<GeoGridResultDto>>> _geogridAggregationCache;
        private readonly IClassCache<ConcurrentDictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>> _taxonAggregationInternalCache;
        private readonly SemaphoreLimitManager _semaphoreLimitManager;
        private readonly ILogger<ObservationsController> _logger;

        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="taxonSearchManager"></param>
        /// <param name="searchFilterUtility"></param>
        /// <param name="inputValidator"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="geogridAggregationCache"></param>
        /// <param name="taxonAggregationInternalCache"></param>
        /// <param name="semaphoreLimitManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationsController(
            IObservationManager observationManager,
            ITaxonSearchManager taxonSearchManager,
            ISearchFilterUtility searchFilterUtility,
            IInputValidator inputValidator,
            ObservationApiConfiguration observationApiConfiguration,
            IClassCache<ConcurrentDictionary<string, CacheEntry<GeoGridResultDto>>> geogridAggregationCache,
            IClassCache<ConcurrentDictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>> taxonAggregationInternalCache,
            SemaphoreLimitManager semaphoreLimitManager,
            ILogger<ObservationsController> logger) 
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
             _taxonSearchManager = taxonSearchManager ?? throw new ArgumentNullException(nameof(taxonSearchManager));
            _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
            _searchFilterUtility = searchFilterUtility ?? throw new ArgumentNullException(nameof(searchFilterUtility));
            _observationApiConfiguration = observationApiConfiguration ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
            _geogridAggregationCache = geogridAggregationCache ?? throw new ArgumentNullException(nameof(geogridAggregationCache));
            _taxonAggregationInternalCache = taxonAggregationInternalCache ?? throw new ArgumentNullException(nameof(taxonAggregationInternalCache));
            _semaphoreLimitManager = semaphoreLimitManager ?? throw new ArgumentNullException(nameof(semaphoreLimitManager));
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
        /// <param name="resolveGeneralizedObservations">If true, then try get real coordinates for generalized observations.</param>
        /// <returns></returns>
        [HttpGet("{id?}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetObservationById(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromRoute] string id,
            [FromQuery] string occurrenceId,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool resolveGeneralizedObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                
                occurrenceId = WebUtility.UrlDecode(id ?? occurrenceId);

                var observation = await _observationManager.GetObservationAsync(this.GetUserId(), roleId, authorizationApplicationIdentifier, occurrenceId, outputFieldSet, translationCultureCode, sensitiveObservations,
                    includeInternalFields: false, resolveGeneralizedObservations);

                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                this.LogObservationCount(1);
                return new OkObjectResult(observation);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting observation {@occurrenceId}", occurrenceId);
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> Count(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterBaseDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);
                
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries));
                
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, "sv-SE");
                var matchCount = await _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter);
                
                this.LogObservationCount(matchCount);
                return new OkObjectResult(matchCount);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
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
        /// <param name="skipCache">If true, skip using cached result.</param>
        /// <returns></returns>
        [HttpPost("GeoGridAggregation")]
        [ProducesResponseType(typeof(GeoGridResultDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GeogridAggregation(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool? skipCache = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult?.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // Cache
                string cacheKey = null;
                ConcurrentDictionary<string, CacheEntry<GeoGridResultDto>> geogridAggregationByCacheKey = null;
                bool useCache = !sensitiveObservations && Request.ContentLength < 10000;
                if (useCache)
                {
                    var parameters = new[]
                    {
                        $"zoom={zoom}",
                        $"sensitiveObservations={sensitiveObservations}"
                    };
                    cacheKey = CreateCacheKey(string.Join("&", parameters), filter);
                    HttpContext.Items.TryAdd("CacheKey", cacheKey);
                    geogridAggregationByCacheKey = _geogridAggregationCache.Get();
                    if (geogridAggregationByCacheKey == null)
                    {
                        geogridAggregationByCacheKey = new ConcurrentDictionary<string, CacheEntry<GeoGridResultDto>>();
                        _geogridAggregationCache.Set(geogridAggregationByCacheKey);
                    }
                    if (!skipCache.GetValueOrDefault(false) && cacheKey != null && geogridAggregationByCacheKey.TryGetValue(cacheKey, out CacheEntry<GeoGridResultDto> cacheEntry))
                    {
                        var val = _geogridAggregationCache.GetCacheEntryValue(cacheEntry);
                        _logger.LogDebug($"GeoGridAggregation result found in cache and is returned as result. Number of requests={cacheEntry.Count}");
                        return new OkObjectResult(val);
                    }
                }

                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    await _inputValidator.ValidateTilesLimitAsync(boundingBox, zoom, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter))
                );
                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _observationManager.GetGeogridTileAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier, searchFilter, zoom);

                var dto = result.ToGeoGridResultDto(boundingBox.CalculateNumberOfTiles(zoom));

                // Cache
                if (useCache)
                {
                    if (cacheKey != null && !geogridAggregationByCacheKey.ContainsKey(cacheKey))
                    {
                        _geogridAggregationCache.CheckCacheSize(geogridAggregationByCacheKey);
                        geogridAggregationByCacheKey.TryAdd(cacheKey, _geogridAggregationCache.CreateCacheEntry(dto));
                    }
                }

                return new OkObjectResult(dto);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
            }
        }

        private string CreateCacheKey(string queryString, object request)
        {
            try
            {
                StringBuilder keyBuilder = new StringBuilder();
                keyBuilder.Append(queryString);

                if (request != null)
                {
                    string requestBody = JsonSerializer.Serialize(request, _jsonSerializerOptions);
                    keyBuilder.Append(requestBody);
                }

                string key = keyBuilder.ToString();
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(keyBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating cache key");
                return null;
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> MetricGridAggregationAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationDto filter,
            [FromQuery] int gridCellSizeInMeters = 100000,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult?.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, "en-GB");

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateGridCellSizeInMetersArgument(gridCellSizeInMeters, minLimit: 100, maxLimit: 100000),
                    await _inputValidator.ValidateTilesLimitMetricAsync(boundingBox.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter))
                    );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _observationManager.GetMetricGridAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier, searchFilter, gridCellSizeInMeters, MetricCoordinateSys.SWEREF99_TM);

                var dto = result.ToDto();
                return new OkObjectResult(dto);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Metric grid aggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
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
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Observation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    _inputValidator.ValidateSearchPagingArguments(skip, take),                    
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode));
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);
                var sortFieldValidationResult = string.IsNullOrEmpty(sortBy) ? Result.Success<List<string>>(null) : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy }));
                if (sortFieldValidationResult.IsFailure) return BadRequest(sortFieldValidationResult.Error);
                if (sortFieldValidationResult.Value != null && sortFieldValidationResult.Value.Any())
                {
                    sortBy = sortFieldValidationResult.Value.First();
                }
                SearchFilter searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, translationCultureCode, sortBy, sortOrder);
                var result = await _observationManager.GetChunkAsync(roleId, authorizationApplicationIdentifier, searchFilter, skip, take);
                var dto = result?.ToPagedResultDto(result.Records);
                this.LogObservationCount(dto?.Records?.Count() ?? 0);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Search error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
            }
        }

        /// <summary>
        /// Search observations and return in Darwin Core format.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Application identifier making the request, used to get proper authorization</param>
        /// <param name="kingdom">Taxon kingdom. Plantae, arachnida, mollusca, insecta, amphibia, aves, mammalia, reptilia, actinopterygii, animalia, fungi</param>
        /// <param name="identificationVerificationStatus">Identification verification status. Research, casual</param>
        /// <param name="license">none,CC-BY,CC-BY-NC,CC-BY-SA</param>
        /// <param name="scientificName"></param>        
        /// <param name="taxonKey"></param>
        /// <param name="issue"></param>
        /// <param name="has">Geo,photos</param>
        /// <param name="minEventDate"></param>
        /// <param name="maxEventDate"></param>
        /// <param name="dataProviderIds">By default only Artportalen observations are returned. If you want other data providers specify them as a comma separated list. E.g. "1,3,8,12"</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB).</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <param name="skip">Start index of returned observations.</param>
        /// <param name="take">Max number of observations to return. Max is 1000 observations in each request.</param>
        /// <param name="sortBy">Field to sort by.</param>
        /// <param name="sortOrder">Sort order (Asc, Desc).</param>
        /// <returns></returns>
        [HttpGet("Search/DwC")]
        [ProducesResponseType(typeof(IEnumerable<DarwinCoreOccurrenceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> ObservationsBySearchDwc(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromQuery] string kingdom,
            [FromQuery] string identificationVerificationStatus,
            [FromQuery] string license,
            [FromQuery] string scientificName,
            [FromQuery] int? taxonKey,
            [FromQuery] string issue,
            [FromQuery] string has,
            [FromQuery] DateTime? minEventDate,
            [FromQuery] DateTime? maxEventDate,
            [FromQuery] string dataProviderIds = null,
            [FromQuery] string translationCultureCode = "en-GB",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = null!,
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Observation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                var sortFieldValidationResult = string.IsNullOrEmpty(sortBy) ? Result.Success<List<string>>(null) : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy }));
                if (sortFieldValidationResult.IsFailure) return BadRequest(sortFieldValidationResult.Error);
                if (sortFieldValidationResult.Value != null && sortFieldValidationResult.Value.Any())
                {
                    sortBy = sortFieldValidationResult.Value.First();
                }                

                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var searchFilter = new SearchFilterInternal(this.GetUserId(), sensitiveObservations ? ProtectionFilter.Sensitive : ProtectionFilter.Public)
                {
                    FieldTranslationCultureCode = translationCultureCode,
                    Output = string.IsNullOrEmpty(sortBy) ? new OutputFilter() : new OutputFilter
                    {
                        SortOrders = new[] {
                            new SortOrderFilter {
                                SortBy = DwAMappingHelper.MapToObservationField(sortBy), SortOrder = sortOrder
                            }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(kingdom))
                {
                    searchFilter.Taxa = new TaxonFilter
                    {
                        Kingdoms = kingdom.Split(",", StringSplitOptions.TrimEntries).Select(s => s.ToUpperFirst())
                    };
                }

                if (!string.IsNullOrEmpty(scientificName))
                {
                    (searchFilter.Taxa ??= new TaxonFilter()).ScientificNames = scientificName.Split(",", StringSplitOptions.TrimEntries).Select(s => s.ToUpperFirst());
                }

                if (!string.IsNullOrEmpty(identificationVerificationStatus))
                {
                    var identificationVerificationStatuses = identificationVerificationStatus.ToLower().Split(",", StringSplitOptions.TrimEntries).ToArray();

                    if (identificationVerificationStatuses.Count(ivs => ivs.Equals("research") || ivs.Equals("casual")) == 2)
                    {
                        searchFilter.VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified;
                    }
                    else if (identificationVerificationStatuses.Any(ivs => ivs.Equals("research")))
                    {
                        searchFilter.VerificationStatus = SearchFilterBase.StatusVerification.Verified;
                    }
                    else if (identificationVerificationStatuses.Any(ivs => ivs.Equals("casual")))
                    {
                        searchFilter.VerificationStatus = SearchFilterBase.StatusVerification.NotVerified;
                    }
                }
                searchFilter.OnlyWithMedia = has?.Contains("photos", StringComparison.CurrentCultureIgnoreCase) ?? false;
                searchFilter.Licenses = license?.ToLower().Split(",");

                if (minEventDate.HasValue || maxEventDate.HasValue)
                {
                    searchFilter.Date = new DateFilter
                    {
                        StartDate = minEventDate,
                        EndDate = maxEventDate
                    };
                }

                if (!string.IsNullOrWhiteSpace(dataProviderIds))
                {
                    searchFilter.DatasourceIds = dataProviderIds.Split(",", StringSplitOptions.TrimEntries)
                        .Select(int.Parse)
                        .ToList();
                }
                else
                {
                    searchFilter.DatasourceIds = new List<int> { 1 }; // Artportalen.
                }

                var result = await _observationManager.GetChunkAsync(roleId, authorizationApplicationIdentifier, searchFilter, skip, take);
                var dtos = result?.Records?.ToObservations().Select(o => o.ToDto());

                this.LogObservationCount(dtos?.Count() ?? 0);
                return new OkObjectResult(dtos);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Search DwC error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
            }
        }

        /// <summary>
        /// Gets a single observation.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="id">The occurrence id of the observation to fetch.</param>
        /// <param name="translationCultureCode">Culture code used for vocabulary translation (sv-SE, en-GB)</param>
        /// <param name="sensitiveObservations">
        /// If true, and the requested observation is sensitive (protected), then the original data will be returned (this requires authentication and authorization).
        /// If false, and the requested observation is sensitive (protected), then diffused data will be returned.
        /// </param>
        /// <returns></returns>
        [HttpGet("DwC/{id}")]
        [ProducesResponseType(typeof(IEnumerable<DarwinCoreOccurrenceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> ObservationByIdDwc(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromRoute] string id,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                id = WebUtility.UrlDecode(id);

                var observation = await _observationManager.GetObservationAsync(this.GetUserId(), roleId, authorizationApplicationIdentifier, id, OutputFieldSet.All, translationCultureCode, sensitiveObservations,
                    includeInternalFields: false, false);

                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                var dto = new[] { observation }.ToObservations().Select(o => o.ToDto()).FirstOrDefault();

                this.LogObservationCount(dto == null ? 0 : 1);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting observation {occurrenceId}", id);
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
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
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var parameters = new[]
                {
                    $"skip={skip}",
                    $"take={take}",
                    $"sensitiveObservations={sensitiveObservations}"
                };
                string cacheKey = CreateCacheKey(string.Join("&", parameters), filter);
                HttpContext.Items.TryAdd("CacheKey", cacheKey);

                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateTaxonAggregationPagingArguments(skip, take)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _taxonSearchManager.GetTaxonAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    searchFilter,
                    skip,
                    take,
                    false);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                PagedResultDto<TaxonAggregationItemDto> dto = result.Value.ToPagedResultDto(result.Value.Records.ToTaxonAggregationItemDtos());
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        /// <param name="resolveGeneralizedObservations">If true, then try get real coordinates for generalized observations.</param>
        /// <returns></returns>
        [HttpGet("Internal/{id?}")]
        [ProducesResponseType(typeof(Observation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> GetObservationByIdInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromRoute] string id,
            [FromQuery] string occurrenceId,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.Minimum,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool resolveGeneralizedObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                
                occurrenceId = WebUtility.UrlDecode(occurrenceId ?? id);
                var observation = await _observationManager.GetObservationAsync(
                    this.GetUserId(),
                    roleId,
                    authorizationApplicationIdentifier, occurrenceId, outputFieldSet, translationCultureCode, sensitiveObservations,
                    includeInternalFields: true, resolveGeneralizedObservations);
                if (observation == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                this.LogObservationCount(1);
                return new OkObjectResult(observation);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting observation {@occurrenceId}", occurrenceId);
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> CountInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalBaseDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE");
                var matchCount = await _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter);
                this.LogObservationCount(matchCount);
                return new OkObjectResult(matchCount);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
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
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> CachedCountInternal(
            [FromQuery] int taxonId)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var result = await _taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(new int[] { taxonId });
                if (!result.Any())
                    return NoContent();
                this.LogObservationCount(result.First()?.ObservationCount ?? 0);
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
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> MultipleCachedCountInternal(
            [FromBody] IEnumerable<int> taxonIds)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var result = await _taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(taxonIds);
                if (!result.Any())
                    return NoContent();

                this.LogObservationCount(result?.Count() ?? 0);
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> GeogridAggregationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var parameters = new[]
                {
                    $"zoom={zoom}",
                    $"sensitiveObservations={sensitiveObservations}"
                };
                string cacheKey = CreateCacheKey(string.Join("&", parameters), filter);
                HttpContext.Items.TryAdd("CacheKey", cacheKey);

                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    await _inputValidator.ValidateTilesLimitAsync(boundingBox, zoom, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter), true)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var result = await _observationManager.GetGeogridTileAggregationAsync(roleId, authorizationApplicationIdentifier, searchFilter, zoom);
              
                GeoGridResultDto dto = result.ToGeoGridResultDto(boundingBox.CalculateNumberOfTiles(zoom));
                return new OkObjectResult(dto);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> GeogridAggregationAsGeoJsonInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int zoom = 1,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilter(this.GetUserId(), filter.ProtectionFilter, translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    await _inputValidator.ValidateTilesLimitAsync(boundingBox, zoom, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter), true)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

               
                var result = await _observationManager.GetGeogridTileAggregationAsync(roleId, authorizationApplicationIdentifier, searchFilter, zoom);
             
                string strJson = result.GetFeatureCollectionGeoJson();
                var bytes = Encoding.UTF8.GetBytes(strJson);
                return File(bytes, "application/json", "grid.geojson");
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregationGeoJson error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
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
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateGeogridZoomArgument(zoom, minLimit: 1, maxLimit: 21),
                    await _inputValidator.ValidateTilesLimitAsync(boundingBox, zoom, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter), true)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }


                var result = await _taxonSearchManager.GetPageGeoTileTaxaAggregationAsync(roleId, authorizationApplicationIdentifier, searchFilter, zoom, geoTilePage, taxonIdPage);
                if (result.IsFailure)
                {
                    return BadRequest(result.Error);
                }

                GeoGridTileTaxonPageResultDto dto = result.Value.ToGeoGridTileTaxonPageResultDto();
                return new OkObjectResult(dto);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GeoGridAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        /// <param name="metricCoordinateSys">Metric coordinate system used to calculate grid cells</param>
        /// <param name="outputFormat">Returned format</param>
        /// <returns></returns>
        [HttpPost("Internal/MetricGridAggregation")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> MetricGridAggregationInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int gridCellSizeInMeters = 10000,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] MetricCoordinateSys metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            [FromQuery] OutputFormatDto outputFormat = OutputFormatDto.Json)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilter(this.GetUserId(), filter.ProtectionFilter, "en-GB");

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateGridCellSizeInMetersArgument(gridCellSizeInMeters, minLimit: 100, maxLimit: 100000),
                    await _inputValidator.ValidateTilesLimitMetricAsync(boundingBox.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters, _observationManager.GetMatchCountAsync(roleId, authorizationApplicationIdentifier, searchFilter), true)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                
                var result = await _observationManager.GetMetricGridAggregationAsync(
                    roleId,
                    authorizationApplicationIdentifier,
                    searchFilter,
                    gridCellSizeInMeters,
                    metricCoordinateSys);

                var dto = result.ToDto();
                return new OkObjectResult(outputFormat == OutputFormatDto.Json ? dto : dto.ToGeoJson(metricCoordinateSys));
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Metric grid aggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
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
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Observation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(                    
                    string.IsNullOrEmpty(sortBy) ? Result.Success() : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy })),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateSearchPagingArgumentsInternal(skip, take),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);
                var sortFieldValidationResult = string.IsNullOrEmpty(sortBy) ? Result.Success<List<string>>(null) : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy }));
                if (sortFieldValidationResult.IsFailure) return BadRequest(sortFieldValidationResult.Error);
                if (sortFieldValidationResult.Value != null && sortFieldValidationResult.Value.Any())
                {
                    sortBy = sortFieldValidationResult.Value.First();
                }
                if (outputFormat == OutputFormatDto.GeoJson || outputFormat == OutputFormatDto.GeoJsonFlat)
                {
                    var outPutFields = EnsureCoordinatesIsRetrievedFromDb(filter?.Output?.Fields);

                    if (outPutFields?.Any() ?? false)
                    {
                        filter.Output ??= new OutputFilterExtendedDto();
                        filter.Output.Fields = EnsureCoordinatesIsRetrievedFromDb(filter?.Output?.Fields);
                    }
                }
                var result = await _observationManager.GetChunkAsync(roleId, authorizationApplicationIdentifier, filter.ToSearchFilterInternal(this.GetUserId(), translationCultureCode, sortBy, sortOrder), skip, take);
                if (result == null)
                {
                    throw new Exception("Something went wrong when your query was executed. Make sure your filter is correct.");
                }
                GeoPagedResultDto<JsonObject> dto = result.ToGeoPagedResultDto(result.Records, outputFormat);
                this.LogObservationCount(dto?.Records?.Count() ?? 0);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SearchInternal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
            }
        }

        private void LogUserInformation()
        {
            try
            {
                _logger.LogInformation($"User.UserId={this.GetUserId()}");
                _logger.LogInformation($"User.Email={this.GetUserEmail()?.ToString() ?? "[null]"}");
                if (User?.Claims != null)
                {
                    foreach (var claim in User.Claims)
                    {
                        _logger.LogInformation($"User.Claim.{claim.Type}={claim.Value.ToString() ?? "[null]"}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "LogUserInformation error");
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
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
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateAggregationPagingArguments(skip, take, true),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode));

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

                var result = await _observationManager.GetAggregatedChunkAsync(roleId, authorizationApplicationIdentifier, filter.ToSearchFilterInternal(this.GetUserId(), translationCultureCode), aggregationType, skip, take);
                PagedResultDto<dynamic> dto = result.ToPagedResultDto(result?.Records);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SearchAggregatedInternal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
            }
        }

        /// <summary>
        /// Aggregate observations into a time series.
        /// </summary>
        /// <param name="roleId">Limit user authorization to specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Filter used to limit the search.</param>
        /// <param name="timeSeriesType">The aggregation type</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <returns></returns>
        [HttpPost("Internal/TimeSeriesHistogram")]
        [ProducesResponseType(typeof(IEnumerable<TimeSeriesHistogramResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> TimeSeriesHistogramInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] TimeSeriesTypeDto timeSeriesType,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }
                                
                var result = await _observationManager.GetTimeSeriesHistogramAsync(
                    roleId, 
                    authorizationApplicationIdentifier, 
                    filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE"), 
                    (TimeSeriesType) timeSeriesType);
                IEnumerable<TimeSeriesHistogramResultDto> dtos = result.ToTimeSeriesHistogramResultDtos();
                return new OkObjectResult(dtos);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TimeSeriesHistogram error");
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, protectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);
                const int maxTotalCount = 100000;
                var validationResult = Result.Combine(                    
                    string.IsNullOrEmpty(sortBy) ? Result.Success() : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy })),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    take <= 10000 ? Result.Success() : Result.Failure("You can't take more than 10 000 at a time."),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode));

                if (validationResult.IsFailure) return BadRequest(validationResult.Error);
                var sortFieldValidationResult = string.IsNullOrEmpty(sortBy) ? Result.Success<List<string>>(null) : (await _inputValidator.ValidateSortFieldsAsync(new[] { sortBy }));
                if (sortFieldValidationResult.IsFailure) return BadRequest(sortFieldValidationResult.Error);
                if (sortFieldValidationResult.Value != null && sortFieldValidationResult.Value.Any())
                {
                    sortBy = sortFieldValidationResult.Value.First();
                }
                SearchFilter searchFilter = filter.ToSearchFilter(this.GetUserId(), protectionFilter, translationCultureCode, sortBy, sortOrder);
                var result = await _observationManager.GetObservationsByScrollAsync(roleId, authorizationApplicationIdentifier, searchFilter, take, scrollId);
                if (result.TotalCount > maxTotalCount)
                {
                    return BadRequest($"Scroll total count limit is maxTotalCount. Your result is {result.TotalCount}. Try use a more specific filter.");
                }

                ScrollResultDto<JsonObject> dto = result.ToScrollResultDto(result.Records);
                this.LogObservationCount(dto?.Records?.Count() ?? 0);
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Search error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Signal search
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="areaBuffer">Are buffer 0 to 100m.</param>
        /// <param name="onlyAboveMyClearance">If true, get signal only above users clearance.</param>
        /// <param name="returnHttp4xxWhenNoPermissions">
        /// If true, an HTTP 403 response will be returned if the user attempts to search in areas where they lack permission.
        /// An HTTP 409 response will be returned if the user has partial permission to search in an area and the signal search returns false.
        /// </param>
        /// <returns></returns>
        [HttpPost("Internal/SignalSearch")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> SignalSearchInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SignalFilterDto filter,
            [FromQuery] bool validateSearchFilter = false, // if false, only mandatory requirements will be validated
            [FromQuery] int areaBuffer = 0,
            [FromQuery] bool onlyAboveMyClearance = true,
            [FromQuery] bool? returnHttp4xxWhenNoPermissions = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, ProtectionFilterDto.Sensitive);

                var validationResult = Result.Combine(
                    (await _inputValidator.ValidateSignalSearchAsync(filter, validateSearchFilter, areaBuffer)),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), true);
                var taxonFound = await _observationManager.SignalSearchInternalAsync(roleId, authorizationApplicationIdentifier, searchFilter, areaBuffer, onlyAboveMyClearance, returnHttp4xxWhenNoPermissions ?? false);

                if (taxonFound == SignalSearchResult.NoPermissions || taxonFound == SignalSearchResult.PartialNoPermissions)
                {
                    _logger.LogInformation("User don't have the SightingIndication permission in provided areas");
                    _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                    _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                    LogUserInformation();

                    if (taxonFound == SignalSearchResult.NoPermissions)
                        return new StatusCodeResult((int)HttpStatusCode.Forbidden);
                    else if (taxonFound == SignalSearchResult.PartialNoPermissions)
                        return new StatusCodeResult((int)HttpStatusCode.Conflict);
                }

                return new OkObjectResult(taxonFound.Equals(SignalSearchResult.Yes));
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Signal search Internal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        /// <param name="skipCache">If true, skip using cached result.</param>   
        /// <returns></returns>
        [HttpPost("Internal/TaxonAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<TaxonAggregationItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> TaxonAggregationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,            
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] string translationCultureCode = "sv-SE",
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sumUnderlyingTaxa = false,
            [FromQuery] bool? skipCache = false)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // Cache
                string cacheKey = null;
                ConcurrentDictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>> resultByCacheKey = null;
                bool useCache = !sensitiveObservations && Request.ContentLength < 10000;
                if (useCache)
                {
                    var parameters = new[]
                    {
                        $"skip={skip}",
                        $"take={take}",
                        $"sensitiveObservations={sensitiveObservations}",
                        $"sumUnderlyingTaxa={sumUnderlyingTaxa}"
                    };
                    cacheKey = CreateCacheKey(string.Join("&", parameters), filter);
                    HttpContext.Items.TryAdd("CacheKey", cacheKey);
                    resultByCacheKey = _taxonAggregationInternalCache.Get();
                    if (resultByCacheKey == null)
                    {
                        resultByCacheKey = new ConcurrentDictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>();
                        _taxonAggregationInternalCache.Set(resultByCacheKey);
                    }
                    if (!skipCache.GetValueOrDefault(false) && cacheKey != null && resultByCacheKey.TryGetValue(cacheKey, out var cacheEntry))
                    {
                        var val = _taxonAggregationInternalCache.GetCacheEntryValue(cacheEntry);
                        _logger.LogDebug($"TaxonAggregationInternal result found in cache and is returned as result. Number of requests={cacheEntry.Count}");
                        return new OkObjectResult(val);
                    }
                }

                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);

                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var boundingBox = filter.Geographics.BoundingBox.ToEnvelope();
                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), translationCultureCode);
                translationCultureCode = CultureCodeHelper.GetCultureCode(translationCultureCode);

                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTranslationCultureCode(translationCultureCode),
                    _inputValidator.ValidateTaxonAggregationPagingArguments(skip, take)                    
                    );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }


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

                // Cache
                if (useCache)
                {
                    if (cacheKey != null && !resultByCacheKey.ContainsKey(cacheKey))
                    {
                        _taxonAggregationInternalCache.CheckCacheSize(resultByCacheKey);
                        resultByCacheKey.TryAdd(cacheKey, _taxonAggregationInternalCache.CreateCacheEntry(dto));
                    }
                }

                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> TaxonSumAggregationInternal(
            [FromBody] TaxonFilterDto taxonFilter,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] string sortBy = "SumObservationCount",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Desc)
        {
            ApiUserType userType = this.GetApiUserType();
            var semaphoreResult = await _semaphoreLimitManager.GetSemaphoreAsync(SemaphoreType.Aggregation, userType, this.GetEndpointName(ControllerContext));
            LogHelper.AddSemaphoreHttpContextItems(semaphoreResult, HttpContext);
            if (semaphoreResult.Semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                var validationResult = sortBy switch
                {
                    "SumObservationCount" or "ObservationCount" or "SumProvinceCount" or "ProvinceCount" => Result.Success(),
                    _ => Result.Failure($"{sortBy} is not a allowed sort field")
                };
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var result = await _taxonSearchManager.GetTaxonSumAggregationAsync(
                    this.GetUserId(),
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
                _logger.LogInformation(e, e.Message);
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "TaxonSumAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphoreResult.Semaphore.Release();
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
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi, AzureInternalApi]
        public async Task<IActionResult> TaxonExistsIndicationInternal(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                // sensitiveObservations is preserved for backward compability
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope!, filter.ProtectionFilter);
                filter = await _searchFilterUtility.InitializeSearchFilterAsync(filter);
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries),
                    _inputValidator.ValidateTaxonExists(filter),
                    _inputValidator.ValidateGeographicalAreaExists(filter?.Geographics));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE");
                var taxonFound = await _taxonSearchManager.GetTaxonExistsIndicationAsync(roleId, authorizationApplicationIdentifier, searchFilter);

                return new OkObjectResult(taxonFound);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
                _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get indication if taxon exists Internal error");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count current users observations group by year
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <returns></returns>
        [HttpPost("Internal/CurrentUser/YearCount")]
        [ProducesResponseType(typeof(IEnumerable<YearCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserYearCountAsync(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                filter.ProtectionFilter = ProtectionFilterDto.Sensitive;
                var validationResult = Result.Combine(
                    validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                    _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                    _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE");
                var yearCounts = await _observationManager.GetUserYearCountAsync(searchFilter);

                return new OkObjectResult(yearCounts);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count current users observations group by year and month
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <returns></returns>
        [HttpPost("Internal/CurrentUser/YearMonthCount")]
        [ProducesResponseType(typeof(IEnumerable<YearMonthCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserYearMonthCountAsync(
            [FromBody] SearchFilterAggregationInternalDto filter,
            [FromQuery] bool validateSearchFilter = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                filter.ProtectionFilter = ProtectionFilterDto.Sensitive;
                var validationResult = Result.Combine(
                   validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                   _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                   _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE");
                var yearMonthCounts = await _observationManager.GetUserYearMonthCountAsync(searchFilter);

                return new OkObjectResult(yearMonthCounts);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year month count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Count current users observations group by year, month and day
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="validateSearchFilter"></param>
        [HttpPost("Internal/CurrentUser/YearMonthDayCount")]
        [ProducesResponseType(typeof(IEnumerable<YearMonthDayCountResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                filter.ProtectionFilter = ProtectionFilterDto.Sensitive;
                var validationResult = Result.Combine(
                  validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                  _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                  _inputValidator.ValidateGeometries(filter?.Geographics?.Geometries)
               );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var searchFilter = filter.ToSearchFilterInternal(this.GetUserId(), "sv-SE");
                var yearMonthDayCounts = await _observationManager.GetUserYearMonthDayCountAsync(searchFilter, skip, take);

                return new OkObjectResult(yearMonthDayCounts);
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                LogUserInformation();
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year month count");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }        
    }
}