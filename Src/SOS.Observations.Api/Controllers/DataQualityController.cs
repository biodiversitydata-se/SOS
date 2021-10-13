using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataQuality;
using SOS.Observations.Api.Controllers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Data quality controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DataQualityController : ControllerBase, IDataQualityController
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

        /// <inheritdoc />
        [HttpGet("report")]
        [ProducesResponseType(typeof(DataQualityReport), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReport()
        {
            try
            {
                return new OkObjectResult(await _dataQualityManager.GetReportAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting data quality report");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}