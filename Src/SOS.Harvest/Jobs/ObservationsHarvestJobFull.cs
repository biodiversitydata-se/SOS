using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJobFull : ObservationsHarvestJobBase, IObservationsHarvestJobFull
    {
        private readonly IProcessObservationsJobFull _processObservationsJobFull;

        protected override async Task PostHarvestAsync(IDictionary<DataProvider, Task<HarvestInfo>> harvestTaskByDataProvider)
        {
            foreach (var task in harvestTaskByDataProvider)
            {
                var provider = task.Key;
                var harvestInfo = task.Value.Result;

                // Some properties can be updated for DwcA providers, update provider on success
                if (harvestInfo.Status == RunStatus.Success && provider.Type == DataProviderType.DwcA)
                {
                    await _dataProviderManager.UpdateDataProvider(provider.Id, provider);
                }

                if (harvestInfo.Status != RunStatus.CanceledSuccess)
                {
                    harvestInfo.Id = provider.Identifier;
                    await _harvestInfoRepository.AddOrUpdateAsync(harvestInfo);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationHarvesterManager"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="artportalenDatasetMetadataHarvester"></param>
        /// <param name="taxonListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processObservationsJobFull"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationsHarvestJobFull(
            IObservationHarvesterManager observationHarvesterManager,
            IProjectHarvester projectHarvester,
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessObservationsJobFull processObservationsJobFull,
            ILogger<ObservationsHarvestJobFull> logger) : base(observationHarvesterManager,
                projectHarvester, artportalenDatasetMetadataHarvester, taxonListHarvester, dataProviderManager, harvestInfoRepository, logger)
        {
            _processObservationsJobFull = processObservationsJobFull ?? throw new ArgumentNullException(nameof(processObservationsJobFull));
        }

        /// <inheritdoc />
        public async Task<bool> RunFullAsync(IJobCancellationToken cancellationToken)
        {
            var harvestCount = await RunAsync(JobRunModes.Full, null, cancellationToken);

            // Always process incremental active instance in order to delete removed observations 
            if (harvestCount == 0)
            {
                return true;
            }

            // If harvest was successful, go on with processing
            return await _processObservationsJobFull.RunAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }

            var harvestDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
            var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
            if (harvestDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {harvestDataProvidersResult.Error}");
                return false;
            }

            return (await HarvestAsync(
                harvestDataProviders.Select(d => d.Value).ToList(),
                JobRunModes.Full,
                null,
                cancellationToken)) != -1;
        }

        public async Task<bool> RunFulliNaturalistHarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var dataProvider =
                await _dataProviderManager.GetDataProviderByIdOrIdentifier("iNaturalist");
            return (await HarvestCompleteWithDelayAsync(
                dataProvider,                
                null,
                cancellationToken)) != -1;
        }
    }
}