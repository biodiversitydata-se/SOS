using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Models.Area;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Area controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AreasController : ControllerBase, IAreasController
    {
        private readonly IAreaManager _areaManager;
        private readonly ILogger<AreasController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaManager"></param>
        /// <param name="logger"></param>
        public AreasController(
            IAreaManager areaManager,
            ILogger<AreasController> logger)
        {
            _areaManager = areaManager ?? throw new ArgumentNullException(nameof(areaManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("")]
        [ProducesResponseType(typeof(PagedResult<ExternalSimpleArea>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreasAsync([FromQuery] IEnumerable<AreaType> areaTypes = null,
            [FromQuery] string searchString = null, [FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                return new OkObjectResult(await _areaManager.GetAreasAsync(areaTypes, searchString, skip, take));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting areas");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{areaId}/export")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> ExportArea(int areaId)
        {
            try
            {
                var zipBytes = await _areaManager.GetZipppedAreaAsync(areaId);

                if (zipBytes == null)
                {
                    return new StatusCodeResult((int) HttpStatusCode.NoContent);
                }

                return File(zipBytes, "application/zip", $"Area{areaId}.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting areas");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}