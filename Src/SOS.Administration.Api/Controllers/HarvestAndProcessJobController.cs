using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestAndProcessJobController : ControllerBase, IHarvestAndProcessJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<HarvestAndProcessJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public HarvestAndProcessJobController(
            IDataProviderManager dataProviderManager,
            ILogger<HarvestAndProcessJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Observations/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyObservationHarvestAndProcessJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IObservationsHarvestJob>($"{nameof(IObservationsHarvestJob)}-Full",
                    job => job.RunFullAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Observations harvest and process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding observations harvest and process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Observations/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunObservationHarvestAndProcessJob()
        {
            try
            {
                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunFullAsync(JobCancellationToken.Null));
                return new OkObjectResult("Observations harvest and process job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing observations harvest and process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Observations/SelectedDataProviders/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunObservationHarvestAndProcessJob(
            [FromQuery] List<string> harvestDataProviderIdOrIdentifiers,
            [FromQuery] List<string> processDataProviderIdOrIdentifiers)
        {
            try
            {
                if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
                {
                    return new BadRequestObjectResult("harvestDataProviderIdOrIdentifiers is not set");
                }

                if (processDataProviderIdOrIdentifiers == null || processDataProviderIdOrIdentifiers.Count == 0)
                {
                    return new BadRequestObjectResult("processDataProviderIdOrIdentifiers is not set");
                }

                var harvestDataProviders =
                    await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
                var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
                if (harvestDataProvidersResult.IsFailure)
                {
                    return new BadRequestObjectResult(harvestDataProvidersResult.Error);
                }

                var processDataProviders =
                    await _dataProviderManager.GetDataProvidersByIdOrIdentifier(processDataProviderIdOrIdentifiers);
                var processDataProvidersResult = Result.Combine(processDataProviders);
                if (processDataProvidersResult.IsFailure)
                {
                    return new BadRequestObjectResult(processDataProvidersResult.Error);
                }

                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunAsync(
                    harvestDataProviderIdOrIdentifiers,
                    processDataProviderIdOrIdentifiers,
                    JobCancellationToken.Null));

                return new OkObjectResult("Process job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Observations/Run/Incremental")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult RunIncrementalObservationHarvestAndProcessJob()
        {
            try
            {
                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunIncrementalActiveAsync(
                  JobCancellationToken.Null));

                

                return new OkObjectResult("Incremental observation Harvest and process job enqued");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enquing incremental observation Harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("Observations/Run/Incremental/Artportalen")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult RunIncrementalArtportalenObservationHarvestAndProcessJob([FromBody] IEnumerable<int> ids)
        {
            try
            {
                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunHarvestArtportalenObservationsAsync(ids,
                    JobCancellationToken.Null));

                return new OkObjectResult("Incremental observation Harvest and process job for specified Artportalen id's enqued");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Incremental observation Harvest and process job for specified Artportalen id's failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Observations/Schedule/Incremental")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ScheduleIncrementalObservationHarvestAndProcessJob([FromQuery]byte runIntervalInMinutes)
        {
            try
            {
                if (runIntervalInMinutes <= 0 || runIntervalInMinutes > 59)
                {

                    return new BadRequestObjectResult("Run interval must be between 1 and 59");
                }

                RecurringJob.AddOrUpdate<IObservationsHarvestJob>(
                    $"{nameof(IObservationsHarvestJob)}-Incremental", job => job.RunIncrementalActiveAsync(JobCancellationToken.Null),
                    $"*/{runIntervalInMinutes} * * * *", TimeZoneInfo.Local);

                return new OkObjectResult("Incremental observation Harvest and process job scheduled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduling incremental observation Harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Checklists/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyChecklistHarvestAndProcessJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IChecklistsHarvestJob>($"{nameof(IChecklistsHarvestJob)}-Full",
                    job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Check lists harvest and process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding checklists harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Checklists/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunChecklistHarvestAndProcessJob()
        {
            try
            {
                BackgroundJob.Enqueue<IChecklistsHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult("Check lists harvest and process job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing checklists harvest and process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}