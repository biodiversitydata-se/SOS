using System;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Instance job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class InstanceJobController : ControllerBase, Interfaces.IInstanceJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<InstanceJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public InstanceJobController(
            IDataProviderManager dataProviderManager,
            ILogger<InstanceJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Copy")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCopyDataProviderData([FromQuery]string dataProviderIdOrIdentifier)
        {
            try
            {
                var dataProvider = await _dataProviderManager.GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult($"No data provider exist with Id or Identifier={dataProviderIdOrIdentifier}");
                }

                BackgroundJob.Enqueue<ICopyProviderDataJob>(job => job.RunAsync(dataProvider.Id));
                return new OkObjectResult($"Copy provider data job for {dataProvider} was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing copy provider data failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Activate")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunSetActivateInstanceJob([FromQuery]byte instance)
        {
            try
            {
                if (instance < 0 || instance > 1)
                {
                    _logger.LogError( "Instance must be 0 or 1");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                BackgroundJob.Enqueue<IActivateInstanceJob>(job => job.RunAsync(instance));
                return new OkObjectResult("Activate instance job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Activate instance failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
