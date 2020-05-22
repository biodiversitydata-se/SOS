using System;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DOIsController : ControllerBase, Interfaces.IDOIsController
    {
        private readonly IDOIManager _doiManager;
        private readonly string _doiContainer;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public DOIsController(IDOIManager doiManager, BlobStorageConfiguration configuration, ILogger<ExportsController> logger)
        {
            _doiManager = doiManager ?? throw new ArgumentNullException(nameof(doiManager));
            _doiContainer = configuration?.DOI_Container ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        [HttpPost]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCreateDOIJobAsync([FromBody]ExportFilter filter)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString();
                var jobId = BackgroundJob.Enqueue<IExportAndStoreJob>(job =>
                    job.RunAsync(filter, _doiContainer, fileName, true, JobCancellationToken.Null));

                return new OkObjectResult(new { fileName, jobId });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running DOI failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc/>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DOI>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDOIsAsync([FromQuery]int skip = 0, [FromQuery]int take = 100)
        {
            try
            {
                var dois = await _doiManager.GetDOIsAsync(skip, take);

                return new OkObjectResult(dois);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting export job status failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{id}/URL")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetDOIFileUrl([FromRoute]Guid id)
        {
            try
            {
                var downloadUrl = _doiManager.GetDOIDownloadUrl(id);

                return new OkObjectResult(downloadUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting DOI file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
