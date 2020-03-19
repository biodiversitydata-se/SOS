using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;

namespace SOS.Administration.Api.Controllers
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
        public IActionResult ScheduleDailyProcessJob([FromQuery]int sources, [FromQuery]bool cleanStart = true, [FromQuery]bool toggleInstanceOnSuccess = true, [FromQuery]int hour = 0, [FromQuery]int minute = 0)
        {
            try
            {
                RecurringJob.AddOrUpdate<IProcessJob>(nameof(IProcessJob), job => job.RunAsync(sources, cleanStart, toggleInstanceOnSuccess, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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
        public IActionResult RunProcessJob([FromQuery]int sources, [FromQuery]bool cleanStart = true, [FromQuery]bool toggleInstanceOnSuccess = true)
        {
            try
            {
                BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(sources, cleanStart, toggleInstanceOnSuccess, JobCancellationToken.Null));
                return new OkObjectResult("Started process job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxa/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyProcessTaxaJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IProcessTaxaJob>(nameof(IProcessTaxaJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxa/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunProcessTaxaJob()
        {
            try
            {
                BackgroundJob.Enqueue<IProcessTaxaJob>(job => job.RunAsync());
                return new OkObjectResult("Started process job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("CopyFieldMapping/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunCopyFieldMappingJob()
        {
            try
            {
                BackgroundJob.Enqueue<ICopyFieldMappingsJob>(job => job.RunAsync());
                return new OkObjectResult("Started copy field mapping job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting copy field mapping job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("CopyAreas/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunCopyAreasJob()
        {
            try
            {
                BackgroundJob.Enqueue<ICopyAreasJob>(job => job.RunAsync());
                return new OkObjectResult("Started copy areas job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting copy areas job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}