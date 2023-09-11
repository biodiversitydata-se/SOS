using AgileObjects.AgileMapper.Extensions;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.FishData.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Kul.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.Biologg.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Harvesters.ObservationDatabase.Interfaces;
using SOS.Harvest.Harvesters.Shark.Interfaces;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Harvest.HarvestersAquaSupport.Sers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJobBase 
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IProjectHarvester _projectHarvester;
        private readonly IArtportalenDatasetMetadataHarvester _artportalenDatasetMetadataHarvester;
        private readonly ITaxonListHarvester _taxonListHarvester;

        protected readonly IDataProviderManager _dataProviderManager;
        protected readonly IDictionary<DataProviderType, IObservationHarvester> _harvestersByType;
        protected readonly ILogger<ObservationsHarvestJobBase> _logger;

        /// <summary>
        /// Run harvest and start processing on success if requested
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="fromDate"></param>
        /// <param name="harvestProviders"></param>
        /// <param name="processProviders"></param>
        /// <param name="processOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<long> RunAsync(JobRunModes mode,
            DateTime? fromDate,
            IEnumerable<DataProvider> harvestProviders,
            IEnumerable<DataProvider> processProviders,
            bool processOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", mode.GetLoggerMode()) });
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="biologgObservationHarvester"></param>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="fishDataObservationHarvester"></param>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="mvmObservationHarvester"></param>
        /// <param name="norsObservationHarvester"></param>
        /// <param name="observationDatabaseHarvester"></param>
        /// <param name="sersObservationHarvester"></param>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="iNaturalistObservationHarvester"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="taxonListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        protected ObservationsHarvestJobBase(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IBiologgObservationHarvester biologgObservationHarvester,
            IDwcObservationHarvester dwcObservationHarvester,
            IFishDataObservationHarvester fishDataObservationHarvester,
            IKulObservationHarvester kulObservationHarvester,
            IMvmObservationHarvester mvmObservationHarvester,
            INorsObservationHarvester norsObservationHarvester,
            IObservationDatabaseHarvester observationDatabaseHarvester,
            ISersObservationHarvester sersObservationHarvester,
            ISharkObservationHarvester sharkObservationHarvester,
            IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IiNaturalistObservationHarvester iNaturalistObservationHarvester,
            IProjectHarvester projectHarvester,
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ObservationsHarvestJobBase> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _projectHarvester = projectHarvester ?? throw new ArgumentNullException(nameof(projectHarvester));
            _artportalenDatasetMetadataHarvester = artportalenDatasetMetadataHarvester ?? throw new ArgumentNullException(nameof(artportalenDatasetMetadataHarvester));
            _taxonListHarvester = taxonListHarvester ?? throw new ArgumentNullException(nameof(taxonListHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (artportalenObservationHarvester == null) throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            if (biologgObservationHarvester == null) throw new ArgumentNullException(nameof(biologgObservationHarvester));
            if (dwcObservationHarvester == null) throw new ArgumentNullException(nameof(dwcObservationHarvester));
            if (fishDataObservationHarvester == null) throw new ArgumentNullException(nameof(fishDataObservationHarvester));
            if (kulObservationHarvester == null) throw new ArgumentNullException(nameof(kulObservationHarvester));
            if (mvmObservationHarvester == null) throw new ArgumentNullException(nameof(mvmObservationHarvester));
            if (norsObservationHarvester == null) throw new ArgumentNullException(nameof(norsObservationHarvester));
            if (observationDatabaseHarvester == null) throw new ArgumentNullException(nameof(observationDatabaseHarvester));
            if (sersObservationHarvester == null) throw new ArgumentNullException(nameof(sersObservationHarvester));
            if (sharkObservationHarvester == null) throw new ArgumentNullException(nameof(sharkObservationHarvester));
            if (virtualHerbariumObservationHarvester == null) throw new ArgumentNullException(nameof(virtualHerbariumObservationHarvester));
            if (iNaturalistObservationHarvester == null) throw new ArgumentNullException(nameof(iNaturalistObservationHarvester));


            _harvestersByType = new Dictionary<DataProviderType, IObservationHarvester>
            {
                {DataProviderType.ArtportalenObservations, artportalenObservationHarvester},
                {DataProviderType.BiologgObservations, biologgObservationHarvester},
                {DataProviderType.DwcA, dwcObservationHarvester},
                {DataProviderType.FishDataObservations, fishDataObservationHarvester},
                {DataProviderType.KULObservations, kulObservationHarvester},
                {DataProviderType.MvmObservations, mvmObservationHarvester},
                {DataProviderType.NorsObservations, norsObservationHarvester},
                {DataProviderType.ObservationDatabase, observationDatabaseHarvester},
                {DataProviderType.SersObservations, sersObservationHarvester},
                {DataProviderType.SharkObservations, sharkObservationHarvester},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationHarvester},
                {DataProviderType.iNaturalistObservations, iNaturalistObservationHarvester}
            };
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
                    var harvestJob = _harvestersByType[dataProvider.Type];

                    if (dataProvider.Type == DataProviderType.DwcA)
                    {
                        harvestTaskByDataProvider.Add(dataProvider, harvestJob.HarvestObservationsAsync(dataProvider, cancellationToken));
                    }
                    else
                    {
                        if (dataProvider.SupportIncrementalHarvest)
                        {
                            harvestTaskByDataProvider.Add(dataProvider, harvestJob.HarvestObservationsAsync(mode, fromDate, cancellationToken));
                        }
                        else
                        {
                            harvestTaskByDataProvider.Add(dataProvider, harvestJob.HarvestObservationsAsync(cancellationToken));
                        }
                    }

                    _logger.LogDebug($"Added {dataProvider.Names.Translate("en-GB")} for {mode} harvest");
                }
                _logger.LogDebug($"Finish adding harvesters ({mode}).");

                _logger.LogInformation($"Start {mode} observations harvesting.");

                await Task.WhenAll(harvestTaskByDataProvider.Values);

                if (mode == JobRunModes.Full)
                {
                    //---------------------------------------------------------------------------------------------------------
                    // 3. Update harvest info
                    //---------------------------------------------------------------------------------------------------------
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

                ////---------------------------------------------------------------------------------------------------------
                //// 4. Make sure all providers where successful
                ////---------------------------------------------------------------------------------------------------------
                //var success = harvestTaskByDataProvider
                //    .All(r => r.Value.Result.Status == RunStatus.Success);

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

        protected async Task<bool> HarvestResources(
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

            var processProviders = activeProviders.Where(p =>
                mode.Equals(JobRunModes.Full) ||
                    p.SupportIncrementalHarvest
            );

            return await RunAsync(mode,
                fromDate,
                harvestProviders,
                processProviders,
                true,
                cancellationToken);
        }
    }
}