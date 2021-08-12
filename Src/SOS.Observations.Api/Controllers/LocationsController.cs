using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Locations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase, ILocationsController
    {
        private readonly IObservationManager _observationManager;
        private readonly ILogger<LocationsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="logger"></param>
        public LocationsController(
            IObservationManager observationManager,
            ILogger<LocationsController> logger) 
        {
            _observationManager = observationManager ??
                                  throw new ArgumentNullException(nameof(observationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost]
        [ProducesResponseType(typeof(SOS.Lib.Models.Processed.Observation.Location), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLocationsAsync([FromBody]IEnumerable<string> locationIds)
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

                var locations = await _observationManager.GetLocationsAsync(locationIds);

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