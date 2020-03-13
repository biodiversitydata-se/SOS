using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class InstanceJobController : ControllerBase, Interfaces.IInstanceJobController
    {
        private readonly ILogger<InstanceJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public InstanceJobController(ILogger<InstanceJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Copy")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunCopyProviderData(DataProvider provider)
        {
            try
            {
                BackgroundJob.Enqueue<ICopyProviderDataJob>(job => job.RunAsync(provider));
                return new OkObjectResult("Started copy provider data job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting copy provider data failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Activate")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunSetActivateInstanceJob([FromQuery]byte instance)
        {
            try
            {
                if (instance < 0 || instance > 1)
                {
                    _logger.LogError( "Instance must be 0 or 1");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                BackgroundJob.Enqueue<IActivateInstanceJob>(job => job.RunAsync(instance));
                return new OkObjectResult("Started activate instance job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting activate instance failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
