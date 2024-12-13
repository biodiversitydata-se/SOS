using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Swagger;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Statistics controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IApiUsageStatisticsManager _apiUsageStatisticsManager;
        private readonly ILogger<StatisticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="apiUsageStatisticsManager"></param>
        /// <param name="logger"></param>
        public StatisticsController(
            IApiUsageStatisticsManager apiUsageStatisticsManager,
            ILogger<StatisticsController> logger)
        {
            _apiUsageStatisticsManager = apiUsageStatisticsManager ?? throw new ArgumentNullException(nameof(apiUsageStatisticsManager));
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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

        /// <summary>
        /// Calculate request statistics
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        [HttpPost("CreateRequestStatisticsExcel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.RequestTimeout)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetRequestStatisticsAsync(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                if (fromDate >= toDate)
                {
                    return BadRequest("From date must be less than toDate");
                }
                if (toDate - fromDate > TimeSpan.FromDays(5 * 365))
                {
                    return BadRequest("You can max calculate statistics for 5 years");
                }

                var excelFile = await _apiUsageStatisticsManager.CreateRequestStatisticsSummaryExcelFileAsync(fromDate, toDate);
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SOS requests summary.xlsx"); 
            }
            catch (AuthenticationRequiredException e)
            {
                _logger.LogInformation(e, e.Message);
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (TaxonValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (TimeoutException)
            {
                return new StatusCodeResult((int)HttpStatusCode.RequestTimeout);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get statistics");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}