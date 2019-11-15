using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Clam tree portal harvest
    /// </summary>
    public class ClamTreePortalHarvestJob : IClamTreePortalHarvestJob
    {
        private readonly IClamTreePortalObservationFactory _clamTreePortalObservationFactory;
        private readonly ILogger<ClamTreePortalHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamTreePortalObservationFactory"></param>
        /// <param name="logger"></param>
        public ClamTreePortalHarvestJob(IClamTreePortalObservationFactory clamTreePortalObservationFactory, ILogger<ClamTreePortalHarvestJob> logger)
        {
            _clamTreePortalObservationFactory = clamTreePortalObservationFactory ?? throw new ArgumentNullException(nameof(clamTreePortalObservationFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Clam Tree Portal Harvest Job");
            // Create task list
            var harvestTasks = new List<Task<bool>>
            {
                _clamTreePortalObservationFactory.HarvestClamsAsync(),
                _clamTreePortalObservationFactory.HarvestTreesAsync(cancellationToken)
            };

            // Run all tasks async
            await Task.WhenAll(harvestTasks);
            var success = harvestTasks.All(t => t.Result);

            _logger.LogDebug($"End Clam Tree Portal Harvest Job. Success: {success}");

            // return result of all harvests
            return success;
        }
    }
}
