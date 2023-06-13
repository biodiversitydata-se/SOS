using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Statistics controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        public StatisticsController(
            ILogger<StatisticsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Harvest API statistics from Application Insights and store in MongoDB.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        [HttpPost("HarvestStatistics/Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddDailyApiStatisticsHarvestJob([FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
                RecurringJob.AddOrUpdate<IApiUsageStatisticsHarvestJob>(nameof(IApiUsageStatisticsHarvestJob), job => job.RunHarvestStatisticsAsync(),
                    $"0 {minute} {hour} * * ?", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
                return new OkObjectResult("API statistics harvest job added");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Adding API statistics harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Harvest API statistics from Application Insights and store in MongoDB.
        /// </summary>
        /// <returns></returns>
        [HttpPost("HarvestStatistics/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunApiStatisticsHarvestJob()
        {
            try
            {
                BackgroundJob.Enqueue<IApiUsageStatisticsHarvestJob>(job => job.RunHarvestStatisticsAsync());
                return new OkObjectResult("API statistics harvest job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing API statistics harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create Excel file with API usage statistics.
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateExcelFileReport/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunCreateApiStatisticsExcelFileJob([FromQuery] string createdBy)
        {
            try
            {
                var reportId = Report.CreateReportId();
                BackgroundJob.Enqueue<IApiUsageStatisticsHarvestJob>(job => job.RunCreateExcelFileReportAsync(reportId, createdBy));
                return new OkObjectResult($"Create API usage statistics Excel report job with Id \"{reportId}\" was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing API usage statistics Excel report job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}