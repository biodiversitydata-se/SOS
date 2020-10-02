using System;
using System.Net;
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
    public class DOIsController : ControllerBase, IDOIsController
    {
        private readonly string _doiContainer;
        private readonly IObservationManager _observationManager;
        private readonly long _exportObservationsLimit;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        public DOIsController(IObservationManager observationManager, 
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<ExportsController> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _exportObservationsLimit = observationApiConfiguration?.ExportObservationsLimit ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCreateDOIJobAsync([FromBody] ExportFilter filter)
        {
            try
            {
                var matchCount = await _observationManager.GetMatchCountAsync(filter);

                if (matchCount == 0)
                {
                    return NoContent();
                }

                if (matchCount > _exportObservationsLimit)
                {
                    return BadRequest($"Query exceeds limit of {_exportObservationsLimit} observations.");
                }

                // Validate creators, title, publisher, publicationYear, resourceTypeGeneral

                var jobId = BackgroundJob.Enqueue<ICreateDoiJob>(job =>
                    job.RunAsync(filter, JobCancellationToken.Null));

                return new OkObjectResult(new { jobId });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running DOI failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}