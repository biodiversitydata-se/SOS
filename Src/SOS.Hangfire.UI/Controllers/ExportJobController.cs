using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Export.Jobs;
using SOS.Export.Jobs.Interfaces;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportJobController : ControllerBase, Interfaces.IExportJobController
    {
        private readonly ILogger<ExportJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ExportJobController(ILogger<ExportJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("DarwinCore/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunDarwinCoreExportJob()
        {
            try
            {
                var fileName = Guid.NewGuid().ToString();
                BackgroundJob.Enqueue<IExportDarwinCoreJob>(job => job.RunAsync(fileName, JobCancellationToken.Null));

                return new OkObjectResult($"Running Darwin Core export. File name {fileName}.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running Darwin Core export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("DarwinCore/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyDarwinCoreExportJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString();
                RecurringJob.AddOrUpdate<IExportDarwinCoreJob>(nameof(ExportDarwinCoreJob), job => job.RunAsync(fileName, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult($"Export Darwin Core Job Scheduled. File name {fileName}.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduling Darwin Core job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
