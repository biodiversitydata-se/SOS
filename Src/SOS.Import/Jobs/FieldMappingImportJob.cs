using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Field mapping import job.
    /// </summary>
    public class FieldMappingImportJob : IFieldMappingImportJob
    {
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<FieldMappingImportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public FieldMappingImportJob(
            IFieldMappingFactory fieldMappingFactory,
            ILogger<FieldMappingImportJob> logger)
        {
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogDebug("Start Field Mapping Import Job");

            var result = await _fieldMappingFactory.ImportAsync();

            _logger.LogDebug($"End Field Mapping Import Job. Result: {result}");

            return result ? true : throw new Exception("Field Mapping Import Job failed");
        }
    }
}