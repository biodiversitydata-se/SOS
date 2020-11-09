using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Process job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProcessJobController : ControllerBase, IProcessJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<ProcessJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public ProcessJobController(
            IDataProviderManager dataProviderManager,
            ILogger<ProcessJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <inheritdoc />
        [HttpPost("Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunProcessJob(
            [FromQuery] bool cleanStart = true,
            [FromQuery] bool copyFromActiveOnFail = false)
        {
            try
            {
                var dataProvidersToProcess = (await _dataProviderManager.GetAllDataProvidersAsync())
                    .Where(dataProvider => dataProvider.IsActive).ToList();
                if (!dataProvidersToProcess.Any())
                {
                    return new BadRequestObjectResult("No data providers is active.");
                }

                BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(cleanStart, copyFromActiveOnFail,
                    JobCancellationToken.Null));
                return new OkObjectResult(
                    $"Process job was enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, dataProvidersToProcess.Select(dataProvider => " -" + dataProvider))}.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("SelectedDataProviders/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunProcessJob(
            [FromQuery] List<string> dataProviderIdOrIdentifiers,
            [FromQuery] JobRunModes mode = JobRunModes.Full)
        {
            try
            {
                if (dataProviderIdOrIdentifiers == null || dataProviderIdOrIdentifiers.Count == 0)
                {
                    return new BadRequestObjectResult("dataProviderIdOrIdentifiers is not set");
                }

                var dataProvidersToProcess =
                    await _dataProviderManager.GetDataProvidersByIdOrIdentifier(dataProviderIdOrIdentifiers);
                var result = Result.Combine(dataProvidersToProcess);
                if (result.IsFailure)
                {
                    return new BadRequestObjectResult(result.Error);
                }

                BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(
                    dataProviderIdOrIdentifiers, 
                    mode,
                    JobCancellationToken.Null));
                return new OkObjectResult(
                    $"Process job was enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, dataProvidersToProcess.Select(res => " - " + res.Value))}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        ///// <inheritdoc />
        //[HttpPost("Daily")]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //public IActionResult ScheduleDailyProcessJob([FromQuery]int sources, [FromQuery]bool cleanStart = true, [FromQuery]bool copyFromActiveOnFail = false, [FromQuery]bool toggleInstanceOnSuccess = true, [FromQuery]int hour = 0, [FromQuery]int minute = 0)
        //{
        //    try
        //    {
        //        RecurringJob.AddOrUpdate<IProcessJob>(nameof(IProcessJob), job => job.RunAsync(sources, cleanStart, copyFromActiveOnFail, toggleInstanceOnSuccess, JobCancellationToken.Null), $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
        //        return new OkObjectResult("Process job added");
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "Adding Process job failed");
        //        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        //    }
        //}

        ///// <inheritdoc />
        //[HttpPost("Run")]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //public IActionResult RunProcessJob([FromQuery]int sources, [FromQuery]bool cleanStart = true, [FromQuery]bool copyFromActiveOnFail = false, [FromQuery]bool toggleInstanceOnSuccess = true)
        //{
        //    try
        //    {
        //        BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(sources, cleanStart, copyFromActiveOnFail, toggleInstanceOnSuccess, JobCancellationToken.Null));
        //        return new OkObjectResult("Started process job");
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "Starting process job failed");
        //        return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        //    }
        //}

        /// <inheritdoc />
        [HttpPost("Taxa/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyProcessTaxaJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IProcessTaxaJob>(nameof(IProcessTaxaJob), job => job.RunAsync(),
                    $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);
                return new OkObjectResult("Process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxa/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public IActionResult RunProcessTaxaJob()
        {
            try
            {
                BackgroundJob.Enqueue<IProcessTaxaJob>(job => job.RunAsync());
                return new OkObjectResult("Process taxa job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process taxa job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}