using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Taxon lists harvest job.
    /// </summary>
    public class ArtportalenDatasetMetadataHarvestJob : IArtportalenDatasetMetadataHarvestJob
    {
        private readonly IArtportalenDatasetMetadataHarvester _artportalenDatasetMetadataHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ArtportalenDatasetMetadataHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonListHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenDatasetMetadataHarvestJob(
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ArtportalenDatasetMetadataHarvestJob> logger)
        {
            _artportalenDatasetMetadataHarvester = artportalenDatasetMetadataHarvester ?? throw new ArgumentNullException(nameof(artportalenDatasetMetadataHarvester));            
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start harvest Artportalen dataset metadata job");

            var result = await _artportalenDatasetMetadataHarvester.HarvestDatasetsAsync();

            _logger.LogInformation($"End harvest Artportalen dataset metadata job. Result: {result.Status == RunStatus.Success}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status == RunStatus.Success && result.Count > 0
                ? true
                : throw new Exception("Harvest Artportalen dataset metadata job failed");
        }
    }
}