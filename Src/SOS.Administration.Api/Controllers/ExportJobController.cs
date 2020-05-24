using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search;

namespace SOS.Administration.Api.Controllers
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunExportAndStoreJob([FromBody]ExportFilter filter, [FromQuery]string blobStorageContainer, [FromQuery]string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(blobStorageContainer))
                {
                    return BadRequest("You must provide a container");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("You must provide a file name");
                }

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndStoreJob>(job => job.RunAsync(filter, blobStorageContainer, fileName, false, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("DarwinCore/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyExportAndStoreJob([FromBody]ExportFilter filter, [FromQuery]string blobStorageContainer, [FromQuery]string fileName, [FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                if (string.IsNullOrEmpty(blobStorageContainer))
                {
                    return BadRequest("You must provide a container");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("You must provide a file name");
                }

                RecurringJob.AddOrUpdate<IExportAndStoreJob>(nameof(IExportAndStoreJob), job => job.RunAsync(filter, blobStorageContainer, fileName, false, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult($"Export Darwin Core Job Scheduled.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduling Darwin Core job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
