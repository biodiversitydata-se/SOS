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
    public class SharkHarvestJob : ISharkHarvestJob
    {
        private readonly ISharkObservationHarvester _sharkObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<SharkHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public SharkHarvestJob(ISharkObservationHarvester sharkObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<SharkHarvestJob> logger)
        {
            _sharkObservationHarvester = sharkObservationHarvester ?? throw new ArgumentNullException(nameof(sharkObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start SHARK Harvest Job");
            var result = await _sharkObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End SHARK Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) && result.Count > 0 ? true : throw new Exception("SHARK Harvest Job failed");
        }
    }
}