using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Location;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Observations.Api.Controllers
{ 
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : SearchBaseController, ILocationsController
    {
        private readonly ILocationManager _locationManager;
        private readonly ILogger<LocationsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="locationManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LocationsController(
            ILocationManager locationManager,
            IAreaManager areaManager,
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<LocationsController> logger) : base(areaManager, observationApiConfiguration)
        {
            _locationManager = locationManager ??
                                  throw new ArgumentNullException(nameof(locationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost]
        [ProducesResponseType(typeof(LocationDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationsByIds([FromBody]IEnumerable<string> locationIds)
        {
            try
            {
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
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<LocationSearchResultDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchAsync([FromBody] GeographicsFilterDto filter, [FromQuery] int skip = 0, [FromQuery] int take = 100, [FromQuery] bool sensitiveObservations = false, [FromQuery] int? roleId = null,
            [FromQuery] string authorizationApplicationIdentifier = null)
        {
            try
            {
                CheckAuthorization(sensitiveObservations);

                var searchFilter = new SearchFilterBaseDto { Geographics = filter };
                var validationResult = Result.Combine(
                    ValidateSearchFilter(searchFilter),
                    ValidateSearchPagingArguments(skip, take)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var locations = await _locationManager.SearchAsync(roleId, authorizationApplicationIdentifier, searchFilter.ToSearchFilter(UserId, sensitiveObservations, "sv-SE"), skip, take);

                return new OkObjectResult(locations);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get locations");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}