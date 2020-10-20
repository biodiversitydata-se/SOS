using System;
using System.ComponentModel;
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
    ///     Artportalen harvest
    /// </summary>
    public class ArtportalenHarvestJob : IArtportalenHarvestJob
    {
        private readonly IArtportalenObservationHarvester _artportalenObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ArtportalenHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenHarvestJob(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ArtportalenHarvestJob> logger)
        {
            _artportalenObservationHarvester = artportalenObservationHarvester ??
                                               throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Make a full harvest of observations from Artportalen")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            return await RunAsync(JobRunModes.Full, cancellationToken);
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from Artportalen")]
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Artportalen Harvest Job");
            var harvestInfoResult = await _artportalenObservationHarvester.HarvestSightingsAsync(mode, cancellationToken);
            _logger.LogInformation($"End Artportalen Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Failed)
                ? throw new Exception("Artportalen Harvest Job failed")
                : harvestInfoResult.Status.Equals(RunStatus.Success);
        }
    }
}