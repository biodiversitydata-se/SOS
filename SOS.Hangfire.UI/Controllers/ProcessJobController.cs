using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Process.Jobs;
using SOS.Process.Jobs.Interfaces;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProcessJobController : ControllerBase, Interfaces.IProcessJobController
    {
        private readonly ILogger<ProcessJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ProcessJobController(ILogger<ProcessJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyProcessJob([FromQuery]int sources, [FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IProcessJob>(nameof(ProcessJob), job => job.Run(sources, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunProcessJob([FromQuery]int sources)
        {
            try
            {
                BackgroundJob.Enqueue<IProcessJob>(job => job.Run(sources, JobCancellationToken.Null));
                return new OkObjectResult("Started process job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("Kul/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunKulProcessJob()
        {
            try
            {
                int sources = (int) SightingProviders.KUL;
                BackgroundJob.Enqueue<IProcessJob>(job => job.Run(sources, JobCancellationToken.Null));
                return new OkObjectResult("Started process KUL job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting process KUL job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
