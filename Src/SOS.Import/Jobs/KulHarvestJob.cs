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
    public class KulHarvestJob : IKulHarvestJob
    {
        private readonly IKulObservationHarvester _kulObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<KulHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public KulHarvestJob(
            IKulObservationHarvester kulObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<KulHarvestJob> logger)
        {
            _kulObservationHarvester = kulObservationHarvester ?? throw new ArgumentNullException(nameof(kulObservationHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(IJobCancellationToken  cancellationToken)
        {
            _logger.LogInformation("Start KUL Harvest Job");
            var dataProvider = await _dataProviderManager.GetDataProviderByType(DataSet.KULObservations);
            var harvestInfoResult = await _kulObservationHarvester.HarvestObservationsAsync(cancellationToken);
            _logger.LogInformation($"End KUL Harvest Job. Status: {harvestInfoResult.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);
            await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);

            return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0 ? true : throw new Exception("KUL Harvest Job failed");
        }
    }
}