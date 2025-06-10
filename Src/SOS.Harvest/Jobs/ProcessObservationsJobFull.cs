using Elastic.Clients.Elasticsearch;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Data;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ProcessObservationsJobFull : ProcessObservationsJobBase, IProcessObservationsJobFull
    {
        private readonly IDwcArchiveFileWriterCoordinator _dwcArchiveFileWriterCoordinator;
        private readonly IObservationsHarvestJobIncremental _observationsIncrementalHarvestJob;

        private readonly Dictionary<DataProviderType, IDatasetProcessor> _datasetProcessorByType;
        private readonly Dictionary<DataProviderType, IEventProcessor> _eventProcessorByType;

        private readonly IDatasetRepository _observationDatasetRepository;
        private readonly IEventRepository _observationEventRepository;
        private readonly IUserObservationRepository _userObservationRepository;

        private readonly ICacheManager _cacheManager;
        private readonly string _exportContainer;

        private async Task DisableEsDatasetIndexingAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"Start disable indexing ({_observationDatasetRepository.UniqueIndexName})");
            await _observationDatasetRepository.DisableIndexingAsync();
            _logger.LogInformation($"Finish disable indexing ({_observationDatasetRepository.UniqueIndexName})");
        }

        private async Task DisableEsEventIndexingAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"Start disable indexing ({_observationEventRepository.UniqueIndexName})");
            await _observationEventRepository.DisableIndexingAsync();
            _logger.LogInformation($"Finish disable indexing ({_observationEventRepository.UniqueIndexName})");
        }

        private async Task EnableEsDatasetIndexingAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"Start enable indexing ({_observationDatasetRepository.UniqueIndexName})");
            await _observationDatasetRepository.EnableIndexingAsync();
            _logger.LogInformation($"Finish enable indexing ({_observationDatasetRepository.UniqueIndexName})");
        }

        private async Task EnableEsEventIndexingAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"Start enable indexing ({_observationEventRepository.UniqueIndexName})");
            await _observationEventRepository.EnableIndexingAsync();
            _logger.LogInformation($"Finish enable indexing ({_observationEventRepository.UniqueIndexName})");
        }

        private async Task InitializeElasticSearchDatasetAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"ProcessedConfiguration.ActiveIndex is {_processedObservationRepository.ActiveInstance}");
            _logger.LogInformation($"_observationDatasetRepository.LiveMode={_observationDatasetRepository.LiveMode}");
            _observationDatasetRepository.LiveMode = _processedObservationRepository.LiveMode;
            _logger.LogInformation($"Set _observationDatasetRepository.LiveMode={_processedObservationRepository.LiveMode}");
            _logger.LogInformation($"Start clear ElasticSearch index: UniqueIndexName={_observationDatasetRepository.UniqueIndexName}, IndexName={_observationDatasetRepository.IndexName}");
            await _observationDatasetRepository.ClearCollectionAsync();
            _logger.LogInformation($"Finish clear ElasticSearch index: {_observationDatasetRepository.UniqueIndexName}");
        }

        private async Task InitializeElasticSearchEventAsync()
        {
            if (!_processConfiguration.ProcessDataset) return;
            _logger.LogInformation($"ProcessedConfiguration.ActiveIndex is {_processedObservationRepository.ActiveInstance}");
            _logger.LogInformation($"_observationEventRepository.LiveMode={_observationEventRepository.LiveMode}");
            _observationEventRepository.LiveMode = _processedObservationRepository.LiveMode;
            _logger.LogInformation($"Set _observationEventRepository.LiveMode={_processedObservationRepository.LiveMode}");
            _logger.LogInformation($"Start clear ElasticSearch index: UniqueIndexName={_observationEventRepository.UniqueIndexName}, IndexName={_observationEventRepository.IndexName}");
            await _observationEventRepository.ClearCollectionAsync();
            _logger.LogInformation($"Finish clear ElasticSearch index: {_observationEventRepository.UniqueIndexName}");
        }

        private async Task<IDictionary<DataProvider, ProcessingStatus>?> ProcessVerbatimDatasets(
           IEnumerable<DataProvider> dataProvidersToProcess,
           JobRunModes mode,
           IDictionary<int, Taxon>? taxonById,
           IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start processing verbatim datasets");
            if (dataProvidersToProcess == null || !dataProvidersToProcess.Any()) return null;
            var processStart = DateTime.Now;

            var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
            foreach (var dataProvider in dataProvidersToProcess)
            {
                _logger.LogDebug($"Start processing verbatim datasets for data provider: {dataProvider}");
                if (_datasetProcessorByType.TryGetValue(dataProvider.Type, out var processor))
                {
                    processTaskByDataProvider.Add(dataProvider,
                    processor.ProcessAsync(dataProvider, cancellationToken));
                }
            }

            var success = (await Task.WhenAll(processTaskByDataProvider.Values)).All(t => t.Status == RunStatus.Success);
            _logger.LogDebug("End processing verbatim datasets");

            //await UpdateProcessInfoAsync(mode, processStart, processTaskByDataProvider, success);
            return processTaskByDataProvider.ToDictionary(pt => pt.Key, pt => pt.Value.Result);
        }

        private async Task<IDictionary<DataProvider, ProcessingStatus>?> ProcessVerbatimEvents(
            IEnumerable<DataProvider> dataProvidersToProcess,
            JobRunModes mode,
            IDictionary<int, Taxon>? taxonById,
            IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start processing verbatim events");
            if (dataProvidersToProcess == null || !dataProvidersToProcess.Any()) return null;
            var processStart = DateTime.Now;
            var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
            foreach (var dataProvider in dataProvidersToProcess)
            {
                _logger.LogDebug($"Start processing verbatim events for data provider: {dataProvider}");
                if (_eventProcessorByType.TryGetValue(dataProvider.Type, out var processor))
                {
                    processTaskByDataProvider.Add(dataProvider,
                    processor.ProcessAsync(dataProvider, cancellationToken));
                }
            }

            var success = (await Task.WhenAll(processTaskByDataProvider.Values)).All(t => t.Status == RunStatus.Success);
            _logger.LogDebug("End processing verbatim events");
            //await UpdateProcessInfoAsync(mode, processStart, processTaskByDataProvider, success);
            return processTaskByDataProvider.ToDictionary(pt => pt.Key, pt => pt.Value.Result);
        }

        /// <summary>
        /// Validate that no protected data is accessable (undiffused) from public index
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private async Task<bool> ValidateIndexesAsync()
        {
            var healthStatus = await _processedObservationRepository.GetHealthStatusAsync(HealthStatus.Green, 1);
            if (healthStatus == HealthStatus.Red)
            {
                _logger.LogError("Elastich health status: Red");
                return false;
            }

            var publicCount = await _processedObservationRepository.IndexCountAsync(false);
            // Make sure we have a reasonable amount of observations processed
            if (publicCount < _processConfiguration.MinObservationCount)
            {
                _logger.LogError($"Validation failed. Only {publicCount} public observations processed. It should be at least {_processConfiguration.MinObservationCount}");
                return false;
            }

            var protectedCount = (int)await _processedObservationRepository.IndexCountAsync(true);
            if (protectedCount < _processConfiguration.MinObservationProtectedCount)
            {
                _logger.LogError($"Validation failed. Only {protectedCount} protected observations processed. It should be at least {_processConfiguration.MinObservationProtectedCount}");
                // No protected observations found. Something is wrong
                return false;
            }

            var diskUsagePercents = await _processedObservationRepository.GetDiskUsageAsync();
            foreach(var diskUsagePercent in diskUsagePercents)
            {
                if (diskUsagePercent.Value >= 95)
                {
                    _logger.LogError($"Validation failed. Current disk usage {diskUsagePercent}% dor node: {diskUsagePercent.Key}");
                    return false;
                }
            }
            

            var validationTasks = new[]
            {
                _processedObservationRepository.ValidateProtectionLevelAsync(false),
                _processedObservationRepository.ValidateProtectionLevelAsync(true),
                ValidateDuplicatesAsync(),
                ValidateRandomObservationsAsync(),
                ValidateRandomObservationsAsync(),
                ValidateRandomObservationsAsync(),
                ValidateRandomObservationsAsync(),
                ValidateRandomObservationsAsync(),
                ValidateCountAsync()
            };
            var results = await Task.WhenAll(validationTasks);

            // Make sure validation tasks succeded
            return results.All(t => t);
        }

        /// <summary>
        /// Check that we ha a resonable amount of observations compared to last run
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateCountAsync()
        {
            var processInfoInactive = await GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);
            _processedObservationRepository.LiveMode = true;
            var processInfoActive = await GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);
            _processedObservationRepository.LiveMode = false;

            if (!processInfoActive?.ProvidersInfo?.Any() ?? true)
            {
                // Processing not done before
                return true;
            }

            if (!processInfoInactive?.ProvidersInfo?.Any() ?? true)
            {
                // Something is wrong. process info should be saved at this point
                _logger.LogError("Can't find any process information for current processing");
                return false;
            }

            foreach (var providerInactive in processInfoInactive!.ProvidersInfo)
            {
                var providerActive = processInfoActive!.ProvidersInfo.FirstOrDefault(p => p.DataProviderId.Equals(providerInactive.DataProviderId));

                if ((providerActive?.PreviousProcessLimit ?? 0) > 0)
                {
                    double percentLimit = Math.Max(0, (providerActive!.PreviousProcessLimit-2) / 100.0); // Allow 2% less than limit in order to allow some invalid observations.
                    int activeCount = providerActive.PublicProcessCount.GetValueOrDefault() + providerActive.ProtectedProcessCount.GetValueOrDefault();
                    int inactiveCount = providerInactive.PublicProcessCount.GetValueOrDefault() + providerInactive.ProtectedProcessCount.GetValueOrDefault();
                    int activeCountLimit = (int)(percentLimit * activeCount);
                    if (inactiveCount < activeCountLimit)
                    {
                        _logger.LogError("Validation failed. Observation count for {@dataProvider} is less than {@previousProcessLimit}% of last run. Count this time={@observationCount}. Count previous time={@previousObservationCount}.",
                            providerInactive.DataProviderIdentifier, providerActive!.PreviousProcessLimit, inactiveCount, activeCount);
                        _logger.LogError("Validation failed. Public observation count for {@dataProvider} is less than {@previousProcessLimit}% of last run. Count this time={@publicProcessCount}. Count previous time={@publicProcessCountPrevious}.",
                        providerInactive.DataProviderIdentifier, providerActive!.PreviousProcessLimit, providerInactive.PublicProcessCount, providerActive.PublicProcessCount);
                        _logger.LogError("Validation failed. Protected observation count for {@dataProvider} is less than {@reviousProcessLimit}% of last run. Count this time={@protectedProcessCount}. Count previous time={@protectedProcessCountPrevious}.",
                            providerInactive.DataProviderIdentifier, providerActive!.PreviousProcessLimit, providerActive!.PreviousProcessLimit, providerActive.ProtectedProcessCount);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validate document duplicates
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateDuplicatesAsync()
        {
            const int maxItems = 20;

            var publicIndexDuplicates =
                (await _processedObservationRepository.TryToGetOccurenceIdDuplicatesAsync(false, maxItems))?.ToArray();
            if (publicIndexDuplicates?.Any() ?? false)
            {
                _logger.LogError($"Public index ({_processedObservationRepository.PublicIndexName}) contains multiple documents with same occurrenceId. " +
                                 $"{string.Join(", ", publicIndexDuplicates)}{(publicIndexDuplicates.Count() == maxItems ? "..." : "")}");
                return false;
            }

            var protectedIndexDuplicates =
                (await _processedObservationRepository.TryToGetOccurenceIdDuplicatesAsync(true, maxItems))?.ToArray();
            if (protectedIndexDuplicates?.Any() ?? false)
            {
                _logger.LogError($"Protected index ({_processedObservationRepository.ProtectedIndexName}) contains multiple documents with same occurrenceId. " +
                $"{string.Join(", ", protectedIndexDuplicates)}{(protectedIndexDuplicates.Count() == maxItems ? "..." : "")}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate 1000 random observations
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateRandomObservationsAsync()
        {
            var observationsCount = 1000;

            // Get 1000 random observations from protected index
            var rndProObservations = (await _processedObservationRepository.GetRandomObservationsAsync(observationsCount, true))?.ToArray();
            if (!rndProObservations?.Any() ?? true)
            {
                return true;
            }

            var protectedObservations = new Dictionary<string, Observation>();
            foreach (var rndProObs in rndProObservations!)
            {
                if (!string.IsNullOrEmpty(rndProObs.Occurrence?.OccurrenceId) && !protectedObservations.ContainsKey(rndProObs.Occurrence.OccurrenceId))
                {
                    protectedObservations.Add(rndProObs.Occurrence.OccurrenceId, rndProObs);
                }
            }

            // Remove diffused observation test for now. Some Artportalen observations have the same diffused and real coordinates.
            //if (protectedObservations?.Any() ?? false)
            //{
            //    // Try to get diffused observations 
            //    var diffusedObservations = (await _processedObservationRepository.GetObservationsAsync(protectedObservations.Keys, false))?
            //        .Where(o => o.Occurrence != null)
            //        .ToDictionary(o => o.Occurrence.OccurrenceId, o => o); ;

            //    if (!diffusedObservations?.Any() ?? true)
            //    {
            //        return true;
            //    }

            //    foreach (var protectedObservation in protectedObservations)
            //    {
            //        // Try to get diffused observation with same occurenceId from public index
            //        if (!diffusedObservations!.TryGetValue(protectedObservation.Key, out var publicObservation))
            //        {
            //            continue;
            //        }

            //        // If observation coordinates equals, something is wrong. Validation failed
            //        if (protectedObservation.Value.Location.DecimalLatitude.Equals(publicObservation.Location.DecimalLatitude)
            //             || protectedObservation.Value.Location.DecimalLongitude.Equals(publicObservation.Location.DecimalLongitude)
            //           )
            //        {
            //            var errorString = $"Failed to validate random observation coordinates. Coordinates match between protected and public index for observation with OccurrenceId: {protectedObservation.Value.Occurrence.OccurrenceId},";
            //            errorString += $"Public coords:{publicObservation.Location.DecimalLatitude}, {publicObservation.Location.DecimalLongitude},";
            //            errorString += $"Protected coords:{protectedObservation.Value.Location.DecimalLatitude}, {protectedObservation.Value.Location.DecimalLongitude},";
            //            _logger.LogError(errorString);
            //            return false;
            //        }
            //    }
            //}

            return true;
        }

        protected override async Task<bool> HarvestIncremetalInactiveInstance(IJobCancellationToken cancellationToken)
        {
            return await _observationsIncrementalHarvestJob.RunIncrementalInactiveAsync(cancellationToken);
        }

        /// <summary>
        /// Tasks to do before processing
        /// </summary>
        protected override async Task PreProcessingAsync()
        {
            if (_processConfiguration.ProcessUserObservation)
            {
                _logger.LogInformation($"_processedObservationRepository.LiveMode={_processedObservationRepository.LiveMode}");
                _logger.LogInformation($"_userObservationRepository.LiveMode={_userObservationRepository.LiveMode}");
                _userObservationRepository.LiveMode = _processedObservationRepository.LiveMode;
                _logger.LogInformation($"Set _userObservationRepository.LiveMode={_processedObservationRepository.LiveMode}");

                _logger.LogInformation(
                    "Start clear ElasticSearch index: UniqueIndexName={@esUniqueIndexName}, IndexName={@esIndexName}", _userObservationRepository.UniqueIndexName, _userObservationRepository.IndexName);
                await _userObservationRepository.ClearCollectionAsync();

                _logger.LogInformation(
                    "Finish clear ElasticSearch index: {@esUniqueIndexName}", _userObservationRepository.UniqueIndexName);

                _logger.LogInformation("Start disable indexing ({@esUniqueIndexName})", _userObservationRepository.UniqueIndexName);
                await _userObservationRepository.DisableIndexingAsync();
                _logger.LogInformation("Finish disable indexing ({@esUniqueIndexName})", _userObservationRepository.UniqueIndexName);
            }

            _dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
        }

        /// <summary>
        /// Tasks to do after processing
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="taxonById"></param>
        /// <param name="publicCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override async Task PostProcessingAsync(IEnumerable<DataProvider> dataProvidersToProcess, IDictionary<int, Taxon> taxonById, (int publicCount, int protectedCount) indexCounts, IJobCancellationToken cancellationToken)
        {
            try
            {
                Task.WaitAll(new[] {
                    _processedObservationRepository.WaitForPublicIndexCreationAsync(indexCounts.publicCount, TimeSpan.FromMinutes(10)),
                    _processedObservationRepository.WaitForPublicIndexCreationAsync(indexCounts.protectedCount, TimeSpan.FromMinutes(10), true)
                });
                
                //---------------------------------------------------------------
                //  Start harvest of Artportalen observations that has been
                //    added during the day and doesn't exist in the backup db
                //---------------------------------------------------------------
                if (_processConfiguration.RunIncrementalAfterFull)
                {
                    if (!await HarvestIncremetalInactiveInstance(cancellationToken))
                    {
                        throw new Exception("Failed to harvest incremetal inactive instance");
                    }
                }

                //----------------------------------
                // 8. Validate observation indexes
                //----------------------------------
                var validateIndexTimerSessionId = _processTimeManager.Start(ProcessTimeManager.TimerTypes.ValidateIndex);
                _logger.LogInformation($"Start validate indexes");
                if (!await ValidateIndexesAsync())
                {
                    throw new Exception("Validation of processed indexes failed. Job stopped.");
                }
                _logger.LogInformation($"Finish validate indexes");
                _processTimeManager.Stop(ProcessTimeManager.TimerTypes.ValidateIndex, validateIndexTimerSessionId);

                // Add data stewardardship events & datasets
                if (_processConfiguration.ProcessDataset)
                {
                    //--------------------
                    // 9. Process events
                    //--------------------
                    await InitializeElasticSearchEventAsync();
                    await DisableEsEventIndexingAsync();
                    _logger.LogDebug($"Number of data providers that supports events: {dataProvidersToProcess.Where(m => m.SupportEvents).Count()}");
                    _logger.LogDebug($"Number of data providers that don't supports events: {dataProvidersToProcess.Where(m => !m.SupportEvents).Count()}");
                    var eventResult = await ProcessVerbatimEvents(dataProvidersToProcess.Where(m => m.IsActive), JobRunModes.Full, taxonById, cancellationToken!);
                    //var eventResult = await ProcessVerbatimEvents(dataProvidersToProcess.Where(m => m.IsActive && m.SupportEvents), mode, taxonById, cancellationToken);
                    var eventSuccess = eventResult == null || eventResult.All(t => t.Value.Status == RunStatus.Success);
                    await EnableEsEventIndexingAsync();
                    int eventProcessCount = 0;
                    if (eventResult != null)
                    {
                        eventProcessCount = eventResult.Sum(s => s.Value.PublicCount);
                        await _observationEventRepository.WaitForIndexCreation(eventProcessCount, TimeSpan.FromMinutes(10));
                    }

                    if (eventSuccess)
                    {
                        _logger.LogInformation("Processing events finished successfully. EventCount={@eventProcessCount}", eventProcessCount);
                    }
                    else
                    {
                        _logger.LogInformation("Processing events failed. EventCount={@eventProcessCount}", eventProcessCount);
                    }

                    //----------------------
                    // 10. Process datasets
                    //----------------------
                    await InitializeElasticSearchDatasetAsync();
                    await DisableEsDatasetIndexingAsync();
                    var datasetResult = await ProcessVerbatimDatasets(dataProvidersToProcess.Where(m => m.IsActive), JobRunModes.Full, taxonById, cancellationToken!);
                    //var datasetResult = await ProcessVerbatimDatasets(dataProvidersToProcess.Where(m => m.IsActive && m.SupportDatasets), mode, taxonById, cancellationToken);
                    var datasetSuccess = datasetResult == null || datasetResult.All(t => t.Value.Status == RunStatus.Success);
                    await EnableEsDatasetIndexingAsync();
                    int datasetProcessCount = 0;
                    if (datasetResult != null)
                    {
                        datasetProcessCount = datasetResult.Sum(s => s.Value.PublicCount);
                        await _observationDatasetRepository.WaitForIndexCreation(datasetProcessCount, TimeSpan.FromMinutes(10));
                    }

                    if (datasetSuccess)
                    {
                        _logger.LogInformation("Processing datasets finished successfully. DatasetCount={@datasetProcessCount}", datasetProcessCount);
                    }
                    else
                    {
                        _logger.LogInformation("Processing datasets failed. DatasetCount={@datasetProcessCount}", datasetProcessCount);
                    }
                }

                if (_processConfiguration.ProcessUserObservation)
                {
                    _logger.LogInformation("Start enable indexing ({@esUniqueIndexName})", _userObservationRepository.UniqueIndexName);
                    await _userObservationRepository.EnableIndexingAsync();
                    _logger.LogInformation("Finish enable indexing ({@esUniqueIndexName})", _userObservationRepository.UniqueIndexName);
                }

                //----------------------------------------------------------------------------
                // 11. End create DwC CSV files and merge the files into multiple DwC-A files.
                //----------------------------------------------------------------------------
                var dwCCreationTimerSessionId = _processTimeManager.Start(ProcessTimeManager.TimerTypes.DwCCreation);
                var dwcFiles = await _dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles();
                _processTimeManager.Stop(ProcessTimeManager.TimerTypes.DwCCreation, dwCCreationTimerSessionId);

                if (dwcFiles?.Any() ?? false)
                {
                    foreach (var dwcFile in dwcFiles)
                    {
                        // Enqueue upload file to blob storage job
                        var uploadJobId = BackgroundJob.Enqueue<IUploadToStoreJob>(job => job.RunAsync(dwcFile, _exportContainer, true,
                            cancellationToken));

                        _logger.LogInformation("Upload file to blob storage job with Id={@jobId} was enqueued", uploadJobId);
                    }
                }

                // Get on going job id's
                var onGoingJobIds = GetOnGoingJobIds("ICreateDoiJob", "IExportAndSendJob", "IExportAndStoreJob");

                //---------------------------------------------------------
                // 12. Toggle active instance when full processing is done
                //---------------------------------------------------------
                _logger.LogInformation("Toggle instance {@activeInstance} => {@inactiveInstance}", _processedObservationRepository.ActiveInstance, _processedObservationRepository.InActiveInstance);
                await _processedObservationRepository.SetActiveInstanceAsync(_processedObservationRepository
                    .InActiveInstance);

                //-------------------------------------------------------------------------
                // 13. Clear ProcessedConfiguration cache in all dependent services (APIs)
                //-------------------------------------------------------------------------
                _logger.LogInformation($"Start clear processed configuration cache at search api");
                await _cacheManager.ClearAsync(Cache.ProcessedConfiguration);
                _logger.LogInformation($"Finish clear processed configuration cache at search api");

                // Restart export jobs since we have switch data base and "SearchAfter" will fail to go on
                RestartJobs(onGoingJobIds);
            }
            catch
            {
                throw;
            }
            finally
            {
                _dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="artportalenObservationProcessor"></param>
        /// <param name="fishDataObservationProcessor"></param>
        /// <param name="kulObservationProcessor"></param>
        /// <param name="mvmObservationProcessor"></param>
        /// <param name="norsObservationProcessor"></param>
        /// <param name="observationDatabaseProcessor"></param>
        /// <param name="sersObservationProcessor"></param>
        /// <param name="sharkObservationProcessor"></param>
        /// <param name="virtualHerbariumObservationProcessor"></param>
        /// <param name="dwcaObservationProcessor"></param>
        /// <param name="taxonCache"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="cacheManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="processTaxaJob"></param>
        /// <param name="areaHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="userObservationRepository"></param>
        /// <param name="observationDatasetRepository"></param>
        /// <param name="observationEventRepository"></param>
        /// <param name="dwcaDatasetProcessor"></param>
        /// <param name="artportalenDatasetProcessor"></param>
        /// <param name="artportalenEventProcessor"></param>
        /// <param name="dwcaEventProcessor"></param>
        /// <param name="observationsIncrementalHarvestJob"></param>
        /// <param name="logger"></param>
        public ProcessObservationsJobFull(IProcessedObservationCoreRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IObservationProcessorManager observationProcessorManager,
            ICache<int, Taxon> taxonCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            IDataProviderCache dataProviderCache,
            ICacheManager cacheManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ProcessConfiguration processConfiguration,
            IUserObservationRepository userObservationRepository,
            IDatasetRepository observationDatasetRepository,
            IEventRepository observationEventRepository,
            IDwcaDatasetProcessor dwcaDatasetProcessor,
            IArtportalenDatasetProcessor artportalenDatasetProcessor,
            IArtportalenEventProcessor artportalenEventProcessor,
            IDwcaEventProcessor dwcaEventProcessor,
            IObservationsHarvestJobIncremental observationsIncrementalHarvestJob,
            ILogger<ProcessObservationsJobFull> logger) : base(processedObservationRepository, processInfoRepository, harvestInfoRepository, observationProcessorManager,
                taxonCache, vocabularyCache, dataProviderCache, processTimeManager, validationManager, processTaxaJob, areaHelper, processConfiguration,
            logger)
        {
            _dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));
            _observationsIncrementalHarvestJob = observationsIncrementalHarvestJob ?? throw new ArgumentNullException(nameof(observationsIncrementalHarvestJob));

            _observationDatasetRepository = observationDatasetRepository ?? throw new ArgumentNullException(nameof(observationDatasetRepository));
            _observationEventRepository = observationEventRepository ?? throw new ArgumentNullException(nameof(observationEventRepository));
            _userObservationRepository = userObservationRepository ?? throw new ArgumentNullException(nameof(userObservationRepository));

            _datasetProcessorByType = new Dictionary<DataProviderType, IDatasetProcessor>
            {
                {DataProviderType.DwcA, dwcaDatasetProcessor},
                {DataProviderType.ArtportalenObservations, artportalenDatasetProcessor},
            };

            _eventProcessorByType = new Dictionary<DataProviderType, IEventProcessor>
            {
                {DataProviderType.DwcA, dwcaEventProcessor},
                {DataProviderType.ArtportalenObservations, artportalenEventProcessor},
            };

            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _exportContainer = processConfiguration?.Export_Container ??
                               throw new ArgumentNullException(nameof(processConfiguration));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            IJobCancellationToken cancellationToken)
        {
            await _dataProviderCache.ClearAsync();

            var dataProviders = await _dataProviderCache.GetAllAsync();
            var dataProvidersToProcess = dataProviders.Where(dataProvider => dataProvider.IsActive).ToList();
            _logger.LogInformation($"Start process observations. {LogHelper.GetMemoryUsageSummary()}");
            if (dataProvidersToProcess != null)
            {
                foreach (var dataProvider in dataProvidersToProcess)
                {
                    _logger.LogInformation("Start process {@dataProvider}, PreviousProcessLimit={@previousProcessLimit}", dataProvider.Identifier, dataProvider.PreviousProcessLimit);
                }
            }

            return await RunAsync(
                dataProvidersToProcess,
                JobRunModes.Full,
                cancellationToken);
        }
    }
}