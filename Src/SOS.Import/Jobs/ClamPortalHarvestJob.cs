using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Clam tree portal harvest
    /// </summary>
    public class ClamPortalHarvestJob : IClamPortalHarvestJob
    {
        private readonly IClamPortalObservationHarvester _clamPortalObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ClamPortalHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="clamPortalObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalHarvestJob(
            IClamPortalObservationHarvester clamPortalObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ClamPortalHarvestJob> logger)
        {
            _clamPortalObservationHarvester = clamPortalObservationHarvester ??
                                              throw new ArgumentNullException(nameof(clamPortalObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest all observations from Clam portal")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Clam Portal Harvest Job");
            var harvestInfoResult = await _clamPortalObservationHarvester.HarvestClamsAsync(cancellationToken);
            _logger.LogInformation($"End Clam Portal Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

            // return result of all harvests
            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("Clam Portal Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for Clam Portal");
        }
    }
}