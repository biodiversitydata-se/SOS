using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Managers.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ArtportalenHarvestJob : IArtportalenHarvestJob
    {
        private readonly IArtportalenObservationHarvester _artportalenObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<ArtportalenHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public ArtportalenHarvestJob(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<ArtportalenHarvestJob> logger)
        {
            _artportalenObservationHarvester = artportalenObservationHarvester ?? throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Artportalen Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataProviderType.ArtportalenObservations);
            var harvestInfoResult = await _artportalenObservationHarvester.HarvestSightingsAsync(cancellationToken);
            _logger.LogInformation($"End Artportalen Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);

            // return result of all imports
            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0 ? true : throw new Exception("Artportalen Harvest Job failed");
        }
    }
}
