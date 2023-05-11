using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Sighting controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase, ISystemsController
    {
        private readonly IDevOpsManager _devOpsManager;
        private readonly IProcessInfoManager _processInfoManager;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<SystemsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="devOpsManager"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SystemsController(
            IDevOpsManager devOpsManager,
            IProcessInfoManager processInfoManager,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<SystemsController> logger)
        {
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _devOpsManager = devOpsManager ?? throw new ArgumentNullException(nameof(devOpsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /*
        /// <inheritdoc />
        [HttpGet("BuildInfo")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBuildInfoAsync()
        {
            try
            {
                var buildInfo = await _devOpsManager.GetBuildInfoAsync();
                return new OkObjectResult(buildInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting copyright");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        */
        /// <inheritdoc />
        [HttpGet("Copyright")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult Copyright()
        {
            try
            {
                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return new OkObjectResult(fileVersionInfo.LegalCopyright);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting copyright");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("ProcessInformation")]
        [ProducesResponseType(typeof(ProcessInfoDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetProcessInfo([FromQuery] bool active)
        {
            try
            {
                return new OkObjectResult(await _processInfoManager.GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting process information");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}