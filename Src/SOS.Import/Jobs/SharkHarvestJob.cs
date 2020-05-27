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
    public class SharkHarvestJob : ISharkHarvestJob
    {
        private readonly ISharkObservationHarvester _sharkObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<SharkHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public SharkHarvestJob(
            ISharkObservationHarvester sharkObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<SharkHarvestJob> logger)
        {
            _sharkObservationHarvester = sharkObservationHarvester ?? throw new ArgumentNullException(nameof(sharkObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start SHARK Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataProviderType.SharkObservations);
            var harvestInfoResult = await _sharkObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End SHARK Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0 ? true : throw new Exception("SHARK Harvest Job failed");
        }
    }
}