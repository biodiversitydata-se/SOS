using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Jobs.Import;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Harvest resources job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestResourcesJobController : ControllerBase, IHarvestResourcesJobController
    {
        private readonly ILogger<HarvestObservationsJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HarvestResourcesJobController(
            ILogger<HarvestObservationsJobController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        #region Areas

        /// <inheritdoc />
        [HttpPost("Areas/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyAreasHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IAreasHarvestJob>(nameof(IAreasHarvestJob), job => job.RunAsync(),
                    $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Areas harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding areas harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Areas/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunAreasHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IAreasHarvestJob>(job => job.RunAsync());
                return new OkObjectResult("Areas harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Areas harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion Geo

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("HarvestTaxonLists/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunHarvestTaxonListsJob()
        {
            try
            {
                BackgroundJob.Enqueue<ITaxonListsHarvestJob>(job => job.RunHarvestTaxonListsAsync());
                return new OkObjectResult("Taxon lists harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing taxon lists harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}