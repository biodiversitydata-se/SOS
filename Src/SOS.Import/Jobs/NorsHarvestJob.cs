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
    public class NorsHarvestJob : INorsHarvestJob
    {
        private readonly INorsObservationHarvester _norsObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<NorsHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="norsObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public NorsHarvestJob(INorsObservationHarvester norsObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<NorsHarvestJob> logger)
        {
            _norsObservationHarvester = norsObservationHarvester ?? throw new ArgumentNullException(nameof(norsObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start NORS Harvest Job");
            var result = await _norsObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End NORS Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) && result.Count > 0 ? true : throw new Exception("NORS Harvest Job failed");
        }
    }
}