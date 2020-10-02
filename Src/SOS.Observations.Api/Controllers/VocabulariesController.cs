using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Vocabularies controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class VocabulariesController : ControllerBase
    {
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fieldMappingManager"></param>
        /// <param name="logger"></param>
        public VocabulariesController(
            IFieldMappingManager fieldMappingManager,
            ILogger<ObservationsController> logger)
        {
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all term vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Terms")]
        [ProducesResponseType(typeof(IEnumerable<FieldMapping>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesAsync()
        {
            try
            {
                return new OkObjectResult(await _fieldMappingManager.GetFieldMappingsAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting term vocabularies");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get term vocabulary.
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        [HttpGet("Term/{termId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabularyAsync([FromRoute] FieldMappingFieldId termId)
        {
            try
            {
                var fieldMappings = await _fieldMappingManager.GetFieldMappingsAsync();
                var fieldMapping = fieldMappings.Single(f => f.Id == termId);
                return new OkObjectResult(fieldMapping);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting term vocabulary");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}