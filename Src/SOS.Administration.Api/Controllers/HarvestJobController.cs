using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Models;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Jobs.Import;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestJobsController : ControllerBase, Interfaces.IHarvestJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<HarvestJobsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public HarvestJobsController(
            IDataProviderManager dataProviderManager,
            ILogger<HarvestJobsController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
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
                RecurringJob.AddOrUpdate<IClamPortalHarvestJob>(nameof(IClamPortalHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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

        #region Areas
        /// <inheritdoc />
        [HttpPost("Areas/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyGeoAreasHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IGeoAreasHarvestJob>(nameof(IGeoAreasHarvestJob), job => job.RunAsync(), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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
        public IActionResult RunGeoAreasHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IGeoAreasHarvestJob>(job => job.RunAsync());
                return new OkObjectResult("Started areas harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running areas harvest job failed");
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
                RecurringJob.AddOrUpdate<IKulHarvestJob>(nameof(IKulHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
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

        #region Artportalen
        /// <inheritdoc />
        [HttpPost("Artportalen/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyArtportalenHarvestJob([FromQuery]int hour, [FromQuery]int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IArtportalenHarvestJob>(nameof(IArtportalenHarvestJob), job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Artportalen harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Artportalen harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Artportalen/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunArtportalenHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IArtportalenHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Started Artportalen harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running Artportalen harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Artportalen

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
                return new OkObjectResult("Started taxon harvest job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running taxon harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Taxon

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
                return new OkObjectResult("Started import field mapping job");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running import field mapping job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion FieldMapping

        #region DwC-A
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("DwcArchive/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RunDwcArchiveHarvestJob([FromForm]UploadDwcArchiveModelDto model)
        {
            try
            {
                var dataProvider = await _dataProviderManager.TryGetDataProviderAsync(model.DataProviderId);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult($"No data provider exist with Id={model.DataProviderId}");
                }

                if (model.DwcaFile.Length == 0)
                {
                    return new BadRequestObjectResult("No file content");
                }
                
                //var filePath = Path.GetTempFileName();
                var filePath = System.IO.Path.Combine(Path.GetTempPath(), model.DwcaFile.FileName);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);

                // process uploaded file
                BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job => job.RunAsync(dataProvider.Id, filePath, JobCancellationToken.Null));
                return new OkObjectResult("Started DwC-A harvest job");
               
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running DwC-A harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion DwC-A
    }
}
