using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    public class MvmHarvestJob : IMvmHarvestJob
    {
        private readonly IMvmObservationHarvester _mvmObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<MvmHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mvmObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public MvmHarvestJob(IMvmObservationHarvester mvmObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<MvmHarvestJob> logger)
        {
            _mvmObservationHarvester = mvmObservationHarvester ?? throw new ArgumentNullException(nameof(mvmObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start MVM Harvest Job");
            var result = await _mvmObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End MVM Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) && result.Count > 0 ? true : throw new Exception("MVM Harvest Job failed");
        }
    }
}