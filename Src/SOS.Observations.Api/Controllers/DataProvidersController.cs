using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;
using Newtonsoft.Json;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;

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
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="observationManager"></param>
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
        [HttpGet]
        [ProducesResponseType(typeof(List<DataProviderDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDataProviders([FromQuery] string cultureCode = "sv-SE", [FromQuery] bool includeProvidersWithNoObservations = false)
        {
            try
            {
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var dataProviders = await _dataProviderManager.GetDataProvidersAsync(false, cultureCode, includeProvidersWithNoObservations);

                if (!dataProviders?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

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
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLastModifiedDateById([FromRoute] int providerId)
        {
            try
            {
                var modifiedDate = await _observationManager.GetLatestModifiedDateForProviderAsync(providerId);
                if (modifiedDate == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                return new OkObjectResult(modifiedDate);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting last modified date for provider {providerId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc/>
        [HttpGet("{providerId}/EML")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetEML([FromRoute] int providerId)
        {
            try
            {
                var returnJson = false;
                if (Request.Headers.ContainsKey("Accept"))
                {
                    returnJson = Request.Headers["Accept"].Equals("application/json");
                }

                var fileData = await _dataProviderManager.GetEmlFileAsync(providerId);

                if (!fileData?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                if (!returnJson)
                {
                    return new ContentResult
                    {
                        ContentType = "application/xml",
                        Content = Encoding.UTF8.GetString(fileData),
                        StatusCode = 200
                    };
                }

                var xDocument = await fileData.ToXmlAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xDocument.ToString());
                var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);

                return new ContentResult
                {
                    ContentType = "application/json",
                    Content = json,
                    StatusCode = 200
                };
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting EML date for provider {providerId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}