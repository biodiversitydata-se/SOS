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

        #region Clam Portal
        /// <inheritdoc />
        [HttpPost("ClamPortal/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyClamPortalHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<ClamPortalHarvestJob>(nameof(ClamPortalHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Clam Portal harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding clam Portal harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("ClamPortal/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunClamPortalHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IClamPortalHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Started clam Portal harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running clam Portal harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Clam Tree Portal

        #region Geo
        /// <inheritdoc />
        [HttpPost("Geo/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyGeoHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<GeoHarvestJob>(nameof(GeoHarvestJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Geo harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding geo harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Geo/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunGeoHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IGeoHarvestJob>(job => job.RunAsync());
                return new OkObjectResult("Started geo harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running geo harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Geo

        #region KUL
        /// <inheritdoc />
        [HttpPost("KUL/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyKulHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IKulHarvestJob>(nameof(KulHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("KUL harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding KUL harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("KUL/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunKulHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IKulHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Started KUL harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running KUL harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion KUL

        #region Species Portal
        /// <inheritdoc />
        [HttpPost("SpeciesPortal/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailySpeciesPortalHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<SpeciesPortalHarvestJob>(nameof(SpeciesPortalHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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
                BackgroundJob.Enqueue<ISpeciesPortalHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Started Species Portal harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running Species Portal harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Species Portal

        #region Taxon
        /// <inheritdoc />
        [HttpPost("Taxon/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyTaxonHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<TaxonHarvestJob>(nameof(TaxonHarvestJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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
                return new OkObjectResult("Started taxon harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running taxon harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Taxon
    }
}
