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
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly ILogger<VocabulariesController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fieldMappingHarvester"></param>
        /// <param name="logger"></param>
        public VocabulariesController(
            IFieldMappingHarvester fieldMappingHarvester,
            ILogger<VocabulariesController> logger)
        {
            _fieldMappingHarvester =
                fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("All/Create")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAllFieldsMappingFilesAsync()
        {
            try
            {
                var fieldMappingFieldIds = Enum.GetValues(typeof(FieldMappingFieldId)).Cast<FieldMappingFieldId>();
                var zipBytes = await _fieldMappingHarvester.CreateFieldMappingsZipFileAsync(fieldMappingFieldIds);
                return File(zipBytes, "application/zip", "AllFieldMappings.zip");
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
        public async Task<IActionResult> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            try
            {
                var fieldMappingFileTuple =
                    await _fieldMappingHarvester.CreateFieldMappingFileAsync(fieldMappingFieldId);
                return File(fieldMappingFileTuple.Bytes, "application/json", fieldMappingFileTuple.Filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}