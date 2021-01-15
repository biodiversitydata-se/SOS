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
    [Authorize/*(Roles = "Privat")*/]
    public class DOIsController : ObservationBaseController, IDOIsController
    {
        private readonly long _exportObservationsLimit;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        public DOIsController(IObservationManager observationManager,
            ITaxonManager taxonManager,
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<ExportsController> logger) : base(observationManager, taxonManager)
        {
            _exportObservationsLimit = observationApiConfiguration?.ExportObservationsLimit ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCreateDOIJobAsync([FromBody] ExportFilterDto filter)
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
                var exportFilter = filter.ToSearchFilter("en-GB");
                var matchCount = await ObservationManager.GetMatchCountAsync(exportFilter);

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
    }
}