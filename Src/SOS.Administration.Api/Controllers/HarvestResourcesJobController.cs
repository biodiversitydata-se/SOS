using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Models;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Harvest resources job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestResourcesJobController : ControllerBase, Interfaces.IHarvestResourcesJobController
    {
        private readonly ILogger<HarvestObservationsJobController> _logger;

        /// <summary>
        /// Constructor
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
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyAreasHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IAreasHarvestJob>(nameof(IAreasHarvestJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Areas harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding areas harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Areas/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Geo

        #region FieldMapping
        /// <inheritdoc />
        [HttpPost("FieldMapping/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunImportFieldMappingJob()
        {
            try
            {
                BackgroundJob.Enqueue<IFieldMappingImportJob>(job => job.RunAsync());
                return new OkObjectResult("Import field mapping job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing import field mapping job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion FieldMapping

        #region Taxon
        /// <inheritdoc />
        [HttpPost("Taxon/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyTaxonHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<ITaxonHarvestJob>(nameof(ITaxonHarvestJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Taxon harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding taxon harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxon/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunTaxonHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<ITaxonHarvestJob>(job => job.RunAsync());
                return new OkObjectResult("Taxon harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Taxon harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Taxon
    }
}
