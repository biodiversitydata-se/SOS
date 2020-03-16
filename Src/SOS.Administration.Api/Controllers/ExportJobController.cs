using System;
using System.Net;
using System.Text.RegularExpressions;
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
        public IActionResult RunDarwinCoreExportJob([FromBody]ExportFilter filter, [FromQuery]string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("You must provide a e-mail address");
                }

                if (!emailRegex.IsMatch(email))
                {
                    return BadRequest("Not a valid e-mail");
                }

                return new OkObjectResult(BackgroundJob.Enqueue<IExportJob>(job => job.RunAsync(filter, email, JobCancellationToken.Null)));
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
        public IActionResult ScheduleDailyDarwinCoreExportJob([FromBody]ExportFilter filter, [FromQuery]string email, [FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {

                RecurringJob.AddOrUpdate<IExportJob>(nameof(IExportJob), job => job.RunAsync(filter, email, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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
