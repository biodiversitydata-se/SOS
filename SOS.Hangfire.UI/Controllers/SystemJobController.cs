using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Process.Jobs.Interfaces;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SystemJobController : ControllerBase, Interfaces.ISystemJobController
    {
        private readonly ILogger<SystemJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public SystemJobController(ILogger<SystemJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Instance/Activate")]
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
