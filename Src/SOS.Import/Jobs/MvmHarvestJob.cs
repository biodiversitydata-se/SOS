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
    public class MvmHarvestJob : IMvmHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<MvmHarvestJob> _logger;
        private readonly IMvmObservationHarvester _mvmObservationHarvester;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mvmObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public MvmHarvestJob(
            IMvmObservationHarvester mvmObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<MvmHarvestJob> logger)
        {
            _mvmObservationHarvester = mvmObservationHarvester ??
                                       throw new ArgumentNullException(nameof(mvmObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start MVM Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataProviderType.MvmObservations);
            var harvestInfoResult = await _mvmObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End MVM Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                ? true
                : throw new Exception("MVM Harvest Job failed");
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(bool incrementalHarvest, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for MVM");
        }
    }
}