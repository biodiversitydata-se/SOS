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
    public class SharkHarvestJob : ISharkHarvestJob
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<SharkHarvestJob> _logger;
        private readonly ISharkObservationHarvester _sharkObservationHarvester;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public SharkHarvestJob(
            ISharkObservationHarvester sharkObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<SharkHarvestJob> logger)
        {
            _sharkObservationHarvester = sharkObservationHarvester ??
                                         throw new ArgumentNullException(nameof(sharkObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from SHARK")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start SHARK Harvest Job");
            var harvestInfoResult = await _sharkObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End SHARK Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("SHARK Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for Shark");
        }
    }
}