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
    public class DwcArchiveHarvestJob : IDwcArchiveHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IDwcObservationHarvester _dwcObservationHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<DwcArchiveHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public DwcArchiveHarvestJob(
            IDwcObservationHarvester dwcObservationHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<DwcArchiveHarvestJob> logger)
        {
            _dwcObservationHarvester = dwcObservationHarvester ??
                                       throw new ArgumentNullException(nameof(dwcObservationHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest observations from a DwC-A file")]
        public async Task<bool> RunAsync(
            int dataProviderId,
            string archivePath,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start DwC-A Harvest Job");
                var dataProvider = await _dataProviderManager.GetDataProviderByIdAsync(dataProviderId);
                if (dataProvider == null)
                {
                    throw new Exception($"Data provider with Id={dataProviderId} is not found");
                }

                if (dataProvider.Type != DataProviderType.DwcA)
                {
                    throw new Exception($"The data provider \"{dataProvider}\" is not a DwC-A provider");
                }

                var harvestInfoResult =
                    await _dwcObservationHarvester.HarvestObservationsAsync(archivePath, dataProvider, cancellationToken);
                _logger.LogInformation($"End DwC-A Harvest Job. Status: {harvestInfoResult.Status}");

                // Save harvest info
                await _harvestInfoRepository
                    .AddOrUpdateAsync(
                        harvestInfoResult); // todo - decide whether we should store harvestInfo in two places or not.
                if (dataProvider != null)
                {
                    await _dataProviderManager.UpdateHarvestInfo(dataProvider.Id, harvestInfoResult);
                }
                return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                    ? true
                    : throw new Exception("DwC-A Harvest Job failed");
            }
            finally
            {
                if (System.IO.File.Exists(archivePath)) System.IO.File.Delete(archivePath);
            }
        }

        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            // todo - implement DwC-A harvest from DataProvider.DownloadUrl
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Method not implemented for Darwin core");
        }
    }
}