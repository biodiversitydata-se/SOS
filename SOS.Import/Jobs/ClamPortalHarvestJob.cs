using System;
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
    public class ClamPortalHarvestJob : IClamPortalHarvestJob
    {
        private readonly IClamPortalObservationFactory _clamPortalObservationFactory;
        private readonly ILogger<ClamPortalHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamPortalObservationFactory"></param>
        /// <param name="logger"></param>
        public ClamPortalHarvestJob(IClamPortalObservationFactory clamPortalObservationFactory, ILogger<ClamPortalHarvestJob> logger)
        {
            _clamPortalObservationFactory = clamPortalObservationFactory ?? throw new ArgumentNullException(nameof(clamPortalObservationFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Clam Portal Harvest Job");

            var success = await _clamPortalObservationFactory.HarvestClamsAsync(cancellationToken);
            
            _logger.LogDebug($"End Clam Portal Harvest Job. Success: {success}");

            // return result of all harvests
            return success;
        }
    }
}
