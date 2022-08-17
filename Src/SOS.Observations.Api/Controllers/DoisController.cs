using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Services.Interfaces;
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
    [Authorize/*(Roles = "Privat")*/]
    public class DOIsController : ObservationBaseController, IDOIsController
    {
        private readonly IDataCiteService _dataCiteService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly long _exportObservationsLimit;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="dataCiteService"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DOIsController(IObservationManager observationManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            IDataCiteService dataCiteService,
            IBlobStorageService blobStorageService,
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<ExportsController> logger) : base(observationManager, areaManager, taxonManager, observationApiConfiguration)
        {
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _blobStorageService = blobStorageService ?? throw new ArgumentException(nameof(blobStorageService));
            _exportObservationsLimit = observationApiConfiguration?.OrderExportObservationsLimit ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateDOI([FromBody] ExportFilterDto filter)
        {
            try
            {
                var validationResults = Result.Combine(
                    ValidateSearchFilter(filter));

                if (validationResults.IsFailure)
                {
                    return BadRequest(validationResults.Error);
                }
                
                var creatorEmail = User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;
                var exportFilter = filter.ToSearchFilter(UserId, false, "en-GB");
                var matchCount = await ObservationManager.GetMatchCountAsync(0, null, exportFilter);

                if (matchCount == 0)
                {
                    return NoContent();
                }

                if (matchCount > _exportObservationsLimit)
                {
                    return BadRequest($"Query exceeds limit of {_exportObservationsLimit} observations.");
                }

                var jobId = BackgroundJob.Enqueue<ICreateDoiJob>(job =>
                    job.RunAsync(exportFilter, creatorEmail, JobCancellationToken.Null));

                return new OkObjectResult(new { jobId });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running DOI failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBatchAsync([FromQuery] int take = 10, [FromQuery] int page = 1, [FromQuery] string orderBy = "created", [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Desc)
        {
            try
            {
                var metadata = await _dataCiteService.GetBatchAsync(take, page, orderBy, sortOrder);

                return new OkObjectResult(metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting DOI metadata failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{prefix}/{suffix}/URL")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetDOIFileUrl([FromRoute] string prefix, [FromRoute] string suffix)
        {
            try
            {
                var downloadUrl = _blobStorageService.GetDOIDownloadUrl(prefix, suffix);

                return new OkObjectResult(downloadUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting DOI file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{prefix}/{suffix}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMetadata([FromRoute] string prefix, [FromRoute] string suffix)
        {
            try
            {
                if (string.IsNullOrEmpty(suffix))
                {
                    return BadRequest("You must provide a doi");
                }

                var metadata = await _dataCiteService.GetMetadataAsync(prefix, suffix);

                return new OkObjectResult(metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting DOI metadata failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchMetadata([FromQuery] string searchFor)
        {
            try
            {
                if (string.IsNullOrEmpty(searchFor))
                {
                    return BadRequest("You must provide something to search for");
                }

                var metadata = await _dataCiteService.SearchMetadataAsync(searchFor);

                return new OkObjectResult(metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting DOI metadata failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}