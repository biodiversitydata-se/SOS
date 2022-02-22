using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJob : IObservationsHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IDictionary<DataProviderType, IObservationHarvester> _harvestersByType;
        private readonly IProjectHarvester _projectHarvester;
        private readonly ITaxonListHarvester _taxonListHarvester;
        private readonly ILogger<ObservationsHarvestJob> _logger;

        private void StopHarvestIfProcessingIsRunning(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            if (monitoringApi.ProcessingJobs(0, (int)monitoringApi.ProcessingCount())
                .Any(j => j.Value.InProcessingState &&
                          j.Value.Job.Type.Name.Equals("IProcessObservationsJob",
                              StringComparison.CurrentCultureIgnoreCase) &&
                          j.Value.Job.Method.Name.Equals("RunAsync", StringComparison.CurrentCultureIgnoreCase) &&
                          j.Value.Job.Args.Any(a =>
                              a.GetType() == typeof(JobRunModes) &&
                              (JobRunModes)a == mode)))
            {
                _logger.LogInformation($"Stop harvest job ({mode}) since processing is running.");
                cancellationToken = new JobCancellationToken(true);

                cancellationToken.ThrowIfCancellationRequested();
            };
        }

        private  async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
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
                harvestProviders,
                processProviders,
                true,
                cancellationToken);
        }

        /// <summary>
        /// Run harvest and start processing on success if requested
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="harvestProviders"></param>
        /// <param name="processProviders"></param>
        /// <param name="processOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(JobRunModes mode, 
            IEnumerable<DataProvider> harvestProviders,
            IEnumerable<DataProvider> processProviders,
            bool processOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start harvest job ({mode})");
            await HarvestResources(mode, cancellationToken);
            var success = await Harvest(harvestProviders, mode, cancellationToken);

            if (!success)
            {
                _logger.LogInformation($"Harvest job ({mode}) failed");

                throw new Exception($"Harvest job ({mode}) failed");
            }
            
            if (processOnSuccess)
            {
                // If harvest was successful, go on with enqueuing processing job to Hangfire
                var jobId = BackgroundJob.Enqueue<IProcessObservationsJob>(job => job.RunAsync(
                    processProviders.Select(dataProvider => dataProvider.Identifier).ToList(),
                    mode,
                    cancellationToken));

                _logger.LogInformation($"Process Job ({mode}) with Id={ jobId } was enqueued");
            }

            _logger.LogInformation($"Finish harvest job ({mode})");

            return true;
        }

        private async Task<bool> HarvestResources(
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Start {mode} resources harvest jobs");
                if (mode == JobRunModes.Full)
                {
                    await _projectHarvester.HarvestProjectsAsync();
                    await _taxonListHarvester.HarvestTaxonListsAsync();
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

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="dataProviders"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> Harvest(
        IEnumerable<DataProvider> dataProviders,
        JobRunModes mode,
        IJobCancellationToken cancellationToken)
        {
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
                    return false;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations 
                //------------------------------------------------------------------------
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<HarvestInfo>>();
                _logger.LogInformation($"Start adding harvesters ({mode}).");
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
                            harvestTaskByDataProvider.Add(dataProvider, harvestJob.HarvestObservationsAsync(mode, cancellationToken));
                        }
                        else
                        {
                            harvestTaskByDataProvider.Add(dataProvider, harvestJob.HarvestObservationsAsync(cancellationToken));
                        }
                    }
                    
                    _logger.LogDebug($"Added {dataProvider.Names.Translate("en-GB")} for {mode} harvest");
                }
                _logger.LogInformation($"Finish adding harvesters ({mode}).");

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

                //---------------------------------------------------------------------------------------------------------
                // 4. Make sure mandatory providers where successful
                //---------------------------------------------------------------------------------------------------------
                var success = harvestTaskByDataProvider.Where(dp => dp.Key.HarvestFailPreventProcessing)
                    .All(r => r.Value.Result.Status == RunStatus.Success || r.Value.Result.Status == RunStatus.CanceledSuccess);

                _logger.LogInformation($"Finish {mode} observations harvesting. Success: { success }");

                return success;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{mode} observation harvest job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{mode} observation harvest job failed.");
                throw new Exception($"{mode} observation harvest job failed.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="biologgObservationHarvester"></param>
        /// <param name="clamPortalObservationHarvester"></param>
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
        public ObservationsHarvestJob(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IBiologgObservationHarvester biologgObservationHarvester,
            IClamPortalObservationHarvester clamPortalObservationHarvester,
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
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ObservationsHarvestJob> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _projectHarvester = projectHarvester ?? throw new ArgumentNullException(nameof(projectHarvester));
            _taxonListHarvester = taxonListHarvester ?? throw new ArgumentNullException(nameof(taxonListHarvester));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (artportalenObservationHarvester == null) throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            if (biologgObservationHarvester == null) throw new ArgumentNullException(nameof(biologgObservationHarvester));
            if (clamPortalObservationHarvester == null) throw new ArgumentNullException(nameof(clamPortalObservationHarvester));
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
                {DataProviderType.ClamPortalObservations, clamPortalObservationHarvester},
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

        /// <inheritdoc />
        public async Task<bool> RunFullAsync(IJobCancellationToken cancellationToken)
        {
            StopHarvestIfProcessingIsRunning(JobRunModes.Full, cancellationToken);
            return await RunAsync(JobRunModes.Full, cancellationToken);
        }

        public async Task<bool> RunIncrementalActiveAsync(IJobCancellationToken cancellationToken)
        {
            StopHarvestIfProcessingIsRunning(JobRunModes.IncrementalActiveInstance, cancellationToken);
            return await RunAsync(JobRunModes.IncrementalActiveInstance, cancellationToken);
        }

        public async Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken)
        {
            StopHarvestIfProcessingIsRunning(JobRunModes.IncrementalInactiveInstance, cancellationToken);
            return await RunAsync(JobRunModes.IncrementalInactiveInstance, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            List<string> processDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            StopHarvestIfProcessingIsRunning(JobRunModes.Full, cancellationToken);

            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }

            if (processDataProviderIdOrIdentifiers == null || processDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because processDataProviderIdOrIdentifiers is not set");
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

            var processDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(processDataProviderIdOrIdentifiers);
            var processDataProvidersResult = Result.Combine(processDataProviders);
            if (processDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {processDataProvidersResult.Error}");
                return false;
            }

            return await RunAsync(JobRunModes.Full, harvestDataProviders.Select(p => p.Value), processDataProviders.Select(p => p.Value), true, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            StopHarvestIfProcessingIsRunning(JobRunModes.Full, cancellationToken);

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

            return await Harvest(
                harvestDataProviders.Select(d => d.Value).ToList(),
                JobRunModes.Full,
                cancellationToken);
        }
    }
}