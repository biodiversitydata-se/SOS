using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.ApplicationInsights;
using SOS.Lib.Services.Interfaces;

namespace SOS.Administration.Gui.Controllers
{    
    [ApiController]
    [Route("[controller]")]
    public class ApplicationInsightsController : ControllerBase
    {
        private IApplicationInsightsService _applicationInsightsService;
        private readonly ILogger<ApplicationInsightsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationInsightsService"></param>
        /// <param name="logger"></param>
        public ApplicationInsightsController(IApplicationInsightsService applicationInsightsService, ILogger<ApplicationInsightsController> logger)
        {
            _applicationInsightsService = applicationInsightsService ?? throw new ArgumentNullException(nameof(applicationInsightsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        ///     Get information about which data sources are included in scheduled harvest and processing.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Search")]
        [ProducesResponseType(typeof(IEnumerable<ApiLogRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLogDataAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var logData = await _applicationInsightsService.GetLogDataAsync(from, to);

                return Ok(logData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
