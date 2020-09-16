using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportsController : ControllerBase, IExportsController
    {
        private readonly IObservationManager _observationManager;
        private readonly IBlobStorageManager _blobStorageManager;
        private readonly long _exportObservationsLimit;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="blobStorageManager"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportsController(IObservationManager observationManager, IBlobStorageManager blobStorageManager, ObservationApiConfiguration configuration, ILogger<ExportsController> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportObservationsLimit = configuration?.ExportObservationsLimit ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult GetExportFiles()
        {
            try
            {
                var files = _blobStorageManager.GetExportFiles();

                return new OkObjectResult(files);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting export files");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{fileName}/URL")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetExportFileUrl([FromRoute] string fileName)
        {
            try
            {
                var downloadUrl = _blobStorageManager.GetExportDownloadUrl(fileName);

                return new OkObjectResult(downloadUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting export file URL");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("Status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetExportStatus([FromQuery] string jobId)
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

        /// <inheritdoc />
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunExportAndSendJob([FromBody] ExportFilter filter, [FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest("You must provide a e-mail address");
                }
                
                var emailRegex =
                    new Regex(
                        @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");
                if (!emailRegex.IsMatch(email))
                {
                    return BadRequest("Not a valid e-mail");
                }

                var matchCount = await _observationManager.GetMatchCountAsync(filter);

                if (matchCount == 0)
                {
                    return NoContent();
                }

                if (matchCount > _exportObservationsLimit)
                {
                    return BadRequest($"Query exceeds limit of {_exportObservationsLimit} observations.");
                }

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(filter, email, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}