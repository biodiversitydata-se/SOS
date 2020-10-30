using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cronos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Models;
using SOS.Lib.Managers.Interfaces;
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
        ///     Import EML metadata for a data provider.
        /// </summary>
        /// <returns></returns>
        [HttpPost("ImportEmlMetadata")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportEmlMetadataAsync(
            [FromForm] ImportEmlFileDto model)
        {
            try
            {
                var dataProvider =
                    await _dataProviderManager.GetDataProviderByIdOrIdentifier(model.DataProviderIdOrIdentifier);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult(
                        $"No data provider exist with Id={model.DataProviderIdOrIdentifier}");
                }

                if (model.File == null || model.File.Length == 0)
                {
                    return new BadRequestObjectResult("No file is provided");
                }

                if (!model.File.FileName.EndsWith(".xml"))
                {
                    return new BadRequestObjectResult(
                        $"Only XML files is supported. This file has the file extension {Path.GetExtension(model.File.FileName)}");
                }

                using var reader = new StreamReader(model.File.OpenReadStream());
                var xmlDocument = XDocument.Load(reader);
                if (xmlDocument.Root == null)
                {
                    return new BadRequestObjectResult(
                        $"The file doesn't seem to be an XML file.");
                }

                if (xmlDocument.Root.Name.LocalName != "eml")
                {
                    return new BadRequestObjectResult(
                        $"The file doesn't seem to be an EML XML file. The root should be eml, but is {xmlDocument.Root.Name.LocalName}");
                }

                var res = await _dataProviderManager.SetEmlMetadataAsync(dataProvider.Id, xmlDocument);
                if (res == false)
                {
                    return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                }

                return Ok($"Ok. The EML was updated for {dataProvider}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        public static async Task<string> ReadFormFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return await Task.FromResult((string)null);
            }

            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        public static async Task<string> ReadAsStringAsync(IFormFile file)
        {
            var result = new StringBuilder();
            using var reader = new StreamReader(file.OpenReadStream());
            while (reader.Peek() >= 0)
                result.AppendLine(await reader.ReadLineAsync());
            return result.ToString();
        }

        /// <summary>
        ///     Get all data providers.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DataProvider>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersAsync()
        {
            try
            {
                var dataProviders = await _dataProviderManager.GetAllDataProvidersAsync();

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
                var allDataProviders = await _dataProviderManager.GetAllDataProvidersAsync();
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