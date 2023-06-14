using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    public class DwcArchiveHarvestJob : IDwcArchiveHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IDwcObservationHarvester _dwcObservationHarvester;
        private readonly IDwcChecklistHarvester _dwcChecklistHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<DwcArchiveHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="dwcChecklistHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public DwcArchiveHarvestJob(
            IDwcObservationHarvester dwcObservationHarvester,
            IDwcChecklistHarvester dwcChecklistHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            IDataProviderManager dataProviderManager,
            ILogger<DwcArchiveHarvestJob> logger)
        {
            _dwcObservationHarvester = dwcObservationHarvester ??
                                       throw new ArgumentNullException(nameof(dwcObservationHarvester));
            _dwcChecklistHarvester =
                dwcChecklistHarvester ?? throw new ArgumentNullException(nameof(dwcChecklistHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            int dataProviderId,
            string archivePath,
            DwcaTarget target,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Start DwC-A Harvest Job: {archivePath}");
                var dataProvider = await _dataProviderManager.GetDataProviderByIdAsync(dataProviderId);
                if (dataProvider == null)
                {
                    throw new Exception($"Data provider with Id={dataProviderId} is not found");
                }

                if (dataProvider.Type != DataProviderType.DwcA)
                {
                    throw new Exception($"The data provider \"{dataProvider.Identifier}\" is not a DwC-A provider");
                }

                _logger.LogInformation("Wait for DwC-A file to be ready");
                Task fileReady = FileSystemHelper.IsFileReady(archivePath);
                fileReady.Wait(TimeSpan.FromSeconds(60));
                if (!fileReady.IsCompleted)
                {
                    _logger.LogError($"Couldn't open the file: {archivePath}");
                    return false;
                }
                _logger.LogInformation($"DwC-A file is ready to be opened: {archivePath}");

                if (target.Equals(DwcaTarget.Checklist))
                {
                    var checklistResult = await _dwcChecklistHarvester.HarvestChecklistsAsync(archivePath, dataProvider, cancellationToken);

                    // Save check list harvest info
                    await _harvestInfoRepository.AddOrUpdateAsync(checklistResult);
                    if (checklistResult.Status != RunStatus.Success)
                    {
                        throw new Exception($"Checklist harvest status: {checklistResult.Status}");
                    }

                    try
                    {
                        await _dataProviderManager.SetEmlMetadataAsync(dataProviderId, _dwcChecklistHarvester.GetEmlXmlDocument(archivePath));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error when writing Checklist EML file for {dataProvider}");
                    }
                }
                // Harvest observations
                var harvestInfoResult = await _dwcObservationHarvester.HarvestObservationsAsync(archivePath, dataProvider, cancellationToken);

                try
                {
                    await _dataProviderManager.SetEmlMetadataAsync(dataProviderId, _dwcObservationHarvester.GetEmlXmlDocument(archivePath));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error when writing observation EML file for {dataProvider}");
                }

                _logger.LogInformation($"End DwC-A Harvest Job. Status: {harvestInfoResult.Status}");

                // Save harvest info
                await _harvestInfoRepository.AddOrUpdateAsync(harvestInfoResult);

                return harvestInfoResult.Status.Equals(RunStatus.Success) && harvestInfoResult.Count > 0
                    ? true
                    : throw new Exception("DwC-A Harvest Job failed");
            }
            finally
            {
                if (File.Exists(archivePath)) File.Delete(archivePath);
            }
        }

        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            return await RunAsync(cancellationToken);
        }
    }
}