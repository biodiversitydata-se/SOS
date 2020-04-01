using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Field mapping import job.
    /// </summary>
    public class FieldMappingImportJob : IFieldMappingImportJob
    {
        private readonly IFieldMappingHarvester _fieldMappingHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<FieldMappingImportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingHarvester"></param>
        /// <param name="logger"></param>
        public FieldMappingImportJob(
            IFieldMappingHarvester fieldMappingHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<FieldMappingImportJob> logger)
        {
            _fieldMappingHarvester = fieldMappingHarvester ?? throw new ArgumentNullException(nameof(fieldMappingHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start Field Mapping Import Job");

            var result = await _fieldMappingHarvester.HarvestAsync();

            _logger.LogInformation($"End Field Mapping Import Job. Result: {result.Status == RunStatus.Success}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status == RunStatus.Success ? true : throw new Exception("Field Mapping Import Job failed");
        }
    }
}