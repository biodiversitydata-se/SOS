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
    public class SersHarvestJob : ISersHarvestJob
    {
        private readonly ISersObservationHarvester _sersObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<SersHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sersObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public SersHarvestJob(ISersObservationHarvester sersObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<SersHarvestJob> logger)
        {
            _sersObservationHarvester = sersObservationHarvester ?? throw new ArgumentNullException(nameof(sersObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start SERS Harvest Job");
            var result = await _sersObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End SERS Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) && result.Count > 0 ? true : throw new Exception("SERS Harvest Job failed");
        }
    }
}