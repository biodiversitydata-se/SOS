using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Managers.Interfaces;
using SOS.Administration.Api.Models.Ipt;
using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///    Ipt controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class IptController : ControllerBase, IIptController
    {
        private readonly IIptManager _iptManager;
        private readonly ILogger<IptController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="iptManager"></param>
        /// <param name="logger"></param>
        public IptController(
            IIptManager iptManager,
            ILogger<IptController> logger)
        {
            _iptManager = iptManager ?? throw new ArgumentNullException(nameof(iptManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("Resources")]
        [ProducesResponseType(typeof(IEnumerable<IptResource>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetResourcesAsync()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var resources = await _iptManager.GetResourcesAsync();

                return Ok(resources);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting IPT resources");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

    }
}