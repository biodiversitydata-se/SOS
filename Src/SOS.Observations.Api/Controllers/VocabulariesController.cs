using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.Extensions;
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
        /// Get all Artportalen projects.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Projects")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _vocabularyManager.GetProjectsAsync();
                var dtos = projects.ToProjectDtos();
                return new OkObjectResult(dtos);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<VocabularyDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabularies()
        {
            try
            {
                var vocabularies = await _vocabularyManager.GetVocabulariesAsync();
                var dtos = vocabularies.ToVocabularyDtos();
                return new OkObjectResult(dtos);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting vocabularies");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all vocabularies as zip file.
        /// </summary>
        /// <returns></returns>
        [HttpGet("ZipFile")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesAsZipFile()
        {
            try
            {
                var vocabularyIds = Enum.GetValues(typeof(VocabularyId)).Cast<VocabularyId>();
                var zipBytes = await _vocabularyManager.GetVocabulariesZipFileAsync(vocabularyIds);
                return File(zipBytes, "application/zip", "Vocabularies.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a specific vocabulary.
        /// </summary>
        /// <param name="vocabularyId"></param>
        /// <returns></returns>
        [HttpGet("{vocabularyId}")]
        [ProducesResponseType(typeof(VocabularyDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabularyById(
            [FromRoute] VocabularyIdDto vocabularyId)
        {
            try
            {
                var vocabularies = await _vocabularyManager.GetVocabulariesAsync();
                var vocabulary = vocabularies.Single(f => f.Id == (VocabularyId)vocabularyId);
                var dto = vocabulary.ToVocabularyDto();
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting vocabulary");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}