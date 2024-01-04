using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Jobs.Shared;
using System;
using System.Net;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ToolsController : ControllerBase, IToolsController
    {
        private readonly ILogger<ToolsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ToolsController(ILogger<ToolsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleCleanUpJob(byte runIntervalInMinutes)
        {
            try
            {
                if (runIntervalInMinutes <= 0 || runIntervalInMinutes > 59)
                {

                    return new BadRequestObjectResult("Run interval must be between 1 and 59");
                }

                RecurringJob.AddOrUpdate<ICleanUpJob>(
                   nameof(ICleanUpJob), job => job.RunAsync(JobCancellationToken.Null),
                    $"*/{runIntervalInMinutes} * * * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

                return new OkObjectResult("Cleanup job scheduled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduling cleanup job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}