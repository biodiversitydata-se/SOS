using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Process.Helpers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Diagnostics controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase, IDiagnosticsController
    {
        private readonly IVocabulariesDiffHelper _vocabulariesDiffHelper;
        private readonly IVocabularyHarvester _vocabularyHarvester;
        private readonly ILogger<DiagnosticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="vocabulariesDiffHelper"></param>
        /// <param name="logger"></param>
        /// <param name="areaHarvester"></param>
        public DiagnosticsController(
            IVocabularyHarvester vocabularyHarvester,
            IVocabulariesDiffHelper vocabulariesDiffHelper,
            IAreaHarvester areaHarvester,
            ILogger<DiagnosticsController> logger)
        {
            _vocabularyHarvester =
                vocabularyHarvester ?? throw new ArgumentNullException(nameof(vocabularyHarvester));
            _vocabulariesDiffHelper =
                vocabulariesDiffHelper ?? throw new ArgumentNullException(nameof(vocabulariesDiffHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get diff between generated, Json files and processed vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("VocabularyDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesDiffAsZipFile()
        {
            try
            {
                var vocabularyIds = Enum.GetValues(typeof(VocabularyId)).Cast<VocabularyId>();
                var generatedVocabularies =
                    await _vocabularyHarvester.CreateAllVocabulariesAsync(vocabularyIds);
                var zipBytes = await _vocabulariesDiffHelper.CreateDiffZipFile(generatedVocabularies);
                return File(zipBytes, "application/zip", "VocabularyDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}