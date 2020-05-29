using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class CopyFieldMappingsJob : ProcessJobBase, ICopyFieldMappingsJob
    {
        private readonly IFieldMappingVerbatimRepository _fieldMappingVerbatimRepository;
        private readonly ILogger<CopyFieldMappingsJob> _logger;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        public CopyFieldMappingsJob(
            IFieldMappingVerbatimRepository fieldMappingVerbatimRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<CopyFieldMappingsJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _fieldMappingVerbatimRepository = fieldMappingVerbatimRepository ??
                                              throw new ArgumentNullException(nameof(fieldMappingVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var start = DateTime.Now;

            var fieldMappings = await _fieldMappingVerbatimRepository.GetAllAsync();
            if (!fieldMappings?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get field mappings");
                return false;
            }

            _logger.LogDebug("Start deleting field mappings");
            if (!await _processedFieldMappingRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete field mappings data");
                return false;
            }

            _logger.LogDebug("Finish deleting field mappings");

            _logger.LogDebug("Start copy field mappings");
            var success = await _processedFieldMappingRepository.AddManyAsync(fieldMappings);
            _logger.LogDebug("Finish copy field mappings");

            _logger.LogDebug("Start updating process info for field mappings");
            var harvestInfo =
                await GetHarvestInfoAsync(HarvestInfo.GetIdFromResourceProvider(DataProviderType.FieldMappings));
            var providerInfo = CreateProviderInfo(DataProviderType.FieldMappings, harvestInfo, start, DateTime.Now,
                success ? RunStatus.Success : RunStatus.Failed, fieldMappings.Count);
            await SaveProcessInfo(nameof(FieldMapping), start, fieldMappings.Count,
                success ? RunStatus.Success : RunStatus.Failed, new[] {providerInfo});
            _logger.LogDebug("Finish updating process info for field mappings");

            return success ? true : throw new Exception("Copy field mappings job failed");
        }
    }
}