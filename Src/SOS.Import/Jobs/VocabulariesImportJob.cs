using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Vocabularies import job.
    /// </summary>
    public class VocabulariesImportJob : IVocabulariesImportJob
    {
        private readonly IVocabularyHarvester _vocabularyHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<VocabulariesImportJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public VocabulariesImportJob(
            IVocabularyHarvester vocabularyHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<VocabulariesImportJob> logger)
        {
            _vocabularyHarvester =
                vocabularyHarvester ?? throw new ArgumentNullException(nameof(vocabularyHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest vocabularies from files")]
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start Vocabularies Import Job");

            var result = await _vocabularyHarvester.HarvestAsync();

            _logger.LogInformation($"End Vocabularies Import Job. Result: {result.Status == RunStatus.Success}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status == RunStatus.Success && result.Count > 0
                ? true
                : throw new Exception("Vocabularies Import Job failed");
        }
    }
}