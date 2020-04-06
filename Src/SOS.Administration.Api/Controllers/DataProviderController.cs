using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Field mapping controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DataProviderController : ControllerBase, Interfaces.IDataProviderController
    {
        private readonly ILogger<DataProviderController> _logger;
        private readonly IDataProviderManager _dataProviderManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public DataProviderController(
            IDataProviderManager dataProviderManager,
            ILogger<DataProviderController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("CreateDefaultDataProviders")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateDefaultDataprovidersAsync()
        {
            try
            {
                await _dataProviderManager.InitDefaultDataProviders();
                return new OkObjectResult("Default data providers created");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}