using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Search.Service.Controllers.Interfaces;
using SOS.Search.Service.Factories.Interfaces;

namespace SOS.Search.Service.Controllers
{
    /// <summary>
    /// Sighting controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase, ISystemController
    {
        private readonly IProcessInfoFactory _processInfoFactory;
        private readonly ILogger<SystemController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInfoFactory"></param>
        /// <param name="logger"></param>
        public SystemController(IProcessInfoFactory processInfoFactory, ILogger<SystemController> logger)
        {
            _processInfoFactory = processInfoFactory ?? throw new ArgumentNullException(nameof(processInfoFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("ProcessInformation")]
        [ProducesResponseType(typeof(ProcessInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProcessInfoAsync([FromQuery]bool active)
        {
            try
            {
                return new OkObjectResult(await _processInfoFactory.GetProcessInfoAsync(active));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting process information");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
