using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Import;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestAndProcessJobController : ControllerBase, Interfaces.IHarvestAndProcessJobController
    {
        private readonly ILogger<HarvestAndProcessJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HarvestAndProcessJobController(ILogger<HarvestAndProcessJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Observations/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyObservationHarvestAndProcessJob([FromQuery]int harvestSources, [FromQuery]int processSources, [FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IObservationsHarvestJob>(nameof(IObservationsHarvestJob), job => job.RunAsync(harvestSources, processSources, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Observations harvest and process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding observations harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Observations/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunObservationHarvestAndProcessJob([FromQuery]int harvestSources, [FromQuery]int processSources)
        {
            try
            {
                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunAsync(harvestSources, processSources, JobCancellationToken.Null));
                return new OkObjectResult("Started observations harvest and process job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running observations harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
