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
    ///     Field mapping controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FieldMappingController : ControllerBase, IFieldMappingController
    {
        private readonly IFieldMappingDiffHelper _fieldMappingDiffHelper;
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly ILogger<FieldMappingController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fieldMappingHarvester"></param>
        /// <param name="fieldMappingDiffHelper"></param>
        /// <param name="logger"></param>
        public FieldMappingController(
            IFieldMappingHarvester fieldMappingHarvester,
            IFieldMappingDiffHelper fieldMappingDiffHelper,
            ILogger<FieldMappingController> logger)
        {
            _fieldMappingHarvester =
                fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _fieldMappingDiffHelper =
                fieldMappingDiffHelper ?? throw new ArgumentNullException(nameof(fieldMappingDiffHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("AllFields/Create")]
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
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("SingleField/Create")]
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
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}