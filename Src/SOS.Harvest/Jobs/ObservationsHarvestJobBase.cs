using AgileObjects.AgileMapper.Extensions;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Data;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJobBase
    {
        private readonly IProjectHarvester _projectHarvester;
        private readonly IArtportalenDatasetMetadataHarvester _artportalenDatasetMetadataHarvester;
        private readonly ITaxonListHarvester _taxonListHarvester;

        private async Task<bool> HarvestResources(
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", mode.GetLoggerMode()) });
            try
            {
                _logger.LogInformation($"Start {mode} resources harvest jobs");
                if (mode == JobRunModes.Full)
                {
                    await _projectHarvester.HarvestProjectsAsync();
                    await _taxonListHarvester.HarvestTaxonListsAsync();
                    await _artportalenDatasetMetadataHarvester.HarvestDatasetsAsync();
                }

                _logger.LogInformation($"Finish {mode} resources harvest jobs");
                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{mode} resources harvest job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{mode} resources harvest job failed.");
                return false;
            }
        }

        protected readonly IHarvestInfoRepository _harvestInfoRepository;
        protected readonly IDataProviderManager _dataProviderManager;
        protected readonly IObservationHarvesterManager _observationHarvesterManager;
        protected readonly ILogger<ObservationsHarvestJobBase> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationHarvesterManager"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="artportalenDatasetMetadataHarvester"></param>
        /// <param name="taxonListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationsHarvestJobBase(
            IObservationHarvesterManager observationHarvesterManager,
            IProjectHarvester projectHarvester,
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ObservationsHarvestJobBase> logger)
        {
            _observationHarvesterManager = observationHarvesterManager ?? throw new ArgumentNullException(nameof(observationHarvesterManager));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _projectHarvester = projectHarvester ?? throw new ArgumentNullException(nameof(projectHarvester));
            _artportalenDatasetMetadataHarvester = artportalenDatasetMetadataHarvester ?? throw new ArgumentNullException(nameof(artportalenDatasetMetadataHarvester));
            _taxonListHarvester = taxonListHarvester ?? throw new ArgumentNullException(nameof(taxonListHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="dataProviders"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<long> HarvestAsync(
        IEnumerable<DataProvider> dataProviders,
        JobRunModes mode,
        DateTime? fromDate,
        IJobCancellationToken cancellationToken)
        {
            _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", mode.GetLoggerMode()) });
            try
            {
                _logger.LogInformation($"Start {mode} harvest jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that any data provider is added
                //------------------------------------------------------------------------
                if (!dataProviders.Any())
                {
                    _logger.LogError(
                        $"No data providers for {mode} harvest");
                    return 0;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations 
                //------------------------------------------------------------------------
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<HarvestInfo>>();
                _logger.LogDebug($"Start adding harvesters ({mode}).");
                foreach (var dataProvider in dataProviders)
                {
                    var harvester = _observationHarvesterManager.GetHarvester(dataProvider.Type);

                    if (dataProvider.Type == DataProviderType.DwcA)
                    {
                        harvestTaskByDataProvider.Add(dataProvider, harvester.HarvestObservationsAsync(dataProvider, cancellationToken));
                    }
                    else
                    {
                        if (dataProvider.SupportIncrementalHarvest)
                        {
                            harvestTaskByDataProvider.Add(dataProvider, harvester.HarvestObservationsAsync(mode, fromDate, cancellationToken));
                        }
                        else
                        {
                            harvestTaskByDataProvider.Add(dataProvider, harvester.HarvestObservationsAsync(cancellationToken));
                        }
                    }

                    _logger.LogDebug($"Added {dataProvider.Names.Translate("en-GB")} for {mode} harvest");
                }
                _logger.LogDebug($"Finish adding harvesters ({mode}).");

                _logger.LogInformation($"Start {mode} observations harvesting.");

                await Task.WhenAll(harvestTaskByDataProvider.Values);
                await PostHarvestAsync(harvestTaskByDataProvider);

                //---------------------------------------------------------------------------------------------------------
                // 4. Make sure mandatory providers where successful
                //---------------------------------------------------------------------------------------------------------
                var success = harvestTaskByDataProvider.Where(dp => dp.Key.HarvestFailPreventProcessing)
                    .All(r => r.Value.Result.Status == RunStatus.Success);

                _logger.LogInformation($"Finish {mode} observations harvesting. Success: {success}");

                return success ? harvestTaskByDataProvider.Sum(ht => ht.Value.Result.Count) : -1;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{mode} observation harvest job was cancelled.");
                return 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{mode} observation harvest job failed.");
                throw new Exception($"{mode} observation harvest job failed.");
            }
        }



        protected virtual async Task PostHarvestAsync(IDictionary<DataProvider, Task<HarvestInfo>> harvestTaskByDataProvider)
        {
        }

        protected async Task<long> RunAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", mode.GetLoggerMode()) });
            var activeProviders = (await _dataProviderManager.GetAllDataProvidersAsync()).Where(dp =>
                dp.IsActive
            ).ToArray();

            var harvestInfos = await _harvestInfoRepository.GetAllAsync();

            var harvestProviders = activeProviders.Where(p =>
                p.IsReadyToHarvest(harvestInfos.FirstOrDefault(hi =>
                    hi.Id == p.Identifier && hi.Status == RunStatus.Success)?.End
                ) &&
                p.IncludeInScheduledHarvest &&
                (
                    mode.Equals(JobRunModes.Full) ||
                    p.SupportIncrementalHarvest
                )
            );

            _logger.LogInformation($"Start harvest job ({mode})");
            await HarvestResources(mode, cancellationToken);
            var harvestCount = await HarvestAsync(harvestProviders, mode, fromDate, cancellationToken);

            if (harvestCount == -1)
            {
                _logger.LogInformation($"Harvest job ({mode}) failed");
                throw new Exception($"Harvest job ({mode}) failed");
            }

            _logger.LogInformation($"Finish harvest job ({mode})");

            return harvestCount;
        }
    }
}