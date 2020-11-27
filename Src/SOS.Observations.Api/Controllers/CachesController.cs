using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Swagger;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Area controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CachesController : BaseController, ICachesController
    {
        private readonly IAreaCache _areaCache;
        private readonly ICache<int, DataProvider> _dataProvidersCache;
        private readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        private readonly ILogger<CachesController> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="areaCache"></param>
        /// <param name="dataProvidersCache"></param>
        /// <param name="vocabularyCache"></param>
        /// <param name="logger"></param>
        public CachesController(
            IAreaCache areaCache,
            ICache<int, DataProvider> dataProvidersCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            ILogger<CachesController> logger)
        {
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _dataProvidersCache = dataProvidersCache ?? throw new ArgumentNullException(nameof(dataProvidersCache));
            _vocabularyCache = vocabularyCache ?? throw new ArgumentNullException(nameof(vocabularyCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpDelete]
        [ProducesResponseType(typeof(bool), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [InternalApi]
        public IActionResult ClearAsync(Cache cache)
        {
            try
            {
                switch (cache)
                {
                    case Cache.Area:
                        _areaCache.Clear();
                        break;
                    case Cache.DataProviders:
                        _dataProvidersCache.Clear();
                        break;
                    case Cache.Vocabulary:
                        _vocabularyCache.Clear();
                        break;
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error clearing {cache} cache");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}