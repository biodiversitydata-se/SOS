using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Area controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
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

        /// <summary>
        ///     Search for areas (regions).
        /// </summary>
        /// <param name="areaTypes">Filter used to limit number of areas returned</param>
        /// <param name="searchString">Filter used to limit number of areas returned</param>
        /// <param name="skip">Start index of returned areas</param>
        /// <param name="take">Number of areas to return</param>
        /// <returns>List of areas</returns>
        [HttpGet()]
        [ProducesResponseType(typeof(PagedResult<AreaBaseDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreas([FromQuery] IEnumerable<AreaTypeDto> areaTypes = null,
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

        /// <summary>
        /// Get a single area
        /// </summary>
        /// <param name="areaType">The area type.</param>
        /// <param name="featureId">The feature id.</param>
        /// <returns></returns>
        [HttpGet("{areaType}/{featureId}")]
        [ProducesResponseType(typeof(AreaBaseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetArea([FromRoute] AreaTypeDto areaType, [FromRoute] string featureId)
        {
            try
            {
                return new OkObjectResult(await _areaManager.GetAreaAsync(areaType, featureId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting area {areaType} {featureId}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get an area as a zipped JSON file including its polygon.
        /// </summary>
        /// <param name="areaType">The area type.</param>
        /// <param name="featureId">The FeatureId.</param>
        /// <param name="format">Export format.</param>
        /// <returns></returns>
        [HttpGet("{areaType}/{featureId}/Export")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetExport(
            [FromRoute] AreaTypeDto areaType, 
            [FromRoute] string featureId, 
            [FromQuery] AreaExportFormatDto format = AreaExportFormatDto.Json)
        {
            try
            {
                var zipBytes = await _areaManager.GetZippedAreaAsync(areaType, featureId,(AreaExportFormat) format);

                if (zipBytes == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return File(zipBytes, "application/zip", $"Area{areaType:G}:{featureId}.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting areas");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}