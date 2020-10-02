using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers.V2
{
    /// <summary>
    /// Data providers controller V2. This currently exists just for testing API versioning.
    /// </summary>
    [ApiVersion("2.0")]
    [Route("[controller]")]
    [ApiController]
    public class DataProvidersController : ControllerBase
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<DataProvidersController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public DataProvidersController(
            IDataProviderManager dataProviderManager,
            ILogger<DataProvidersController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(List<DataProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersAsync()
        {
            try
            {
                var dataProviders = await _dataProviderManager.GetDataProvidersAsync(true);
                return Ok(dataProviders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data providers");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}