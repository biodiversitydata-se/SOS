using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Jobs
{
    public class KulHarvestJob : IKulHarvestJob
    {
        private readonly IKulObservationFactory _kulObservationFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<KulHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public KulHarvestJob(IKulObservationFactory kulObservationFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<KulHarvestJob> logger)
        {
            _kulObservationFactory = kulObservationFactory ?? throw new ArgumentNullException(nameof(kulObservationFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogDebug("Start KUL Harvest Job");
            var result = await _kulObservationFactory.HarvestObservationsAsync(cancellationToken);
            _logger.LogDebug($"End KUL Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success);
        }
    }
}