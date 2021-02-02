using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportsController : ObservationBaseController, IExportsController
    {
        private readonly IBlobStorageManager _blobStorageManager;
        private readonly long _exportObservationsLimit;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="blobStorageManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportsController(IObservationManager observationManager, 
            IBlobStorageManager blobStorageManager, 
            ITaxonManager taxonManager, 
            ObservationApiConfiguration configuration, 
            ILogger<ExportsController> logger) :base(observationManager, taxonManager)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportObservationsLimit = configuration?.ExportObservationsLimit ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetExportFilesAsync()
        {
            try
            {
                var files = await _blobStorageManager.GetExportFilesAsync();

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
        [HttpPost("Create")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunExportAndSendJob([FromBody] ExportFilterDto filter)
        {
            try
            {
                var email = User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;

                var validationResults = Result.Combine(
                    ValidateSearchFilter(filter),
                    ValidateEmail(email));

                if (validationResults.IsFailure)
                {
                    return BadRequest(validationResults.Error);
                }

                var exportFilter = filter.ToSearchFilter("en-GB", false);
                var matchCount = await ObservationManager.GetMatchCountAsync(exportFilter);

                if (matchCount == 0)
                {
                    return NoContent();
                }

                if (matchCount > _exportObservationsLimit)
                {
                    return BadRequest($"Query exceeds limit of {_exportObservationsLimit} observations.");
                }

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(null, email, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}