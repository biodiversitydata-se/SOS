using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Data providers controller.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class DataProvidersController : ControllerBase, IDataProviderController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IObservationManager _observationManager;
        private readonly ILogger<DataProvidersController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public DataProvidersController(
            IDataProviderManager dataProviderManager,
            IObservationManager observationManager,
            ILogger<DataProvidersController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("")]
        [ProducesResponseType(typeof(List<DataProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersAsync([FromQuery] string cultureCode = "en-GB")
        {
            try
            {
                var dataProviders = await _dataProviderManager.GetDataProvidersAsync(false, cultureCode);
                return Ok(dataProviders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data providers");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        /// <inheritdoc/>
        [HttpGet("{providerId}/LastModified")]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLatestModifiedDateForProviderAsync([FromRoute] int providerId)
        {
            try
            {
                return new OkObjectResult(await _observationManager.GetLatestModifiedDateForProviderAsync(providerId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting last modified date for provider {providerId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}