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
    public class VirtualHerbariumHarvestJob : IVirtualHerbariumHarvestJob
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<VirtualHerbariumHarvestJob> _logger;
        private readonly IVirtualHerbariumObservationHarvester _virtualHerbariumObservationHarvester;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumHarvestJob(
            IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<VirtualHerbariumHarvestJob> logger)
        {
            _virtualHerbariumObservationHarvester = virtualHerbariumObservationHarvester ??
                                                    throw new ArgumentNullException(
                                                        nameof(virtualHerbariumObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from Virtual Herbarium")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Virtual Herbarium Harvest Job");
            var harvestInfoResult =
                await _virtualHerbariumObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End Virtual Herbarium Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("Virtual Herbarium Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for Vitual herbarium");
        }
    }
}