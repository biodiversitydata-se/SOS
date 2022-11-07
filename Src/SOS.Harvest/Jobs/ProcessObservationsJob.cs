using System.Collections.Concurrent;
using AgileObjects.AgileMapper.Extensions;
using Elasticsearch.Net;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.FishData.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Harvest.Processors.Kul.Interfaces;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Harvest.Processors.Nors.Interfaces;
using SOS.Harvest.Processors.ObservationDatabase.Interfaces;
using SOS.Harvest.Processors.Sers.Interfaces;
using SOS.Harvest.Processors.Shark.Interfaces;
using SOS.Harvest.Processors.VirtualHerbarium.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Models.Processed.Dataset;
using System.Data;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Processed.Event;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Common;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ProcessObservationsJob : ProcessJobBase, IProcessObservationsJob
    {
        private readonly IDwcArchiveFileWriterCoordinator _dwcArchiveFileWriterCoordinator;
        private readonly IAreaHelper _areaHelper;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ICacheManager _cacheManager;
        private readonly IProcessTimeManager _processTimeManager;
        private readonly IValidationManager _validationManager;
        private readonly ILogger<ProcessObservationsJob> _logger;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IUserObservationRepository _userObservationRepository;
        private readonly IObservationDatasetRepository _observationDatasetRepository;
        private readonly IObservationEventRepository _observationEventRepository;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly ICache<int, Taxon> _taxonCache;
        private readonly Dictionary<DataProviderType, IObservationProcessor> _processorByType;
        private readonly IProcessTaxaJob _processTaxaJob;
        private readonly string _exportContainer;
        private readonly bool _runIncrementalAfterFull;
        private readonly long _minObservationCount;
        private readonly bool _enableTimeManager;

        private async Task<IDictionary<int, Taxon>> GetTaxaAsync(JobRunModes mode)
        {
            // Use current taxa if we are in incremental mode, to speed things up
            if (mode == JobRunModes.Full)
            {
                //----------------------------------------------------------------------
                // Process taxa
                //----------------------------------------------------------------------
                _logger.LogInformation("Start processing taxonomy");

                if (!await _processTaxaJob.RunAsync())
                {
                    _logger.LogError("Failed to process taxonomy");
                    return null;
                }
                _taxonCache.Clear();
                _logger.LogInformation("Finish processing taxonomy");
            }

            //--------------------------------------
            // Get taxonomy
            //--------------------------------------
            _logger.LogInformation("Start getting processed taxa");

            var taxa = await _taxonCache.GetAllAsync();
            if (!taxa?.Any() ?? true)
            {
                _logger.LogWarning("Failed to get processed taxa");
                return null;
            }

            var taxaDictonary = new ConcurrentDictionary<int, Taxon>();
            taxa.ForEach(t => taxaDictonary.TryAdd(t.Id, t));

            _logger.LogInformation($"Finish getting processed taxa ({taxaDictonary.Count})");

            return taxaDictonary;
        }

        private async Task InitializeAreaHelperAsync()
        {
            _logger.LogDebug("Start initialize area cache");
            await _areaHelper.InitializeAsync();
            _logger.LogDebug("Finish initialize area cache");
        }

        private async Task InitializeElasticSearchAsync(JobRunModes mode)
        {
            if (mode == JobRunModes.Full)
            {
                _logger.LogInformation(
                    $"Start clear ElasticSearch index: {_processedObservationRepository.PublicIndexName}");
                await _processedObservationRepository.ClearCollectionAsync(false);

                _logger.LogInformation(
                    $"Finish clear ElasticSearch index: {_processedObservationRepository.PublicIndexName}");

                _logger.LogInformation(
                    $"Start clear ElasticSearch index: {_processedObservationRepository.ProtectedIndexName}");
                await _processedObservationRepository.ClearCollectionAsync(true);

                _logger.LogInformation(
                    $"Finish clear ElasticSearch index: {_processedObservationRepository.ProtectedIndexName}");

                if (_processConfiguration.ProcessUserObservation)
                {
                    _logger.LogInformation($"_processedObservationRepository.LiveMode={_processedObservationRepository.LiveMode}");
                    _logger.LogInformation($"_userObservationRepository.LiveMode={_userObservationRepository.LiveMode}");
                    _userObservationRepository.LiveMode = _processedObservationRepository.LiveMode;
                    _logger.LogInformation($"Set _userObservationRepository.LiveMode={_processedObservationRepository.LiveMode}");

                    _logger.LogInformation(
                        $"Start clear ElasticSearch index: UniqueIndexName={_userObservationRepository.UniqueIndexName}, IndexName={_userObservationRepository.IndexName}");
                    await _userObservationRepository.ClearCollectionAsync();

                    _logger.LogInformation(
                        $"Finish clear ElasticSearch index: {_userObservationRepository.UniqueIndexName}");
                }

                if (_processConfiguration.ProcessObservationDataset)
                {
                    _logger.LogInformation($"_processedObservationRepository.LiveMode={_processedObservationRepository.LiveMode}");

                    // Dataset
                    _logger.LogInformation($"_observationDatasetRepository.LiveMode={_observationDatasetRepository.LiveMode}");
                    _observationDatasetRepository.LiveMode = _observationDatasetRepository.LiveMode;
                    _logger.LogInformation($"Set _observationDatasetRepository.LiveMode={_observationDatasetRepository.LiveMode}");
                    _logger.LogInformation($"Start clear ElasticSearch index: UniqueIndexName={_observationDatasetRepository.UniqueIndexName}, IndexName={_observationDatasetRepository.IndexName}");
                    await _observationDatasetRepository.ClearCollectionAsync();
                    _logger.LogInformation($"Finish clear ElasticSearch index: {_observationDatasetRepository.UniqueIndexName}");

                    // Event                    
                    _logger.LogInformation($"_observationEventRepository.LiveMode={_observationEventRepository.LiveMode}");
                    _observationEventRepository.LiveMode = _observationEventRepository.LiveMode;
                    _logger.LogInformation($"Set _observationEventRepository.LiveMode={_observationEventRepository.LiveMode}");
                    _logger.LogInformation($"Start clear ElasticSearch index: UniqueIndexName={_observationEventRepository.UniqueIndexName}, IndexName={_observationEventRepository.IndexName}");
                    await _observationEventRepository.ClearCollectionAsync();
                    _logger.LogInformation($"Finish clear ElasticSearch index: {_observationEventRepository.UniqueIndexName}");
                }
            }
            else
            {
                _logger.LogInformation($"Start ensure collection exists ({_processedObservationRepository.PublicIndexName})");
                // Create ES index ProcessedObservation-{0/1} if it doesn't exist.
                await _processedObservationRepository.VerifyCollectionAsync(false);
                _logger.LogInformation($"Finish ensure collection exists ({_processedObservationRepository.PublicIndexName})");

                _logger.LogInformation($"Start ensure collection exists ({_processedObservationRepository.ProtectedIndexName})");
                // Create ES index ProcessedObservation-{0/1} if it doesn't exist.
                await _processedObservationRepository.VerifyCollectionAsync(true);
                _logger.LogInformation($"Finish ensure collection exists ({_processedObservationRepository.ProtectedIndexName})");
            }
        }

        /// <summary>
        /// Disable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task DisableIndexingAsync()
        {
            _logger.LogInformation($"Start disable indexing ({_processedObservationRepository.PublicIndexName})");
            await _processedObservationRepository.DisableIndexingAsync(false);
            _logger.LogInformation($"Finish disable indexing ({_processedObservationRepository.PublicIndexName})");

            _logger.LogInformation($"Start disable indexing ({_processedObservationRepository.ProtectedIndexName})");
            await _processedObservationRepository.DisableIndexingAsync(true);
            _logger.LogInformation($"Finish disable indexing ({_processedObservationRepository.ProtectedIndexName})");

            if (_processConfiguration.ProcessUserObservation)
            {
                _logger.LogInformation($"Start disable indexing ({_userObservationRepository.UniqueIndexName})");
                await _userObservationRepository.DisableIndexingAsync();
                _logger.LogInformation($"Finish disable indexing ({_userObservationRepository.UniqueIndexName})");
            }

            if (_processConfiguration.ProcessObservationDataset)
            {
                // Dataset
                _logger.LogInformation($"Start disable indexing ({_observationDatasetRepository.UniqueIndexName})");
                await _observationDatasetRepository.DisableIndexingAsync();
                _logger.LogInformation($"Finish disable indexing ({_observationDatasetRepository.UniqueIndexName})");

                // Event
                _logger.LogInformation($"Start disable indexing ({_observationEventRepository.UniqueIndexName})");
                await _observationEventRepository.DisableIndexingAsync();
                _logger.LogInformation($"Finish disable indexing ({_observationEventRepository.UniqueIndexName})");
            }
        }

        /// <summary>
        /// Enable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task EnableIndexingAsync()
        {
            _logger.LogInformation($"Start enable indexing ({_processedObservationRepository.PublicIndexName})");
            await _processedObservationRepository.EnableIndexingAsync(false);
            _logger.LogInformation($"Finish enable indexing ({_processedObservationRepository.PublicIndexName})");

            _logger.LogInformation($"Start enable indexing ({_processedObservationRepository.ProtectedIndexName})");
            await _processedObservationRepository.EnableIndexingAsync(true);
            _logger.LogInformation($"Finish enable indexing ({_processedObservationRepository.ProtectedIndexName})");

            if (_processConfiguration.ProcessUserObservation)
            {
                _logger.LogInformation($"Start enable indexing ({_userObservationRepository.UniqueIndexName})");
                await _userObservationRepository.EnableIndexingAsync();
                _logger.LogInformation($"Finish enable indexing ({_userObservationRepository.UniqueIndexName})");
            }

            if (_processConfiguration.ProcessObservationDataset)
            {
                // Dataset
                _logger.LogInformation($"Start enable indexing ({_observationDatasetRepository.UniqueIndexName})");
                await _observationDatasetRepository.EnableIndexingAsync();
                _logger.LogInformation($"Finish enable indexing ({_observationDatasetRepository.UniqueIndexName})");

                // Event
                _logger.LogInformation($"Start enable indexing ({_observationEventRepository.UniqueIndexName})");
                await _observationEventRepository.EnableIndexingAsync();
                _logger.LogInformation($"Finish enable indexing ({_observationEventRepository.UniqueIndexName})");
            }
        }

        /// <summary>
        /// Validate that no protected data is accessable (undiffusedgf) from public index
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateIndexesAsync()
        {
            var healthStatus = await _processedObservationRepository.GetHealthStatusAsync(WaitForStatus.Green, 1);
            if (healthStatus == WaitForStatus.Red)
            {
                _logger.LogError("Elastich health status: Red");
                return false;
            }

            var publicCount = await _processedObservationRepository.IndexCountAsync(false);

            // Make sure we have a reasonable amount of observations processed
            if (publicCount < _minObservationCount)
            {
                _logger.LogError($"Validation failed. Only {publicCount} public observations processed. It should be at least {_minObservationCount}");
                return false;
            }

            var protectedCount = (int)await _processedObservationRepository.IndexCountAsync(true);
            if (protectedCount < 1)
            {
                _logger.LogError($"Validation failed. Only {protectedCount} protected observations processed");
                // No protected observations found. No more validation can be done
                return true;
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
                ValidateRandomObservationsAsync()
            };

            // Make sure no protected observations exists in public index and vice versa
            return (await Task.WhenAll(validationTasks)).All(t => t);
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
            foreach (var rndProObs in rndProObservations)
            {
                if (!string.IsNullOrEmpty(rndProObs.Occurrence?.OccurrenceId) && !protectedObservations.ContainsKey(rndProObs.Occurrence.OccurrenceId))
                {
                    protectedObservations.Add(rndProObs.Occurrence?.OccurrenceId, rndProObs);
                }
            }

            if (protectedObservations?.Any() ?? false)
            {
                // Try to get diffused observations 
                var diffusedObservations = (await _processedObservationRepository.GetObservationsAsync(protectedObservations.Keys, false))?
                    .Where(o => o.Occurrence != null)
                    .ToDictionary(o => o.Occurrence.OccurrenceId, o => o); ;

                if (!diffusedObservations?.Any() ?? true)
                {
                    return true;
                }

                foreach (var protectedObservation in protectedObservations)
                {
                    // Try to get diffused observation with same occurenceId from public index
                    if (!diffusedObservations.TryGetValue(protectedObservation.Key, out var publicObservation))
                    {
                        continue;
                    }

                    // If observation coordinates equals, something is wrong. Validation failed
                    if (protectedObservation.Value.Location.DecimalLatitude.Equals(publicObservation.Location.DecimalLatitude)
                         || protectedObservation.Value.Location.DecimalLongitude.Equals(publicObservation.Location.DecimalLongitude)
                       )
                    {
                        var errorString = $"Failed to validate random observation coordinates. Coordinates match between protected and public index for observation with OccurrenceId: {protectedObservation.Value.Occurrence.OccurrenceId},";
                        errorString += $"Public coords:{publicObservation.Location.DecimalLatitude}, {publicObservation.Location.DecimalLongitude},";
                        errorString += $"Protected coords:{protectedObservation.Value.Location.DecimalLatitude}, {protectedObservation.Value.Location.DecimalLongitude},";
                        _logger.LogError(errorString);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///  Run process job
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(
            IEnumerable<DataProvider> dataProvidersToProcess,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processOverallTimerSessionId = _processTimeManager.Start(ProcessTimeManager.TimerTypes.ProcessOverall);

                //-----------------
                // 1. Arrange
                //-----------------
                _processedObservationRepository.LiveMode = mode == JobRunModes.IncrementalActiveInstance;

                //-----------------
                // 2. Validation
                //-----------------
                if (!dataProvidersToProcess.Any())
                {
                    return false;
                }

                //----------------------------------------------------------------------
                // 3. Initialization of meta data etc
                //----------------------------------------------------------------------
                var getTaxaTask = GetTaxaAsync(mode);
                await Task.WhenAll(getTaxaTask, InitializeAreaHelperAsync(), _validationManager.VerifyCollectionAsync(mode));

                var taxonById = await getTaxaTask;

                if ((taxonById?.Count ?? 0) == 0)
                {
                    return false;
                }

                cancellationToken?.ThrowIfCancellationRequested();

                // Init indexes
                await InitializeElasticSearchAsync(mode);

                if (mode == JobRunModes.Full)
                {
                    //------------------------------------------------------------------------
                    // 4. Start DWC file writing
                    //------------------------------------------------------------------------
                    _dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();

                    // Disable indexing for public and protected index
                    await DisableIndexingAsync();
                }

                //------------------------------------------------------------------------
                // 5. Create observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------                
                var result = await ProcessVerbatim(dataProvidersToProcess, mode, taxonById, cancellationToken);
                var success = result.All(t => t.Value.Status == RunStatus.Success);
                
                //---------------------------------
                // 6. Create ElasticSearch index
                //---------------------------------
                if (success)
                {
                    // Update dynamic provider data
                    await UpdateProvidersMetadataAsync(dataProvidersToProcess);
                    
                    if (mode == JobRunModes.Full)
                    {
                        // Enable indexing for public and protected index
                        await EnableIndexingAsync();

                        var processCount = result.Sum(s => s.Value.PublicCount);
                        var docCount = await _processedObservationRepository.IndexCountAsync(false);
                        var iterations = 0;
                        // Compare number of documents processed with acctually db count
                        // If docCoumt is less than process count, indexing is not ready yet
                        while (docCount < processCount && iterations < 100)
                        {
                            iterations++; // Safty to prevent infinite loop.
                            _logger.LogInformation($"Waiting for indexing to be done {iterations}");
                            Thread.Sleep(TimeSpan.FromSeconds(6)); // Wait for Elasticsearch indexing to finish.
                            docCount = await _processedObservationRepository.IndexCountAsync(false);
                        }

                        // Add data stewardardship datasets
                        if (_processConfiguration.ProcessObservationDataset)
                        {
                            await AddObservationDatasetsAsync();
                            await AddObservationEventsAsync();
                        }

                        if (_runIncrementalAfterFull)
                        {
                            // Enqueue incremental harvest/process job to Hangfire in order to get latest sightings
                            var jobId = BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunIncrementalInactiveAsync(cancellationToken));

                            _logger.LogInformation($"Incremental harvest/process job with Id={jobId} was enqueued");
                        }                        

                        //----------------------------------------------------------------------------
                        // 7. End create DwC CSV files and merge the files into multiple DwC-A files.
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

                                _logger.LogInformation($"Upload file to blob storage job with Id={uploadJobId} was enqueued");
                            }
                        }                        
                    }

                    if (!await _processedObservationRepository.EnsureNoDuplicatesAsync())
                    {
                        _logger.LogError($"Failed to delete duplicates");
                    }                    

                    // When we do a incremental harvest to live index, there is no meaning to do validation since the data is already live
                    if (mode == JobRunModes.Full && !_runIncrementalAfterFull ||
                        mode == JobRunModes.IncrementalInactiveInstance)
                    {
                        var validateIndexTimerSessionId = _processTimeManager.Start(ProcessTimeManager.TimerTypes.ValidateIndex);
                        _logger.LogInformation($"Start validate indexes");
                        if (!await ValidateIndexesAsync())
                        {
                            throw new Exception("Validation of processed indexes failed. Job stopped to prevent leak of protected data");
                        }
                        _logger.LogInformation($"Finish validate indexes");
                        _processTimeManager.Stop(ProcessTimeManager.TimerTypes.ValidateIndex, validateIndexTimerSessionId);

                        // Get on going job id's
                        var onGouingJobIds = GetOnGoingJobIds( "ICreateDoiJob", "IExportAndSendJob", "IExportAndStoreJob" );

                        // Toggle active instance if we are done
                        _logger.LogInformation($"Toggle instance {_processedObservationRepository.ActiveInstance} => {_processedObservationRepository.InActiveInstance}");
                        await _processedObservationRepository.SetActiveInstanceAsync(_processedObservationRepository
                            .InActiveInstance);

                        // Clear processed configuration cache in observation API
                        _logger.LogInformation($"Start clear processed configuration cache at search api");
                        await _cacheManager.ClearAsync(Cache.ProcessedConfiguration);
                        _logger.LogInformation($"Finish clear processed configuration cache at search api");

                        // Restart export jobs since we have switch data base and "SearchAfter" will fale to go on
                        RestartJobs(onGouingJobIds);
                    }
                }

                _logger.LogInformation($"Processing done: {success} {mode}");                

                _processTimeManager.Stop(ProcessTimeManager.TimerTypes.ProcessOverall, processOverallTimerSessionId);

                if (_enableTimeManager)
                {
                    var timers = _processTimeManager.GetTimers();

                    foreach (var timer in timers)
                    {
                        var timerMessage = $"Total duration for {timer.Key}:{mode} was {timer.Value.TotalDuration:g}";

                        if (timer.Value.SessionCount > 1)
                        {
                            timerMessage += $", there where {timer.Value.SessionCount} sessions and the average duration was {timer.Value.AverageDuration:g}";
                        }

                        _logger.LogInformation(timerMessage);
                    }
                }                

                //-------------------------------
                // 8. Return processing result
                //-------------------------------
                return success ? true : throw new Exception($@"Failed to process observations. {string.Join(", ", result
                    .Where(r => r.Value.Status != RunStatus.Success)
                        .Select(r => $"Provider: {r.Key}-{r.Value.Status}"))}");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Process sightings job failed.");
                throw new Exception("Process sightings job failed.");
            }
            finally
            {
                if (mode == JobRunModes.Full)
                {
                    _dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();
                }
            }
        }

        /// <summary>
        ///  Process verbatim observations
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>
        /// <param name="taxonById"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<IDictionary<DataProvider, ProcessingStatus>> ProcessVerbatim(
            IEnumerable<DataProvider> dataProvidersToProcess,
            JobRunModes mode,
            IDictionary<int, Taxon> taxonById,
            IJobCancellationToken cancellationToken)
        {
            var processStart = DateTime.Now;

            var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
            foreach (var dataProvider in dataProvidersToProcess)
            {
                if (!dataProvider.IsActive ||
                    (mode != JobRunModes.Full && !dataProvider.SupportIncrementalHarvest))
                {
                    continue;
                }

                var processor = _processorByType[dataProvider.Type];
                processTaskByDataProvider.Add(dataProvider,
                    processor.ProcessAsync(dataProvider, taxonById, mode, cancellationToken));
            }

            var success = (await Task.WhenAll(processTaskByDataProvider.Values)).All(t => t.Status == RunStatus.Success);

            await UpdateProcessInfoAsync(mode, processStart, processTaskByDataProvider, success);
            return processTaskByDataProvider.ToDictionary(pt => pt.Key, pt => pt.Value.Result);
        }

        /// <summary>
        /// Update process info
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="processStart"></param>
        /// <param name="processTaskByDataProvider"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        private async Task UpdateProcessInfoAsync(JobRunModes mode,
            DateTime processStart,
            IDictionary<DataProvider,
                Task<ProcessingStatus>> processTaskByDataProvider,
            bool success)
        {

            if (!processTaskByDataProvider?.Any() ?? true)
            {
                return;
            }

            // Try to get process info for current instance
            var processInfo = await GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName);

            if (processInfo == null || mode == JobRunModes.Full)
            {
                var providersInfo = new List<ProviderInfo>();

                var totalProcessFailCount = 0;
                var totalPublicCount = 0;
                var totalProtectedCount = 0;

                foreach (var taskProvider in processTaskByDataProvider)
                {
                    var provider = taskProvider.Key;
                    var processResult = taskProvider.Value.Result;

                    if (processResult == null)
                    {
                        continue;
                    }

                    // Get harvest info and create a provider info object 
                    var harvestInfo = await GetHarvestInfoAsync(provider.Identifier);
                    var providerInfo = new ProviderInfo(provider)
                    {
                        HarvestCount = harvestInfo?.Count,
                        HarvestEnd = harvestInfo?.End,
                        HarvestNotes = harvestInfo?.Notes,
                        HarvestStart = harvestInfo?.Start,
                        HarvestStatus = harvestInfo?.Status,
                        ProcessFailCount = processResult.FailedCount,
                        PublicProcessCount = processResult.PublicCount,
                        ProtectedProcessCount = processResult.ProtectedCount,
                        ProcessEnd = processResult.End,
                        ProcessStart = processResult.Start,
                        ProcessStatus = processResult.Status
                    };

                    providersInfo.Add(providerInfo);

                    totalProcessFailCount += processResult.FailedCount;
                    totalPublicCount += processResult.PublicCount;
                    totalProtectedCount += processResult.ProtectedCount;
                }

                var metaDataProcessInfo = await GetProcessInfoAsync(new[]
                    {
                        nameof(Lib.Models.Processed.Observation.Area),
                        nameof(Taxon)
                    }
                );

                processInfo = new ProcessInfo(_processedObservationRepository.UniquePublicIndexName, processStart)
                {
                    ProcessFailCount = totalProcessFailCount,
                    PublicCount = totalPublicCount,
                    ProtectedCount = totalProtectedCount,
                    End = DateTime.Now,
                    MetadataInfo = metaDataProcessInfo,
                    ProvidersInfo = providersInfo,
                    Status = success ? RunStatus.Success : RunStatus.Failed
                };
            }
            else
            {
                foreach (var taskProvider in processTaskByDataProvider)
                {
                    var provider = taskProvider.Key;
                    var processResult = taskProvider.Value.Result;

                    if (processResult == null)
                    {
                        continue;
                    }

                    // Get provider info and update incremental values
                    var providerInfo = processInfo.ProvidersInfo.FirstOrDefault(pi => pi.DataProviderId == provider.Id);
                    if (providerInfo == null)
                    {
                        continue;
                    }

                    providerInfo.LatestIncrementalPublicCount = processResult.PublicCount;
                    providerInfo.LatestIncrementalProtectedCount = processResult.ProtectedCount;
                    providerInfo.LatestIncrementalEnd = processResult.End;
                    providerInfo.LatestIncrementalStart = processResult.Start;
                    providerInfo.LatestIncrementalStatus = processResult.Status;
                }
            }

            _logger.LogInformation("Start updating process info for observations");
            await SaveProcessInfo(processInfo);
            _logger.LogInformation("Finish updating process info for observations");
        }

        private async Task UpdateProvidersMetadataAsync(IEnumerable<DataProvider> providers)
        {
            foreach (var provider in providers.Where(p => p.SupportDynamicEml))
            {
                var eml = await _dataProviderCache.GetEmlAsync(provider.Id);

                if (eml == null)
                {
                    _logger.LogWarning($"No eml file found for provider: {provider.Identifier}");
                    continue;
                }

                // Get public meta data
                var metadata = await _processedObservationRepository.GetProviderMetaDataAsync(provider.Id, false);

                // Get protected meta data
                var protctedMetadata = await _processedObservationRepository.GetProviderMetaDataAsync(provider.Id, true);

                // Copmare public and protected and store peek values
                if ((protctedMetadata.firstSpotted ?? metadata.firstSpotted) < metadata.firstSpotted)
                {
                    metadata.firstSpotted = protctedMetadata.firstSpotted;
                }

                if ((protctedMetadata.lastSpotted ?? metadata.lastSpotted) < metadata.lastSpotted)
                {
                    metadata.lastSpotted = protctedMetadata.lastSpotted;
                }

                if (protctedMetadata.geographicCoverage.BottomRight.Lon > metadata.geographicCoverage.BottomRight.Lon)
                {
                    metadata.geographicCoverage.BottomRight.Lon = protctedMetadata.geographicCoverage.BottomRight.Lon;
                }

                if (protctedMetadata.geographicCoverage.BottomRight.Lat < metadata.geographicCoverage.BottomRight.Lat)
                {
                    metadata.geographicCoverage.BottomRight.Lat = protctedMetadata.geographicCoverage.BottomRight.Lat;
                }

                if (protctedMetadata.geographicCoverage.TopLeft.Lon < metadata.geographicCoverage.TopLeft.Lon)
                {
                    metadata.geographicCoverage.TopLeft.Lon = protctedMetadata.geographicCoverage.TopLeft.Lon;
                }

                if (protctedMetadata.geographicCoverage.TopLeft.Lat > metadata.geographicCoverage.TopLeft.Lat)
                {
                    metadata.geographicCoverage.BottomRight.Lat = protctedMetadata.geographicCoverage.BottomRight.Lat;
                }

                DwCArchiveEmlFileFactory.UpdateDynamicMetaData(eml, metadata.firstSpotted, metadata.lastSpotted, metadata.geographicCoverage);
                await _dataProviderCache.StoreEmlAsync(provider.Id, eml);
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
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessObservationsJob(IProcessedObservationCoreRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IArtportalenObservationProcessor artportalenObservationProcessor,
            IFishDataObservationProcessor fishDataObservationProcessor,
            IKulObservationProcessor kulObservationProcessor,
            IMvmObservationProcessor mvmObservationProcessor,
            INorsObservationProcessor norsObservationProcessor,
            IObservationDatabaseProcessor observationDatabaseProcessor,
            ISersObservationProcessor sersObservationProcessor,
            ISharkObservationProcessor sharkObservationProcessor,
            IVirtualHerbariumObservationProcessor virtualHerbariumObservationProcessor,
            IDwcaObservationProcessor dwcaObservationProcessor,
            ICache<int, Taxon> taxonCache,
            IDataProviderCache dataProviderCache,
            ICacheManager cacheManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ProcessConfiguration processConfiguration,
            IUserObservationRepository userObservationRepository,            
            IObservationDatasetRepository observationDatasetRepository,
            IObservationEventRepository observationEventRepository,
            ILogger<ProcessObservationsJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _taxonCache = taxonCache ??
                          throw new ArgumentNullException(nameof(taxonCache));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _processTimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));

            if (fishDataObservationProcessor == null) throw new ArgumentNullException(nameof(fishDataObservationProcessor));
            if (kulObservationProcessor == null) throw new ArgumentNullException(nameof(kulObservationProcessor));
            if (mvmObservationProcessor == null) throw new ArgumentNullException(nameof(mvmObservationProcessor));
            if (norsObservationProcessor == null) throw new ArgumentNullException(nameof(norsObservationProcessor));
            if (sersObservationProcessor == null) throw new ArgumentNullException(nameof(sersObservationProcessor));
            if (sharkObservationProcessor == null) throw new ArgumentNullException(nameof(sharkObservationProcessor));
            if (virtualHerbariumObservationProcessor == null)
                throw new ArgumentNullException(nameof(virtualHerbariumObservationProcessor));
            if (dwcaObservationProcessor == null) throw new ArgumentNullException(nameof(dwcaObservationProcessor));
            if (artportalenObservationProcessor == null)
                throw new ArgumentNullException(nameof(artportalenObservationProcessor));
            if (sharkObservationProcessor == null) throw new ArgumentNullException(nameof(sharkObservationProcessor));
            _processorByType = new Dictionary<DataProviderType, IObservationProcessor>
            {
                {DataProviderType.ArtportalenObservations, artportalenObservationProcessor},
                {DataProviderType.DwcA, dwcaObservationProcessor},
                {DataProviderType.BiologgObservations, dwcaObservationProcessor},
                {DataProviderType.FishDataObservations, fishDataObservationProcessor},
                {DataProviderType.KULObservations, kulObservationProcessor},
                {DataProviderType.MvmObservations, mvmObservationProcessor},
                {DataProviderType.NorsObservations, norsObservationProcessor},
                {DataProviderType.ObservationDatabase, observationDatabaseProcessor},
                {DataProviderType.SersObservations, sersObservationProcessor},
                {DataProviderType.SharkObservations, sharkObservationProcessor},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationProcessor},
                {DataProviderType.iNaturalistObservations, dwcaObservationProcessor}
            };

            _exportContainer = processConfiguration?.Export_Container ??
                               throw new ArgumentNullException(nameof(processConfiguration));
            _runIncrementalAfterFull = processConfiguration.RunIncrementalAfterFull;
            _minObservationCount = processConfiguration.MinObservationCount;
            _enableTimeManager = processConfiguration.EnableTimeManager;
            _processConfiguration = processConfiguration;
            _userObservationRepository = userObservationRepository ?? throw new ArgumentNullException(nameof(userObservationRepository));
            _observationDatasetRepository = observationDatasetRepository ?? throw new ArgumentNullException(nameof(observationDatasetRepository));
            _observationEventRepository = observationEventRepository ?? throw new ArgumentNullException(nameof(observationEventRepository));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            if (mode == JobRunModes.Full)
            {
                _dataProviderCache.Clear();
            }

            var allDataProviders = await _dataProviderCache.GetAllAsync();
            List<DataProvider> dataProvidersToProcess;
            if (dataProviderIdOrIdentifiers?.Any() ?? false)
            {
                dataProvidersToProcess = allDataProviders.Where(dataProvider =>
                        dataProviderIdOrIdentifiers.Any(dataProvider.EqualsIdOrIdentifier) &&
                        dataProvider.IsActive &&
                        (mode == JobRunModes.Full || dataProvider.SupportIncrementalHarvest))
                    .ToList();
            }
            else
            {
                dataProvidersToProcess = allDataProviders.Where(dataProvider =>
                        dataProvider.IsActive &&
                        (mode == JobRunModes.Full || dataProvider.SupportIncrementalHarvest))
                    .ToList();
            }

            return await RunAsync(
                dataProvidersToProcess,
                mode,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            IJobCancellationToken cancellationToken)
        {
            _dataProviderCache.Clear();

            var dataProviders = await _dataProviderCache.GetAllAsync();
            var dataProvidersToProcess = dataProviders.Where(dataProvider => dataProvider.IsActive).ToList();
            return await RunAsync(
                dataProvidersToProcess,
                JobRunModes.Full,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ProcessArtportalenObservationsAsync(IEnumerable<ArtportalenObservationVerbatim> verbatims)
        {
            var processor = _processorByType[DataProviderType.ArtportalenObservations] as IArtportalenObservationProcessor;
            var provider = await _dataProviderCache.GetAsync(1);
            var taxa = await GetTaxaAsync(JobRunModes.IncrementalActiveInstance);
            _processedObservationRepository.LiveMode = true;
            return await processor.ProcessObservationsAsync(provider, taxa, verbatims);
        }

        private async Task AddObservationDatasetsAsync()
        {
            try
            {
                /*
                 * Workflow - Med nuvarande SOS-struktur
                 * --------------------------------------
                 *  1. Observationer skördas från Artportalen till MongoDB precis som vanligt, men nu ska också en ny property DataStewardshipDatasetId sättas för de observationer som ingår i datavärdskapet. En ny tabell i Artportalen behövs som beskriver datasetet och ytterligare en som pekar på vilka projekt som ingår i datasetet.
                 *  2. Datasetets metadata ska skördas från Artportalen till MongoDB.
                 *  3. Observationerna processas precis som tidigare.
                 *  4. När processningen av alla observationer är klara så processas dataseten, dels utifrån metadatainformationen som finns i MongoDB och dels från de processade observationerna för att få fram vilka EventId:n som ingår i datasetet.
                 *
                 *  Problem
                 *  ----------------------------------
                 *  1. Det finns en del strukturer som inte finns i SOS idag. Exempelvis:
                 *    - WeatherVariable
                 *  Lösning: Antingen får vi lägga till mer properties till nuvarande struktur, eller så får vi skapa ett nytt index för datavärdskapet.
                 */

                List<ObservationDataset> datasets = new List<ObservationDataset>();
                var batDataset = GetSampleBatDataset();
                batDataset.EndDate = DateTime.Now;

                // Determine which events that belongs to this dataset. Aggregate unique EventIds with filter: ProjectIds in [3606]
                var searchFilter = new SearchFilter(0);
                searchFilter.DataStewardshipDatasetIds = new List<string> { batDataset.Identifier };
                var eventIds = await _processedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
                batDataset.EventIds = eventIds.Select(m => m.AggregationKey).ToList();
                datasets.Add(batDataset);
                await _observationDatasetRepository.AddManyAsync(datasets);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Add data stewardship datasets failed.");
            }
        }

        private readonly List<string> _observationEventOutputFields = new List<string>()
        {
            "occurrence",
            "location",
            "event",
            "dataStewardshipDatasetId",
            "institutionCode",
        };

        private async Task AddObservationEventsAsync()
        {
            try
            {                
                int batchSize = 5000;
                var filter = new SearchFilter(0);
                filter.IsPartOfDataStewardshipDataset = true;
                var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
                Dictionary<string, List<string>> totalOccurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId, m => m.OccurrenceIds);
                var chunks = totalOccurrenceIdsByEventId.Chunk(batchSize);

                foreach (var chunk in chunks) // todo - do this step in parallel
                {
                    var occurrenceIdsByEventId = chunk.ToDictionary(m => m.Key, m => m.Value);
                    var firstOccurrenceIdInEvents = occurrenceIdsByEventId.Select(m => m.Value.First());
                    var observations = await _processedObservationRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
                    var events = new List<ObservationEvent>();
                    foreach (var observation in observations)
                    {
                        var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
                        var eventModel = observation.ToObservationEvent(occurrenceIds);
                        events.Add(eventModel);
                    }

                    // write to ES
                    await _observationEventRepository.AddManyAsync(events);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Add data stewardship events failed.");
            }
        }


        private ObservationDataset GetSampleBatDataset()
        {
            var dataset = new ObservationDataset            
            {
                Identifier = "ArtportalenDataHost - Dataset Bats", // Ändra till Id-nummer. Enligt DPS:en ska det vara ett id-nummer från informationsägarens metadatakatalog. Om det är LST som är informationsägare så bör de ha datamängden registrerad i sin metadatakatalog, med ett id där.
                Metadatalanguage = "Swedish",
                Language = "Swedish",
                AccessRights = AccessRights.Publik,
                Purpose = Purpose.NationellMiljöövervakning,                
                Assigner = new Organisation
                {
                    OrganisationID = "2021001975",
                    OrganisationCode = "Naturvårdsverket"
                },
                // Creator = Utförare av datainsamling.
                Creator = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "SLU Artdatabanken"
                },
                // Finns inte alltid i AP, behöver skapas/hämtasandra DV informationskällor?
                OwnerinstitutionCode = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "Länsstyrelsen Jönköping"
                },
                Publisher = new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "SLU Artdatabanken"
                },
                DataStewardship = "Datavärdskap Naturdata: Arter",
                StartDate = new DateTime(2011, 1, 1),
                EndDate = null,
                Description = "Inventeringar av fladdermöss som görs inom det gemensamma delprogrammet för fladdermöss, dvs inom regional miljöövervakning, biogeografisk uppföljning och områdesvis uppföljning (uppföljning av skyddade områden).\r\n\r\nDet finns totalt tre projekt på Artportalen för det gemensamma delprogrammet och i detta projekt rapporteras data från den biogeografiska uppföljningen. Syftet med övervakningen är att följa upp hur antal och utbredning av olika arter förändras över tid. Övervakningen ger viktig information till bland annat EU-rapporteringar, rödlistningsarbetet och kan även användas i uppföljning av miljömålen och som underlag i ärendehandläggning. Den biogeografiska uppföljningen omfattar för närvarande några av de mest artrika fladdermuslokalerna i de olika biogeografiska regionerna i Sverige. Dessa inventeras vartannat år. Ett fåartsområde för fransfladdermus i norra Sverige samt några övervintringslokaler ingår också i övervakningen.",
                Title = "Fladdermöss",
                Spatial = "Sverige",                
                ProjectId = "Artportalen ProjectId:3606",
                ProjectCode = "Fladdermöss - gemensamt delprogram (biogeografisk uppföljning)",
                Methodology = new List<Methodology>
                {
                    new Methodology
                    {
                        MethodologyDescription = "Methodology description?", // finns sällan i projektbeskrivning i AP, behöver hämtas från andra DV informationskällor
                        MethodologyLink = "https://www.naturvardsverket.se/upload/stod-i-miljoarbetet/vagledning/miljoovervakning/handledning/metoder/undersokningstyper/landskap/fladdermus-artkartering-2017-06-05.pdf",
                        MethodologyName = "Undersökningstyp fladdermöss - artkartering",
                        SpeciesList = "Species list?" // artlistan behöver skapas, alternativt "all species occurring in Sweden"
                    }
                },
                EventIds = new List<string>
                {
                    //"urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00"
                }
            };

            return dataset;
        }
    }
}