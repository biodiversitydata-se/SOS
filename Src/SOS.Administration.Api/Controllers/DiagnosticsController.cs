using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    /// Diagnostics controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase, Interfaces.IDiagnosticsController
    {
        private readonly ILogger<DiagnosticsController> _logger;
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly IFieldMappingDiffHelper _fieldMappingDiffHelper;
        private readonly IAreaDiffHelper _areaDiffHelper;
        private readonly IAreaHarvester _areaHarvester;

        /// <summary>
        /// Constructor
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
            IAreaDiffHelper areaDiffHelper,
            ILogger<DiagnosticsController> logger)
        {
            _fieldMappingHarvester = fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _fieldMappingDiffHelper = fieldMappingDiffHelper ?? throw new ArgumentNullException(nameof(fieldMappingDiffHelper));
            _areaHarvester = areaHarvester ?? throw new ArgumentNullException(nameof(areaHarvester));
            _areaDiffHelper = areaDiffHelper ?? throw new ArgumentNullException(nameof(areaDiffHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get diff between generated, verbatim and processed field mappings.
        /// </summary>
        /// <returns></returns>
        [HttpGet("FieldMappingDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetFieldMappingsDiffAsZipFile()
        {
            try
            {
                var fieldMappingFieldIds = Enum.GetValues(typeof(FieldMappingFieldId)).Cast<FieldMappingFieldId>();
                IEnumerable<FieldMapping> generatedFieldMappings = await _fieldMappingHarvester.CreateAllFieldMappingsAsync(fieldMappingFieldIds);
                var zipBytes = await _fieldMappingDiffHelper.CreateDiffZipFile(generatedFieldMappings);
                return File(zipBytes, "application/zip", "FieldMappingDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get diff between generated, verbatim and processed areas.
        /// </summary>
        /// <returns></returns>
        [HttpGet("AreaDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreasDiffAsZipFile()
        {
            try
            {
                var generatedAreas = await _areaHarvester.GetAreasAsync();
                var zipBytes = await _areaDiffHelper.CreateDiffZipFile(generatedAreas.ToArray());
                return File(zipBytes, "application/zip", "AreaDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod().Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}