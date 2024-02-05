using AgileObjects.AgileMapper.Extensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Factories;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Collections.Concurrent;
using System.Data;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ProcessObservationsJobBase : ProcessJobBase
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IValidationManager _validationManager;
        private readonly ICache<int, Taxon> _taxonCache;
        private readonly IProcessTaxaJob _processTaxaJob;
        private readonly bool _enableTimeManager;
        private static SemaphoreSlim _getTaxaSemaphore = new SemaphoreSlim(1, 1);

        private async Task InitializeAreaHelperAsync()
        {
            _logger.LogDebug("Start initialize area cache");
            await _areaHelper.InitializeAsync();
            _logger.LogDebug("Finish initialize area cache");
        }

        private async Task InitializeElasticSearchAsync(JobRunModes mode)
        {
            // Make sure we get latest info about current instance
            _processedObservationRepository.ClearConfigurationCache();

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
        }

        /// <summary>
        ///  Process verbatim observations
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>
        /// <param name="taxonById"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<IDictionary<DataProvider, ProcessingStatus>> ProcessVerbatimObservations(
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

                var processor = _observationProcessorManager.GetProcessor(dataProvider.Type);
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

                foreach (var taskProvider in processTaskByDataProvider!)
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
                foreach (var taskProvider in processTaskByDataProvider!)
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

        protected readonly IDataProviderCache _dataProviderCache;
        protected readonly ILogger<ProcessObservationsJobBase> _logger;
        protected readonly IObservationProcessorManager _observationProcessorManager;
        protected readonly ProcessConfiguration _processConfiguration;
        protected readonly IProcessedObservationCoreRepository _processedObservationRepository;
        protected readonly IProcessTimeManager _processTimeManager;

        /// <summary>
        /// Constructor
        /// </summary>      
        protected ProcessObservationsJobBase(IProcessedObservationCoreRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IObservationProcessorManager observationProcessorManager,
            ICache<int, Taxon> taxonCache,
            IDataProviderCache dataProviderCache,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            ILogger<ProcessObservationsJobBase> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _observationProcessorManager = observationProcessorManager ?? throw new ArgumentNullException(nameof(observationProcessorManager));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _taxonCache = taxonCache ??
                          throw new ArgumentNullException(nameof(taxonCache));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _processTimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _enableTimeManager = processConfiguration.EnableTimeManager;
            _processConfiguration = processConfiguration;
        }

        /// <summary>
        /// Get taxonomy
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected async Task<IDictionary<int, Taxon>> GetTaxaAsync(JobRunModes mode)
        {
            await _getTaxaSemaphore.WaitAsync();
            try
            {
                // Use current taxa if we are in incremental mode, to speed things up
                if (mode == JobRunModes.Full)
                {
                    //----------------------------------------------------------------------
                    // Process taxa
                    //----------------------------------------------------------------------
                    _logger.LogInformation("Start harvest taxa");

                    if (!await _processTaxaJob.RunAsync())
                    {
                        _logger.LogError("Failed to process taxa");
                        return null!;
                    }

                    _logger.LogInformation("Finish harvest taxa");

                    _taxonCache.Clear();
                    _logger.LogInformation("Taxa cache cleared.");
                }

                //--------------------------------------
                // Get taxonomy
                //--------------------------------------
                _logger.LogDebug("Start getting taxa from cache");

                var taxa = await _taxonCache.GetAllAsync();
                if (!taxa?.Any() ?? true)
                {
                    _logger.LogError("Failed to get taxa");
                    return null!;
                }

                var taxaDictonary = new ConcurrentDictionary<int, Taxon>();
                taxa.ForEach(t => taxaDictonary.TryAdd(t.Id, t));

                _logger.LogInformation($"Finish getting taxa from cache (taxaDictionary.Count={taxaDictonary.Count}, taxa.Count={taxa?.Count()})");

                return taxaDictonary;
            }
            finally
            {
                _getTaxaSemaphore.Release();
            }
        }

        /// <summary>
        /// Harvest incremental inactive instance is only called for full processing
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task<bool> HarvestIncremetalInactiveInstance(IJobCancellationToken cancellationToken)
        {
            return await Task.Run(() => true);
        }

        /// <summary>
        /// Tasks to do before processing
        /// </summary>
        protected virtual async Task PreProcessingAsync()
        {
            await Task.Yield();
        }

        /// <summary>
        /// Tasks to do after processing
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="taxonById"></param>
        /// <param name="publicCount"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task PostProcessingAsync(IEnumerable<DataProvider> dataProvidersToProcess, IDictionary<int, Taxon> taxonById, (int publicCount, int protectedCount) indexCounts, IJobCancellationToken cancellationToken)
        {
            if (!await _processedObservationRepository.EnsureNoDuplicatesAsync())
            {
                _logger.LogError($"Failed to delete duplicates");
            }
        }

        /// <summary>
        ///  Run process job
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<bool> RunAsync(
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
                await Task.WhenAll(getTaxaTask, InitializeAreaHelperAsync(), _validationManager.VerifyCollectionAsync(mode), _validationManager.VerifyEventCollectionAsync(mode));

                var taxonById = await getTaxaTask;

                if ((taxonById?.Count ?? 0) == 0)
                {
                    return false;
                }

                cancellationToken?.ThrowIfCancellationRequested();

                // Init indexes
                await InitializeElasticSearchAsync(mode);
                // Disable indexing for public and protected index
                await DisableIndexingAsync();
                await PreProcessingAsync();

                //------------------------------------------------------------------------
                // 5. Create observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------                
                var result = await ProcessVerbatimObservations(dataProvidersToProcess, mode, taxonById!, cancellationToken!);
                var success = result.All(t => t.Value.Status == RunStatus.Success);

                //---------------------------------------------------------------
                // 6. Enable Elasticsearch observation index
                //---------------------------------------------------------------
                await EnableIndexingAsync();

                if (success)
                {
                    // Update dynamic provider data
                    await UpdateProvidersMetadataAsync(dataProvidersToProcess);
                    await PostProcessingAsync(dataProvidersToProcess, taxonById!, (result.Sum(s => s.Value.PublicCount), result.Sum(s => s.Value.ProtectedCount)), cancellationToken!);
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
                // 14. Return processing result
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
        }
    }
}