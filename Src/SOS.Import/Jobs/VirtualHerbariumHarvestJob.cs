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
    public class VirtualHerbariumHarvestJob : IVirtualHerbariumHarvestJob
    {
        private readonly IVirtualHerbariumObservationHarvester _virtualHerbariumObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<VirtualHerbariumHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumHarvestJob(IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<VirtualHerbariumHarvestJob> logger)
        {
            _virtualHerbariumObservationHarvester = virtualHerbariumObservationHarvester ?? throw new ArgumentNullException(nameof(virtualHerbariumObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start Virtual Herbarium Harvest Job");
            var result = await _virtualHerbariumObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End Virtual Herbarium Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status.Equals(RunStatus.Success) && result.Count > 0 ? true : throw new Exception("Virtual Herbarium Harvest Job failed");
        }
    }
}