using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Search.Service.Controllers.Interfaces;
using SOS.Search.Service.Factories.Interfaces;

namespace SOS.Search.Service.Controllers
{
    /// <summary>
    /// Sighting controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SightingController : ControllerBase, ISightingController
    {
        private readonly ISightingFactory _sightingFactory;
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<SightingController> _logger;
        private const int _maxBatchSize = 10000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public SightingController(
            ISightingFactory sightingFactory, 
            IFieldMappingFactory fieldMappingFactory,
            ILogger<SightingController> logger)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<ProcessedSighting>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChunkAsync([FromBody] AdvancedFilter filter, [FromQuery]int skip, [FromQuery]int take)
        {
            try
            {
                if (!filter.IsFilterActive || skip < 0 || take <= 0 || take > _maxBatchSize)
                {
                    return new BadRequestResult();
                }

                return new OkObjectResult(await _sightingFactory.GetChunkAsync(filter, skip, take));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of sightings");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("FieldMapping")]
        [ProducesResponseType(typeof(IEnumerable<FieldMapping>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFieldMappingAsync()
        {
            try
            {
                return new OkObjectResult(await _fieldMappingFactory.GetFieldMappingsAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting field mappings");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}