using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
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
    public class LocationsController : SearchBaseController
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

        /// <summary>
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
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
        public async Task<IActionResult> SearchAsync([FromBody] GeographicsFilterDto filter, [FromQuery] int skip = 0, [FromQuery] int take = 100, [FromQuery] bool sensitiveObservations = false, [FromQuery] int? roleId = null,
            [FromQuery] string authorizationApplicationIdentifier = null)
        {
            try
            {
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                var searchFilter = new SearchFilterBaseDto { Geographics = filter };
                var validationResult = Result.Combine(
                    ValidateSearchFilter(searchFilter),
                    ValidateBoundingBox(filter?.BoundingBox, false),
                    ValidateSearchPagingArguments(skip, take)
                );

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var locations = await _locationManager.SearchAsync(roleId, authorizationApplicationIdentifier, searchFilter.ToSearchFilter(UserId, protectionFilter, "sv-SE"), skip, take);

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