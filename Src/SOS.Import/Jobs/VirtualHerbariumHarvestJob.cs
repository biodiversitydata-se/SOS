using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Managers.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    public class VirtualHerbariumHarvestJob : IVirtualHerbariumHarvestJob
    {
        private readonly IVirtualHerbariumObservationHarvester _virtualHerbariumObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<VirtualHerbariumHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumHarvestJob(
            IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<VirtualHerbariumHarvestJob> logger)
        {
            _virtualHerbariumObservationHarvester = virtualHerbariumObservationHarvester ?? throw new ArgumentNullException(nameof(virtualHerbariumObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start Virtual Herbarium Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataSet.VirtualHerbariumObservations);
            var harvestInfoResult = await _virtualHerbariumObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End Virtual Herbarium Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0 ? true : throw new Exception("Virtual Herbarium Harvest Job failed");
        }
    }
}