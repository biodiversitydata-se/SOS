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
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class SpeciesPortalHarvestJob : ISpeciesPortalHarvestJob
    {
        private readonly ISpeciesPortalSightingFactory _speciesPortalSightingFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<SpeciesPortalHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalSightingFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalHarvestJob(ISpeciesPortalSightingFactory speciesPortalSightingFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<SpeciesPortalHarvestJob> logger)
        {
            _speciesPortalSightingFactory = speciesPortalSightingFactory ?? throw new ArgumentNullException(nameof(speciesPortalSightingFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Species Portal Harvest Job");
           
            var result = await _speciesPortalSightingFactory.HarvestSightingsAsync(cancellationToken);

            _logger.LogDebug($"End Species Portal Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all imports
            return result.Status.Equals(HarvestStatus.Succeded);
        }
    }
}
