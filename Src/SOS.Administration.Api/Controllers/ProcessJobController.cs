using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunProcessJob()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dataProvidersToProcess = (await _dataProviderManager.GetAllDataProvidersAsync())
                    .Where(dataProvider => dataProvider.IsActive).ToList();
                if (!dataProvidersToProcess.Any())
                {
                    return new BadRequestObjectResult("No data providers is active.");
                }

                BackgroundJob.Enqueue<IProcessObservationsJobFull>(job => job.RunAsync(JobCancellationToken.Null));
                return new OkObjectResult(
                    $"Process job was enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, dataProvidersToProcess.Select(dataProvider => " -" + dataProvider))}.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxa/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult ScheduleDailyProcessTaxaJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                RecurringJob.AddOrUpdate<IProcessTaxaJob>(nameof(IProcessTaxaJob), job => job.RunAsync(),
                    $"0 {minute} {hour} * * ?", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
                return new OkObjectResult("Process job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding Process job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Taxa/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunProcessTaxaJob()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                BackgroundJob.Enqueue<IProcessTaxaJob>(job => job.RunAsync());
                return new OkObjectResult("Process taxa job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process taxa job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}