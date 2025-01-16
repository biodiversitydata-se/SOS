﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Swagger;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Caches controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CachesController : ControllerBase
    {
        private readonly IAreaCache _areaCache;
        private readonly IDataProviderCache _dataProvidersCache;
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ICache<int, ProjectInfo> _projectsCache;
        private readonly ICache<int, TaxonList> _taxonListCache;
        private readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        private readonly ILogger<CachesController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaCache"></param>
        /// <param name="dataProvidersCache"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="projectsCache"></param>
        /// <param name="taxonListCache"></param>
        /// <param name="vocabularyCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CachesController(
            IAreaCache areaCache,
            IDataProviderCache dataProvidersCache,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ICache<int, ProjectInfo> projectsCache,
            ICache<int, TaxonList> taxonListCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            ILogger<CachesController> logger)
        {
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _dataProvidersCache = dataProvidersCache ?? throw new ArgumentNullException(nameof(dataProvidersCache));
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
            _projectsCache = projectsCache ?? throw new ArgumentNullException(nameof(projectsCache));
            _taxonListCache = taxonListCache ?? throw new ArgumentNullException(nameof(taxonListCache));
            _vocabularyCache = vocabularyCache ?? throw new ArgumentNullException(nameof(vocabularyCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Clear a cache
        /// </summary>
        /// <param name="cache">The cache to clear.</param>
        /// <returns></returns>
        [HttpDelete("{cache}")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> DeleteCache([FromRoute] Cache cache)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                switch (cache)
                {
                    case Cache.Area:
                        await _areaCache.ClearAsync();
                        break;
                    case Cache.DataProviders:
                        await _dataProvidersCache.ClearAsync();
                        break;
                    case Cache.ProcessedConfiguration:
                        await _processedConfigurationCache.ClearAsync();
                        break;
                    case Cache.Projects:
                        await _projectsCache.ClearAsync();
                        break;
                    case Cache.TaxonLists:
                        await _taxonListCache.ClearAsync();
                        break;
                    case Cache.Vocabulary:
                        await _vocabularyCache.ClearAsync();
                        break;
                }

                _logger.LogInformation($"The {cache} cache was cleared");
                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error clearing the {cache} cache");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns> 
        [HttpGet("{cache}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetCache([FromRoute] Cache cache)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                switch (cache)
                {
                    case Cache.Area:
                        var areas = await _areaCache.GetAllAsync();
                        return new OkObjectResult(areas);
                    case Cache.DataProviders:
                        var dataProviders = await _dataProvidersCache.GetAllAsync();
                        return new OkObjectResult(dataProviders);
                    case Cache.ProcessedConfiguration:
                        var processedConfiguration = await _processedConfigurationCache.GetAllAsync();
                        return new OkObjectResult(processedConfiguration);
                    case Cache.Projects:
                        var projects = await _projectsCache.GetAllAsync();
                        return new OkObjectResult(projects);
                    case Cache.TaxonLists:
                        var taxonLists = await _taxonListCache.GetAllAsync();
                        return new OkObjectResult(taxonLists);
                    case Cache.Vocabulary:
                        var vocabulary = await _vocabularyCache.GetAllAsync();
                        return new OkObjectResult(vocabulary);
                    default:
                        _logger.LogError($"{cache} is not supported");
                        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting {cache} cache");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}