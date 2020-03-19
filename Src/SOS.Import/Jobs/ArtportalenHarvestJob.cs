using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
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
        private readonly ILogger<ArtportalenHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenHarvestJob(IArtportalenObservationHarvester artportalenObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ArtportalenHarvestJob> logger)
        {
            _artportalenObservationHarvester = artportalenObservationHarvester ?? throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Artportalen Harvest Job");
           
            var result = await _artportalenObservationHarvester.HarvestSightingsAsync(cancellationToken);

            _logger.LogDebug($"End Artportalen Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all imports
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Artportalen Harvest Job failed");
        }
    }
}
