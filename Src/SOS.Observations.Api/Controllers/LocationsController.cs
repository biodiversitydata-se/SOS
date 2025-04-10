﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Configuration;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Dtos.Location;
using SOS.Shared.Api.Extensions.Controller;
using SOS.Shared.Api.Extensions.Dto;
using SOS.Shared.Api.Validators.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Result = CSharpFunctionalExtensions.Result;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using System.Diagnostics;
using SOS.Lib.Enums;
using System.Threading;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationManager _locationManager;
        private readonly IInputValidator _inputValidator;
        private readonly ObservationApiConfiguration _observationApiConfiguration;
        private readonly SemaphoreLimitManager _semaphoreLimitManager;
        private readonly ILogger<LocationsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="locationManager"></param>
        /// <param name="inputValidator"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="semaphoreLimitManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LocationsController(
            ILocationManager locationManager,
            IInputValidator inputValidator,
            ObservationApiConfiguration observationApiConfiguration,
            SemaphoreLimitManager semaphoreLimitManager,
            ILogger<LocationsController> logger)
        {
            _locationManager = locationManager ??
                                  throw new ArgumentNullException(nameof(locationManager));
            _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
            _observationApiConfiguration = observationApiConfiguration ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
            _semaphoreLimitManager = semaphoreLimitManager ?? throw new ArgumentNullException(nameof(semaphoreLimitManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(List<LocationDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetLocationsByIds([FromBody] IEnumerable<string> locationIds)
        {
            var semaphore = await GetSemaphoreAsync(SemaphoreType.Aggregation);
            if (semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                if (!locationIds?.Any() ?? true)
                {
                    return BadRequest("You have to provide at least one location id");
                }

                if (locationIds.Count() > 10000)
                {
                    return BadRequest("You can't provide more than 10000 location id's at a time");
                }

                var locations = await _locationManager.GetLocationsAsync(locationIds);

                return new OkObjectResult(locations);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get locations");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Search for locations
        /// </summary>
        /// <param name="filter">The search filter.</param>
        /// <param name="skip">Pagination start index.</param>
        /// <param name="take">Number of items to return.</param>
        /// <param name="sensitiveObservations">If true, only sensitive (protected) observations will be searched (this requires authentication and authorization). If false, public available observations will be searched.</param>
        /// <param name="roleId">Limit user authorization too specified role.</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <returns></returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<LocationSearchResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> SearchAsync([FromBody] GeographicsFilterDto filter, [FromQuery] int skip = 0, [FromQuery] int take = 100, [FromQuery] bool sensitiveObservations = false, [FromQuery] int? roleId = null,
            [FromQuery] string authorizationApplicationIdentifier = null)
        {
            var semaphore = await GetSemaphoreAsync(SemaphoreType.Aggregation);
            if (semaphore == null) return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);

            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                this.User.CheckAuthorization(_observationApiConfiguration.ProtectedScope, protectionFilter);

                var searchFilter = new SearchFilterBaseDto { Geographics = filter };
                var validationResult = Result.Combine(
                    (await _inputValidator.ValidateSearchFilterAsync(searchFilter)),
                    _inputValidator.ValidateBoundingBox(filter?.BoundingBox, false),
                    _inputValidator.ValidateSearchPagingArguments(skip, take)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var locations = await _locationManager.SearchAsync(roleId, authorizationApplicationIdentifier, searchFilter.ToSearchFilter(this.GetUserId(), protectionFilter, "sv-SE"), skip, take);

                return new OkObjectResult(locations);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get locations");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<SemaphoreSlim?> GetSemaphoreAsync(SemaphoreType semaphoreType)
        {
            var userType = this.GetApiUserType();
            SemaphoreSlim semaphore;
            if (semaphoreType == SemaphoreType.Aggregation)
            {
                semaphore = _semaphoreLimitManager.GetAggregationSemaphore(userType);
            }
            else
            {
                semaphore = _semaphoreLimitManager.GetObservationSemaphore(userType);
            }

            var semaphoreTime = Stopwatch.StartNew();
            if (semaphore.CurrentCount == 0)
            {
                _logger.LogWarning("All semaphore slots are occupied. Request will be queued. Endpoint={endpoint}, UserType={@userType}", this.GetEndpointName(ControllerContext), userType);
                HttpContext.Items["SemaphoreLimitUsed"] = "Wait";
            }

            if (!await semaphore.WaitAsync(_semaphoreLimitManager.DefaultTimeout))
            {
                _logger.LogError("Too many requests. Semaphore limit reached. Endpoint={endpoint}, UserType={@userType}", this.GetEndpointName(ControllerContext), userType);
                HttpContext.Items["SemaphoreLimitUsed"] = "Timeout";
                return null;
            }

            semaphoreTime.Stop();
            if (semaphoreTime.ElapsedMilliseconds > 1000)
            {
                HttpContext.Items["SemaphoreWaitSeconds"] = (int)Math.Round(semaphoreTime.ElapsedMilliseconds / 1000.0);
            }

            return semaphore;
        }
    }
}