using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Models;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Data providers controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
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

        /// <summary>
        ///     Initialize the MongoDB DataProvider collection with default data providers.
        /// </summary>
        /// <param name="forceOverwriteIfCollectionExist">
        ///     If the DataProvider collection already exists, set
        ///     forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.
        /// </param>
        /// <returns></returns>
        [HttpPost("ImportDefaultDataProviders")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportDefaultDataprovidersAsync(
            [FromQuery] bool forceOverwriteIfCollectionExist = false)
        {
            try
            {
                var result = await _dataProviderManager.InitDefaultDataProviders(forceOverwriteIfCollectionExist);
                if (result.IsFailure) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get all data providers.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(List<DataProvider>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersAsync()
        {
            try
            {
                var dataProviders = await _dataProviderManager.GetAllDataProviders();
                //var dtos = dataProviders.Select(DataProviderDto.Create).ToList(); // todo - use DTO?
                return Ok(dataProviders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data providers");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get information about which data sources are included in scheduled harvest and processing.
        /// </summary>
        /// <returns></returns>
        [HttpGet("HarvestAndProcessSettings")]
        [ProducesResponseType(typeof(DataProviderHarvestAndProcessSettingsDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DataProviderHarvestSettings()
        {
            try
            {
                var allDataProviders = await _dataProviderManager.GetAllDataProviders();
                var harvestProcessSettings = new DataProviderHarvestAndProcessSettingsDto
                {
                    IncludedInScheduledHarvest = allDataProviders
                        .Where(dataProvider => dataProvider.IncludeInScheduledHarvest)
                        .Select(dataProvider => dataProvider.ToString()).ToList(),
                    IncludedInProcessing = allDataProviders
                        .Where(dataProvider => dataProvider.IsActive)
                        .Select(dataProvider => dataProvider.ToString()).ToList(),
                    NotIncludedInScheduledHarvest = allDataProviders
                        .Where(dataProvider => !dataProvider.IncludeInScheduledHarvest)
                        .Select(dataProvider => dataProvider.ToString()).ToList(),
                    NotIncludedInProcessing = allDataProviders
                        .Where(dataProvider => !dataProvider.IsActive)
                        .Select(dataProvider => dataProvider.ToString()).ToList()
                };

                return Ok(harvestProcessSettings);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}