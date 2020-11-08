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

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Term dictionary controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class VocabulariesController : ControllerBase, IVocabulariesController
    {
        private readonly IVocabularyHarvester _vocabularyHarvester;
        private readonly ILogger<VocabulariesController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="logger"></param>
        public VocabulariesController(
            IVocabularyHarvester vocabularyHarvester,
            ILogger<VocabulariesController> logger)
        {
            _vocabularyHarvester =
                vocabularyHarvester ?? throw new ArgumentNullException(nameof(vocabularyHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("All/Create")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAllVocabulariesFilesAsync()
        {
            try
            {
                var vocabularyIds = Enum.GetValues(typeof(VocabularyId)).Cast<VocabularyId>();
                var zipBytes = await _vocabularyHarvester.CreateVocabulariesZipFileAsync(vocabularyIds);
                return File(zipBytes, "application/zip", "AllVocabularies.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Single/Create")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateSingleVocabularyFileAsync(VocabularyId vocabularyId)
        {
            try
            {
                var vocabularyFileTuple =
                    await _vocabularyHarvester.CreateVocabularyFileAsync(vocabularyId);
                return File(vocabularyFileTuple.Bytes, "application/json", vocabularyFileTuple.Filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}