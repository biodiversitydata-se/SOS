using System.Net;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Configuration;
using SOS.ElasticSearch.Proxy.Controllers.Interfaces;

namespace SOS.ElasticSearch.Proxy.Controllers
{
    /// <summary>
    ///     Caches controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CachesController : ControllerBase, ICachesController
    {
        private readonly ICache<string, ProcessedConfiguration> _processedConfigurationCache;
        private readonly ILogger<CachesController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CachesController(
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<CachesController> logger)
        {
         
            _processedConfigurationCache = processedConfigurationCache ?? throw new ArgumentNullException(nameof(processedConfigurationCache));
           
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpDelete("{cache}")]
        [ProducesResponseType(typeof(bool), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult DeleteCache([FromRoute] Cache cache)
        {
            try
            {
                _processedConfigurationCache.Clear();

                _logger.LogInformation($"The {cache} cache was cleared");
                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error clearing the {cache} cache");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}