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
        private readonly IVocabularyManager _vocabularyManager;
        private readonly ILogger<VocabulariesController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyManager"></param>
        /// <param name="logger"></param>
        public VocabulariesController(
            IVocabularyManager vocabularyManager,
            ILogger<VocabulariesController> logger)
        {
            _vocabularyManager = vocabularyManager ?? throw new ArgumentNullException(nameof(vocabularyManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all term vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<Vocabulary>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesAsync()
        {
            try
            {
                return new OkObjectResult(await _vocabularyManager.GetVocabulariesAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting vocabularies");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get specific vocabulary.
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        [HttpGet("{termId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabularyAsync([FromRoute] VocabularyId termId)
        {
            try
            {
                var fieldMappings = await _vocabularyManager.GetVocabulariesAsync();
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