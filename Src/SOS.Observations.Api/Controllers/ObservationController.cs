using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Factories.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Observation controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ObservationController : ControllerBase, IObservationController
    {
        private readonly IObservationFactory _observationFactory;
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<ObservationController> _logger;
        private const int _maxBatchSize = 10000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationFactory"></param>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public ObservationController(
            IObservationFactory observationFactory, 
            IFieldMappingFactory fieldMappingFactory,
            ILogger<ObservationController> logger)
        {
            _observationFactory = observationFactory ?? throw new ArgumentNullException(nameof(observationFactory));
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<ProcessedObservation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChunkAsync([FromBody] SearchFilter filter, [FromQuery]int skip, [FromQuery]int take)
        {
            try
            {
                if (!filter.IsFilterActive || skip < 0 || take <= 0 || take > _maxBatchSize)
                {
                    return new BadRequestResult();
                }

                return new OkObjectResult(await _observationFactory.GetChunkAsync(filter, skip, take));
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