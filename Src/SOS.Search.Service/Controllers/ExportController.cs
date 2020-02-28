using System;
using System.Net;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Export.Jobs.Interfaces;
using SOS.Lib.Models.Search;

namespace SOS.Search.Service.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportController : ControllerBase, Interfaces.IExportController
    {
        private readonly ILogger<ExportController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ExportController(ILogger<ExportController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunExportJob([FromBody]AdvancedFilter filter, [FromQuery]string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");

                if (!string.IsNullOrEmpty(email) && !emailRegex.IsMatch(email))
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
        [HttpGet("Status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetExportStatus([FromQuery]string jobId)
        {
            try
            {
                var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                return new OkObjectResult(jobData.State);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting export job status failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
