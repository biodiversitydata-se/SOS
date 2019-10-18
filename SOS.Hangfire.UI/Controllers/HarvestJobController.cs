using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Jobs;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestJobsController : ControllerBase, Interfaces.IHarvestJobController
    {
        private readonly ILogger<HarvestJobsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HarvestJobsController(ILogger<HarvestJobsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("SpeciesPortal/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailySpeciesPortalHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<SpeciesPortalHarvestJob>(nameof(SpeciesPortalHarvestJob), job => job.Run(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Species Portal harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Species Portal harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("SpeciesPortal/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunSpeciesPortalHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<ISpeciesPortalHarvestJob>(job => job.Run());
                return new OkObjectResult("Started Species Portal harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running Species Portal harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
