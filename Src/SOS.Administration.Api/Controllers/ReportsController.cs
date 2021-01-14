using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Reports controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportManager _reportManager;
        private readonly ILogger<ReportsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="reportManager"></param>
        /// <param name="logger"></param>
        public ReportsController(
            IReportManager reportManager,
            ILogger<ReportsController> logger)
        {
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Delete reports and their associated files older than the specified number of days.
        /// </summary>
        /// <param name="nrOfDays">Delete reports and files older than this number of days.</param>
        /// <returns></returns>
        [HttpDelete("DeleteOld")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteOldReportsAndFilesAsync([FromQuery] int nrOfDays = 30)
        {
            try
            {
                if (nrOfDays < 0 || nrOfDays > 10000) return BadRequest($"nrOfDays is not in supported range 0-10000");
                var result = await _reportManager.DeleteOldReportsAndFilesAsync(TimeSpan.FromDays(nrOfDays));
                if (result.IsFailure) return BadRequest(result.Error);
                if (result.Value == 0) return Ok($"No reports found that is older than {nrOfDays} days.");
                return Ok($"Ok. {result.Value} reports deleted.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete a specific report and its associated file.
        /// </summary>
        /// <param name="reportId">The reportId.</param>
        /// <returns></returns>
        [HttpDelete("Delete/{reportId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteReportAndFileAsync([FromRoute] string reportId)
        {
            try
            {
                var report = await _reportManager.GetReportAsync(reportId);
                if (report == null)
                {
                    return NotFound($"reportId \"{reportId}\" not found");
                }

                await _reportManager.DeleteReportAsync(reportId);
                return Ok($"Ok. Report With Id \"{reportId}\" was deleted.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all reports sorted by date.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<Report>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReportsAsync()
        {
            try
            {
                var reports = await _reportManager.GetAllReportsAsync();
                reports = reports.OrderByDescending(m => m.CreatedDate).ToList();
                return new OkObjectResult(reports);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting report files");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get information about a specific report.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [HttpGet("{reportId}")]
        [ProducesResponseType(typeof(Report), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReportAsync([FromRoute] string reportId)
        {
            try
            {
                var report = await _reportManager.GetReportAsync(reportId);
                if (report == null)
                {
                    return NotFound($"reportId \"{reportId}\" not found");
                }
                return new OkObjectResult(report);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting report");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get the file for a specific report.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [HttpGet("{reportId}/File")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFileAsync([FromRoute] string reportId)
        {
            try
            {
                var report = await _reportManager.GetReportAsync(reportId);
                if (report == null) return NotFound($"reportId \"{reportId}\" not found");
                var reportFileResult = await _reportManager.GetReportFileAsync(reportId);
                if (reportFileResult.IsFailure) return BadRequest(reportFileResult.Error);
                var reportFile = reportFileResult.Value;
                return File(reportFile.File, reportFile.ContentType, reportFile.Filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}