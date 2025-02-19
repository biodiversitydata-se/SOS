﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Models;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public DataProvidersController(
            IDataProviderManager dataProviderManager,
            IProcessInfoRepository processInfoRepository,
            ILogger<DataProvidersController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
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
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportDefaultDataprovidersAsync(
            [FromQuery] bool forceOverwriteIfCollectionExist = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var result = await _dataProviderManager.InitDefaultDataProviders(forceOverwriteIfCollectionExist);
                if (result.IsFailure) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Initialize the MongoDB DataProvider collection with default data provider.
        /// </summary>
        /// <param name="forceOverwriteIfCollectionExist">
        ///     If the DataProvider already exists, set
        ///     forceOverwriteIfCollectionExist to true if you want to overwrite this record with default data.
        /// </param>
        /// <param name="dataProviderIdOrIdentifier">Name or identifier of dataprovider</param>
        /// <returns></returns>
        [HttpPost("ImportDefaultDataProvider")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportDefaultDataproviderAsync(
            [FromQuery] string dataProviderIdOrIdentifier,
            [FromQuery] bool forceOverwriteIfCollectionExist = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dataProvider =
                        await _dataProviderManager.GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier);
                if (dataProvider != null && !forceOverwriteIfCollectionExist)
                {
                    return new BadRequestObjectResult(
                        $"Data provider already exist and forceOverwriteIfCollectionExist=false");
                }

                var result = await _dataProviderManager.InitDefaultDataProvider(dataProviderIdOrIdentifier);
                if (result.IsFailure) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);                
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }


        /// <summary>
        ///  Initialize the MongoDB DataProvider eml collection with default eml files for passed providers (No id's passed = all)
        /// </summary>
        /// <param name="datproviderIds"></param>
        /// <returns></returns>
        [HttpPost("ImportDefaultEml")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportDefaultEmlAsync([FromBody] IEnumerable<int> datproviderIds)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var result = await _dataProviderManager.InitDefaultEml(datproviderIds);
                if (result.IsFailure) return BadRequest(result.Error);
                return Ok(result.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                int.TryParse(model.DataProviderIdOrIdentifier, out var providerId);

                if (providerId != -1)
                {
                    var dataProvider =
                        await _dataProviderManager.GetDataProviderByIdOrIdentifier(model.DataProviderIdOrIdentifier);
                    if (dataProvider == null)
                    {
                        return new BadRequestObjectResult(
                            $"No data provider exist with Id={model.DataProviderIdOrIdentifier}");
                    }

                    providerId = dataProvider.Id;
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

                var res = await _dataProviderManager.SetEmlMetadataAsync(providerId, xmlDocument);
                if (res == false)
                {
                    return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                }

                return Ok($"Ok. The EML was updated for provider id: {providerId.ToString(CultureInfo.InvariantCulture)}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<string> ReadFormFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return await Task.FromResult((string)null);
            }

            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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
        [ProducesResponseType(typeof(IEnumerable<DataProvider>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersAsync()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dataProviders = await _dataProviderManager.GetAllDataProvidersAsync();

                //var dtos = dataProviders.Select(DataProviderDto.Create).ToList(); // todo - use DTO?
                return Ok(dataProviders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data providers");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get markdowns summary for all data providers.
        /// </summary>
        /// <param name="cultureCode">Culture code.</param>
        /// <param name="includeProvidersWithNoObservations">If false, data providers with no observations are excluded from the result.</param>
        /// <returns></returns>
        [HttpGet("GetMarkdownSummary")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProvidersMarkdownSummaryAsync(
            [FromQuery] string cultureCode = "en-GB",
            [FromQuery] bool includeProvidersWithNoObservations = false)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var dataProviders = await _dataProviderManager.GetAllDataProvidersAsync();
                var processInfo = (await _processInfoRepository.GetAllAsync())
                    .Where(m => m.Id.Contains("observation", StringComparison.InvariantCultureIgnoreCase))
                    .Where(m => m.Status == RunStatus.Success)
                    .Where(m => m.ProvidersInfo != null && m.ProvidersInfo.Count() > 1)
                    .OrderByDescending(m => m.End)
                    .First();                                
                var markdown = CreateMarkdown(dataProviders, processInfo.ProvidersInfo, cultureCode, includeProvidersWithNoObservations);               
                return Ok(markdown);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data providers");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private string CreateMarkdown(List<DataProvider> dataProviders, 
            IEnumerable<Lib.Models.Processed.ProcessInfo.ProviderInfo> providerInfos, 
            string cultureCode,
            bool includeProvidersWithNoObservations)
        {
            _logger.LogInformation("CreateMarkdown culture (before change): {@currentCulture}, numberGroupSeparator: {@numberGroupSeparator}", Thread.CurrentThread.CurrentCulture.Name, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE");
            _logger.LogInformation("CreateMarkdown culture (after change): {@currentCulture}, numberGroupSeparator: {@numberGroupSeparator}", Thread.CurrentThread.CurrentCulture.Name, Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator);
            int totalCount = 0;
            var sb = new StringBuilder();
            sb.AppendLine("| Id 	| Name 	| Organization 	| Number of observations 	|");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var dataProvider in dataProviders)
            {
                var providerInfo = providerInfos.FirstOrDefault(m => m.DataProviderId == dataProvider.Id);
                if (providerInfo == null) continue;
                int observationCount = providerInfo.PublicProcessCount.GetValueOrDefault(0) + providerInfo.ProtectedProcessCount.GetValueOrDefault(0);
                if (!includeProvidersWithNoObservations && observationCount == 0) continue;

                sb.AppendLine($"| {dataProvider.Id} | [{dataProvider.Names.Translate(cultureCode)}]({dataProvider.Url}) | {dataProvider.Organizations.Translate(cultureCode)} | {observationCount:N0} |");
                totalCount += observationCount;
            }
            sb.AppendLine($"|  |  |  | **{totalCount:N0}** |");

            return sb.ToString();
        }

        /// <summary>
        ///     Get information about which data sources are included in scheduled harvest and processing.
        /// </summary>
        /// <returns></returns>
        [HttpGet("HarvestAndProcessSettings")]
        [ProducesResponseType(typeof(DataProviderHarvestAndProcessSettingsDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DataProviderHarvestSettings()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}