using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Statistics controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IObservationManager _observationManager;
        private readonly ILogger<StatisticsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StatisticsController(
            IObservationManager observationManager,            
            ILogger<StatisticsController> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculate observation statistics and create Excel file. This takes some time...
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        [HttpPost("CreateObservationsStatisticsExcel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.RequestTimeout)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetNvStatisticsAsync(
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
                if (toDate - fromDate > TimeSpan.FromDays(1 * 365))
                {
                    return BadRequest("You can max calculate statistics for 1 year");
                }

                var excelFile = await _observationManager.CreateObservationStatisticsSummaryExcelFileAsync(fromDate, toDate);
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SOS observations summary.xlsx");
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