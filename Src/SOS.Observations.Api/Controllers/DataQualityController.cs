using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataQuality;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Data quality controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DataQualityController : ControllerBase
    {
        private readonly IDataQualityManager _dataQualityManager;
        private readonly ILogger<DataQualityController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataQualityManager"></param>
        /// <param name="logger"></param>
        public DataQualityController(
            IDataQualityManager dataQualityManager,
            ILogger<DataQualityController> logger)
        {
            _dataQualityManager = dataQualityManager ?? throw new ArgumentNullException(nameof(dataQualityManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Data quality report.
        /// </summary>
        /// <param name="organismGroup">OrganismgGroup filter.</param>
        /// <returns>List of observations that can be duplicates</returns>
        [HttpGet("report/{organismGroup}")]
        [ProducesResponseType(typeof(DataQualityReport), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReport([FromRoute]string organismGroup)
        {
            try
            {
                return new OkObjectResult(await _dataQualityManager.GetReportAsync(organismGroup));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data quality report");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}