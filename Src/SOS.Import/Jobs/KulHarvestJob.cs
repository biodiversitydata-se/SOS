using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.ObservationHarvesters.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    public class KulHarvestJob : IKulHarvestJob
    {
        private readonly IKulObservationHarvester _kulObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<KulHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public KulHarvestJob(IKulObservationHarvester kulObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<KulHarvestJob> logger)
        {
            _kulObservationHarvester = kulObservationHarvester ?? throw new ArgumentNullException(nameof(kulObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogDebug("Start KUL Harvest Job");
            var result = await _kulObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogDebug($"End KUL Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("KUL Harvest Job failed");
        }
    }
}