using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Field mapping import job.
    /// </summary>
    public class FieldMappingsImportJob : Interfaces.IFieldMappingImportJob
    {
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<FieldMappingsImportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public FieldMappingsImportJob(
            
            IFieldMappingFactory fieldMappingFactory,
            ILogger<FieldMappingsImportJob> logger)
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
            return result;
        }
    }
}