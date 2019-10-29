using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<bool> Run()
        {
            _logger.LogDebug("Start Species Portal Harvest Job");
            // Create task list
            var harvestTasks = new List<Task<bool>>
            {
                _speciesPortalSightingFactory.HarvestAreasAsync(), // Make sure we have the latest areas
                _speciesPortalSightingFactory.HarvestSightingsAsync()
            };

            // Run all tasks async
            await Task.WhenAll(harvestTasks);
            var success = harvestTasks.All(t => t.Result);

            _logger.LogDebug($"End Species Portal Harvest Job. Success: {success}");

            // return result of all imports
            return success;
        }
    }
}
