using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Hangfire.UI.Controllers
{
    /// <summary>
    /// Field mapping controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class FieldMappingController : ControllerBase, Interfaces.IFieldMappingController
    {
        private readonly ILogger<FieldMappingController> _logger;
        private readonly IFieldMappingFactory _fieldMappingFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public FieldMappingController(
            IFieldMappingFactory fieldMappingFactory,
            ILogger<FieldMappingController> logger)
        {
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("AllFields/Create")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateAllFieldsMappingFilesAsync()
        {
            try
            {
                var fieldMappingFieldIds = Enum.GetValues(typeof(FieldMappingFieldId)).Cast<FieldMappingFieldId>();
                var zipBytes = await _fieldMappingFactory.CreateFieldMappingsZipFileAsync(fieldMappingFieldIds);
                return File(zipBytes, "application/zip", "AllFieldMappings.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Field/Create")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            try
            {
                var fieldMappingFileTuple = await _fieldMappingFactory.CreateFieldMappingFileAsync(fieldMappingFieldId);
                return File(fieldMappingFileTuple.Bytes, "application/json", fieldMappingFileTuple.Filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}