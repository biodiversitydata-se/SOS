using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Field mapping import job.
    /// </summary>
    public class FieldMappingImportJob : IFieldMappingImportJob
    {
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly ILogger<FieldMappingImportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingHarvester"></param>
        /// <param name="logger"></param>
        public FieldMappingImportJob(
            IFieldMappingHarvester fieldMappingHarvester,
            ILogger<FieldMappingImportJob> logger)
        {
            _fieldMappingHarvester = fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogDebug("Start Field Mapping Import Job");

            var result = await _fieldMappingHarvester.ImportAsync();

            _logger.LogDebug($"End Field Mapping Import Job. Result: {result}");

            return result ? true : throw new Exception("Field Mapping Import Job failed");
        }
    }
}