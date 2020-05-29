using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Harvest observations job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestObservationsJobController : ControllerBase, IHarvestObservationJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<HarvestObservationsJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public HarvestObservationsJobController(
            IDataProviderManager dataProviderManager,
            ILogger<HarvestObservationsJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers)
        {
            try
            {
                if (dataProviderIdOrIdentifiers == null || dataProviderIdOrIdentifiers.Count == 0)
                {
                    return new BadRequestObjectResult("dataProviderIdOrIdentifiers is not set");
                }

                var parsedDataProvidersResult =
                    await _dataProviderManager.GetDataProvidersByIdOrIdentifier(dataProviderIdOrIdentifiers);
                var parsedDataProvidersCombinedResult = Result.Combine(parsedDataProvidersResult);
                if (parsedDataProvidersCombinedResult.IsFailure)
                {
                    return new BadRequestObjectResult(parsedDataProvidersCombinedResult.Error);
                }

                if (parsedDataProvidersResult.Any(providerResult => providerResult.Value.Type == DataProviderType.DwcA))
                {
                    return new BadRequestObjectResult(
                        "A DwC-A data provider was included in the list. Currently DwC-A harvesting is only supported by providing a file using the Administration API.");
                }

                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunHarvestObservationsAsync(
                    parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                    JobCancellationToken.Null));

                return new OkObjectResult(
                    $"Harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers,
            [FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                if (dataProviderIdOrIdentifiers == null || dataProviderIdOrIdentifiers.Count == 0)
                {
                    return new BadRequestObjectResult("dataProviderIdOrIdentifiers is not set");
                }

                var parsedDataProvidersResult =
                    await _dataProviderManager.GetDataProvidersByIdOrIdentifier(dataProviderIdOrIdentifiers);
                var parsedDataProvidersCombinedResult = Result.Combine(parsedDataProvidersResult);
                if (parsedDataProvidersCombinedResult.IsFailure)
                {
                    return new BadRequestObjectResult(parsedDataProvidersCombinedResult.Error);
                }

                if (parsedDataProvidersResult.Any(providerResult => providerResult.Value.Type == DataProviderType.DwcA))
                {
                    return new BadRequestObjectResult(
                        "A DwC-A data provider was included in the list. Currently DwC-A harvesting is only supported by providing a file using the Administration API.");
                }

                RecurringJob.AddOrUpdate<IObservationsHarvestJob>(
                    nameof(IObservationsHarvestJob),
                    job => job.RunHarvestObservationsAsync(
                        parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                        JobCancellationToken.Null),
                    $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);

                return new OkObjectResult(
                    $"Scheduled harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #region DwC-A

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("DwcArchive/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RunDwcArchiveHarvestJob([FromForm] UploadDwcArchiveModelDto model)
        {
            try
            {
                var dataProvider =
                    await _dataProviderManager.GetDataProviderByIdOrIdentifier(model.DataProviderIdOrIdentifier);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult(
                        $"No data provider exist with Id={model.DataProviderIdOrIdentifier}");
                }

                if (dataProvider.Type != DataProviderType.DwcA)
                {
                    return new BadRequestObjectResult($"The data provider \"{dataProvider}\" is not a DwC-A provider");
                }

                if (model.DwcaFile.Length == 0)
                {
                    return new BadRequestObjectResult("No file content");
                }

                //var filePath = Path.GetTempFileName();
                var filePath = Path.Combine(Path.GetTempPath(), model.DwcaFile.FileName);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);

                // process uploaded file
                BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job =>
                    job.RunAsync(dataProvider.Id, filePath, JobCancellationToken.Null));
                return new OkObjectResult(
                    $"DwC-A harvest job for data provider: {dataProvider} was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing DwC-A harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion DwC-A

        #region Artportalen

        /// <inheritdoc />
        [HttpPost("Artportalen/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyArtportalenHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IArtportalenHarvestJob>(nameof(IArtportalenHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Artportalen harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Artportalen harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Artportalen/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunArtportalenHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IArtportalenHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Artportalen harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueueing Artportalen harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion Artportalen

        #region Clam Portal

        /// <inheritdoc />
        [HttpPost("ClamPortal/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyClamPortalHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IClamPortalHarvestJob>(nameof(IClamPortalHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Clam Portal harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding clam Portal harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("ClamPortal/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunClamPortalHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IClamPortalHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Clam Portal harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Clam Portal harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion Clam Tree Portal

        #region KUL

        /// <inheritdoc />
        [HttpPost("KUL/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyKulHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IKulHarvestJob>(nameof(IKulHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("KUL harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding KUL harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("KUL/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunKulHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IKulHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("KUL harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing KUL harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion KUL

        #region MVM

        /// <inheritdoc />
        [HttpPost("MVM/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyMvmHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IMvmHarvestJob>(nameof(IMvmHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("MVM harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding MVM harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("MVM/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunMvmHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IMvmHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("MVM harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing MVM harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion MVM

        #region NORS

        /// <inheritdoc />
        [HttpPost("NORS/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyNorsHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<INorsHarvestJob>(nameof(INorsHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("NORS harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding NORS harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("NORS/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunNorsHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<INorsHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("NORS harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing NORS harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion NORS

        #region SERS

        /// <inheritdoc />
        [HttpPost("SERS/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailySersHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<ISersHarvestJob>(nameof(ISersHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("SERS harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding SERS harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("SERS/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunSersHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<ISersHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("SERS harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing SERS harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion SERS

        #region SHARK

        /// <inheritdoc />
        [HttpPost("SHARK/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailySharkHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<ISharkHarvestJob>(nameof(ISharkHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("SHARK harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding SHARK harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("SHARK/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunSharkHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<ISharkHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("SHARK harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing SHARK harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion SHARK

        #region Virtual Herbarium

        /// <inheritdoc />
        [HttpPost("VirtualHerbarium/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyVirtualHerbariumHarvestJob(int hour, int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IVirtualHerbariumHarvestJob>(nameof(IVirtualHerbariumHarvestJob),
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Virtual Herbarium harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Virtual Herbarium harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("VirtualHerbarium/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunVirtualHerbariumHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IVirtualHerbariumHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Virtual Herbarium harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Virtual Herbarium harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion Virtual Herbarium
    }
}