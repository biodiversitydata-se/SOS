﻿using System;
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
        private readonly IFieldMappingDiffHelper _fieldMappingDiffHelper;
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly ILogger<DiagnosticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fieldMappingHarvester"></param>
        /// <param name="fieldMappingDiffHelper"></param>
        /// <param name="areaDiffHelper"></param>
        /// <param name="logger"></param>
        /// <param name="areaHarvester"></param>
        public DiagnosticsController(
            IFieldMappingHarvester fieldMappingHarvester,
            IFieldMappingDiffHelper fieldMappingDiffHelper,
            IAreaHarvester areaHarvester,
            ILogger<DiagnosticsController> logger)
        {
            _fieldMappingHarvester =
                fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _fieldMappingDiffHelper =
                fieldMappingDiffHelper ?? throw new ArgumentNullException(nameof(fieldMappingDiffHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get diff between generated, Json files and processed field mappings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("FieldMappingDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFieldMappingsDiffAsZipFile()
        {
            try
            {
                var fieldMappingFieldIds = Enum.GetValues(typeof(FieldMappingFieldId)).Cast<FieldMappingFieldId>();
                var generatedFieldMappings =
                    await _fieldMappingHarvester.CreateAllFieldMappingsAsync(fieldMappingFieldIds);
                var zipBytes = await _fieldMappingDiffHelper.CreateDiffZipFile(generatedFieldMappings);
                return File(zipBytes, "application/zip", "FieldMappingDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }
    }
}