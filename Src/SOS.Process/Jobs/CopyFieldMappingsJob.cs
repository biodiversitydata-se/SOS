using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class CopyFieldMappingsJob : ICopyFieldMappingsJob
    {
        private readonly IFieldMappingVerbatimRepository _fieldMappingVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _fieldMappingProcessedRepository;
        private readonly ILogger<CopyFieldMappingsJob> _logger;

        public CopyFieldMappingsJob(
            IFieldMappingVerbatimRepository fieldMappingVerbatimRepository,
            IProcessedFieldMappingRepository fieldMappingProcessedRepository,
            ILogger<CopyFieldMappingsJob> logger)
        {
            _fieldMappingVerbatimRepository = fieldMappingVerbatimRepository ?? throw new ArgumentNullException(nameof(fieldMappingVerbatimRepository));
            _fieldMappingProcessedRepository = fieldMappingProcessedRepository ?? throw new ArgumentNullException(nameof(fieldMappingProcessedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var fieldMappings = await _fieldMappingVerbatimRepository.GetFieldMappingsAsync();
            if (!fieldMappings?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get field mappings");
                return false;
            }

            _logger.LogDebug("Start deleting field mappings");
            if (!await _fieldMappingProcessedRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete field mappings data");
                return false;
            }
            _logger.LogDebug("Finish deleting field mappings");

            _logger.LogDebug("Start copy field mappings");
            var success = await _fieldMappingProcessedRepository.AddManyAsync(fieldMappings);
            _logger.LogDebug("Finish copy field mappings");

            return success ? true : throw new Exception("Copy field mappings job failed");
        }
    }
}