using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class SpeciesPortalHarvestJob : ISpeciesPortalHarvestJob
    {
        private readonly ISpeciesPortalSightingFactory _speciesPortalSightingFactory;
        private readonly ILogger<SpeciesPortalHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalSightingFactory"></param>
        public SpeciesPortalHarvestJob(ISpeciesPortalSightingFactory speciesPortalSightingFactory, ILogger<SpeciesPortalHarvestJob> logger)
        {
            _speciesPortalSightingFactory = speciesPortalSightingFactory ?? throw new ArgumentNullException(nameof(speciesPortalSightingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Species Portal Harvest Job");
           
            var success = await _speciesPortalSightingFactory.HarvestSightingsAsync(cancellationToken);

            _logger.LogDebug($"End Species Portal Harvest Job. Success: {success}");

            // return result of all imports
            return success;
        }
    }
}
