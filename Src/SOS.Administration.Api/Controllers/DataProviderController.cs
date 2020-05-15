using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Managers.Interfaces;

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

        /// <summary>
        /// Initialize the DataProvider collection with default data providers.
        /// </summary>
        /// <param name="forceOverwriteIfCollectionExist">If the DataProvider collection already exists, set forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.</param>
        /// <returns></returns>
        [HttpPost("CreateDefaultDataProviders")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateDefaultDataprovidersAsync([FromQuery]bool forceOverwriteIfCollectionExist = false)
        {
            try
            {
                var result = await _dataProviderManager.InitDefaultDataProviders(forceOverwriteIfCollectionExist);
                if (result.IsFailure) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}