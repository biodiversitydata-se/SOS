using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Clam tree portal harvest
    /// </summary>
    public class ClamPortalHarvestJob : IClamPortalHarvestJob
    {
        private readonly IClamPortalObservationFactory _clamPortalObservationFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ClamPortalHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamPortalObservationFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalHarvestJob(IClamPortalObservationFactory clamPortalObservationFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ClamPortalHarvestJob> logger)
        {
            _clamPortalObservationFactory = clamPortalObservationFactory ?? throw new ArgumentNullException(nameof(clamPortalObservationFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Clam Portal Harvest Job");

            var result = await _clamPortalObservationFactory.HarvestClamsAsync(cancellationToken);
            
            _logger.LogDebug($"End Clam Portal Harvest Job. Status: {result.Status}");
            
            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);
            
            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Clam Portal Harvest Job failed");
        }
    }
}
