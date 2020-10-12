using System;
using System.ComponentModel;
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
    public class FishDataHarvestJob : IFishDataHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IFishDataObservationHarvester _fishDataObservationHarvester;
        private readonly ILogger<FishDataHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fishDataObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public FishDataHarvestJob(
            IFishDataObservationHarvester fishDataObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<FishDataHarvestJob> logger)
        {
            _fishDataObservationHarvester = fishDataObservationHarvester ??
                                       throw new ArgumentNullException(nameof(fishDataObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest fish data")]
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Fish Data Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataProviderType.FishDataObservations);
            var harvestInfoResult = await _fishDataObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End Fish Data Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            if (dataProvider != null)
            {
                await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);
            }

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("Fish Data Harvest Job failed");
        }
        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for Fish data");
        }
    }
}