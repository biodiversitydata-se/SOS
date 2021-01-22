using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.FishData.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Mvm.Interfaces;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Processors.Shark.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ProcessJob : ProcessJobBase, IProcessJob
    {
        private readonly IDwcArchiveFileWriterCoordinator _dwcArchiveFileWriterCoordinator;
        private readonly IAreaHelper _areaHelper;
        private readonly ICache<int, DataProvider> _dataProviderCache;
        private readonly IInstanceManager _instanceManager;
        private readonly IValidationManager _validationManager;
        private readonly ILogger<ProcessJob> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ICache<int, Taxon> _taxonCache;
        private readonly Dictionary<DataProviderType, IProcessor> _processorByType;
        private readonly IProcessTaxaJob _processTaxaJob;
        private readonly string _exportContainer;
        private readonly bool _runIncrementalAfterFull;

        private async Task<IDictionary<int, Taxon>> GetTaxa(JobRunModes mode)
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

            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            _logger.LogInformation("Finish getting processed taxa");

            return taxonById;
        }

        private async Task InitializeAreaHelperAsync(JobRunModes mode)
        {
            _logger.LogDebug("Start initialize area cache");
            await _areaHelper.InitializeAsync();
            _logger.LogDebug("Finish initialize area cache");
        }

        private async Task InitializeElasticSearch(JobRunModes mode, bool cleanStart)
        {
            if (cleanStart && mode == JobRunModes.Full)
            {
                _logger.LogInformation(
                    $"Start clear ElasticSearch index: {_processedObservationRepository.IndexName}");
                await _processedObservationRepository.ClearCollectionAsync();
               
                _logger.LogInformation(
                    $"Finish clear ElasticSearch index: {_processedObservationRepository.IndexName}");
            }
            else
            {
                _logger.LogInformation($"Start ensure collection exists ({_processedObservationRepository.IndexName})");
                // Create ES index ProcessedObservation-{0/1} if it doesn't exist.
                await _processedObservationRepository.VerifyCollectionAsync();
                _logger.LogInformation($"Finish ensure collection exists ({_processedObservationRepository.IndexName})");
            }
        }

        /// <summary>
        ///  Run process job
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(
            IEnumerable<DataProvider> dataProvidersToProcess,
            JobRunModes mode,
            bool cleanStart,
            bool copyFromActiveOnFail,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                //-----------------
                // 1. Arrange
                //-----------------
                var processStart = DateTime.Now;
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
                var getTaxaTask = GetTaxa(mode);
                await Task.WhenAll(getTaxaTask, InitializeAreaHelperAsync(mode), _validationManager.VerifyCollectionAsync(mode));

                var taxonById = await getTaxaTask;

                if ((taxonById?.Count ?? 0) == 0)
                {
                    return false;
                }

                cancellationToken?.ThrowIfCancellationRequested();

                if (mode == JobRunModes.Full)
                {
                    //------------------------------------------------------------------------
                    // 4. Start DWC file writing
                    //------------------------------------------------------------------------
                    _dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
                }

                //------------------------------------------------------------------------
                // 5. Create public observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                var success = await ProcessVerbatim(dataProvidersToProcess, mode, ObservationType.Public, taxonById, cleanStart, processStart, copyFromActiveOnFail, cancellationToken);

                //------------------------------------------------------------------------
                // 6. Create protected observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                success = success && await ProcessVerbatim(dataProvidersToProcess, mode, ObservationType.Protected, taxonById, cleanStart, processStart, copyFromActiveOnFail, cancellationToken);

                //------------------------------------------------------------------------
                // 7. Create diffused observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                success = await ProcessVerbatim(dataProvidersToProcess, mode, ObservationType.Diffused, taxonById, cleanStart, processStart, copyFromActiveOnFail, cancellationToken);

                //---------------------------------
                // 8. Create ElasticSearch index
                //---------------------------------
                if (success)
                {
                    // Toogle active instance if it's a full harvest and incremental update not should run after, or after the incremental update has run
                    if (mode == JobRunModes.Full && !_runIncrementalAfterFull || mode == JobRunModes.IncrementalInactiveInstance)
                    {
                        _logger.LogInformation($"Toggle instance {_processedObservationRepository.ActiveInstanceName} => {_processedObservationRepository.InactiveInstanceName}");
                        await _processedObservationRepository.SetActiveInstanceAsync(_processedObservationRepository
                            .InActiveInstance);
                    }

                    if (mode == JobRunModes.Full)
                    {
                        if (_runIncrementalAfterFull)
                        {
                            // Enqueue incremental harvest/process job to Hangfire in order to get latest sightings

                            var jobId = BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunAsync(JobRunModes.IncrementalInactiveInstance,
                                cancellationToken));

                            _logger.LogInformation($"Incremental harvest/process job with Id={jobId} was enqueued");
                        }
                        
                        //----------------------------------------------------------------------------
                        // 9. End create DwC CSV files and merge the files into multiple DwC-A files.
                        //----------------------------------------------------------------------------
                        var dwcFiles = await _dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles();

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
                }

                _logger.LogInformation($"Processing done: {success} {mode}");

                //-------------------------------
                // 10. Return processing result
                //-------------------------------
                return success ? true : throw new Exception("Failed to process observations.");
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
        /// Process verbatim observations
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="mode"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="taxonById"></param>
        /// <param name="cleanStart"></param>
        /// <param name="processStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> ProcessVerbatim(IEnumerable<DataProvider> dataProvidersToProcess, JobRunModes mode, ObservationType observationType, IDictionary<int, Taxon> taxonById, bool cleanStart, DateTime processStart, bool copyFromActiveOnFail, IJobCancellationToken cancellationToken)
        {
            _processedObservationRepository.ObservationType = observationType;

            await InitializeElasticSearch(mode, cleanStart);

            _logger.LogInformation($"Start disable indexing ({_processedObservationRepository.IndexName})");
            await _processedObservationRepository.DisableIndexingAsync();
            _logger.LogInformation($"Finish disable indexing ({_processedObservationRepository.IndexName})");

            var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
            foreach (var dataProvider in dataProvidersToProcess)
            {
                if (!dataProvider.IsActive || 
                    (mode != JobRunModes.Full && !dataProvider.SupportIncrementalHarvest) || 
                    ((observationType == ObservationType.Protected || observationType == ObservationType.Diffused) && !dataProvider.SupportProtectedHarvest))
                {
                    continue;
                }

                var processor = _processorByType[dataProvider.Type];
                processTaskByDataProvider.Add(dataProvider,
                    processor.ProcessAsync(dataProvider, taxonById, observationType, mode, cancellationToken));
            }

            var success = (await Task.WhenAll(processTaskByDataProvider.Values)).All(t => t.Status == RunStatus.Success);
            
            if (mode == JobRunModes.Full)
            {
                //----------------------------------------------------------------------------
                //  If a data provider failed to process and it was not Artportalen,
                //     then try to copy that data from the active instance.
                //----------------------------------------------------------------------------

                var artportalenSuccededOrDidntRun = !processTaskByDataProvider.Any(pair =>
                    pair.Key.Type == DataProviderType.ArtportalenObservations &&
                    pair.Value.Result.Status == RunStatus.Failed);

                if (!success && copyFromActiveOnFail && artportalenSuccededOrDidntRun)
                {
                    var copyTasks = processTaskByDataProvider
                        .Where(t => t.Value.Result.Status == RunStatus.Failed)
                        .Select(t => _instanceManager.CopyProviderDataAsync(t.Key)).ToArray();

                    await Task.WhenAll(copyTasks);
                    success = copyTasks.All(t => t.Result);
                }
            }

            _logger.LogInformation($"Start enable indexing ({_processedObservationRepository.IndexName})");
            await _processedObservationRepository.EnableIndexingAsync();
            _logger.LogInformation($"Finish enable indexing ({_processedObservationRepository.IndexName})");

            await UpdateProcessInfoAsync(mode, processStart, processTaskByDataProvider, success);

            return success;
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
            var processInfo = (await GetProcessInfoAsync(new[]
            {
                   _processedObservationRepository.CurrentInstanceName
                })).FirstOrDefault();

            if (processInfo == null || mode == JobRunModes.Full)
            {
                var providersInfo = new List<ProviderInfo>();

                foreach (var taskProvider in processTaskByDataProvider)
                {
                    var provider = taskProvider.Key;
                    var processResult = taskProvider.Value.Result;

                    // Get harvest info and create a provider info object 
                    var harvestInfo = await GetHarvestInfoAsync(provider.Identifier);
                    var providerInfo = new ProviderInfo(provider)
                    {
                        HarvestCount = harvestInfo?.Count,
                        HarvestEnd = harvestInfo?.End,
                        HarvestStart = harvestInfo?.Start,
                        HarvestStatus = harvestInfo?.Status,
                        ProcessCount = processResult.Count,
                        ProcessEnd = processResult.End,
                        ProcessStart = processResult.Start,
                        ProcessStatus = processResult.Status
                    };

                    providersInfo.Add(providerInfo);
                }

                var metaDataProcessInfo = await GetProcessInfoAsync(new[]
                {
                        nameof(Lib.Models.Processed.Observation.Area),
                        nameof(Taxon)
                    });

                processInfo = new ProcessInfo(_processedObservationRepository.CurrentInstanceName, processStart)
                {
                    Count = processTaskByDataProvider.Sum(pi => pi.Value.Result.Count),
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
                    
                    // Get provider info and update incremental values
                    var providerInfo = processInfo.ProvidersInfo.FirstOrDefault(pi => pi.DataProviderId == provider.Id);
                    if (providerInfo != null)
                    {
                        providerInfo.LatestIncrementalCount = processResult.Count;
                        providerInfo.LatestIncrementalEnd = processResult.End;
                        providerInfo.LatestIncrementalStart = processResult.Start;
                        providerInfo.LatestIncrementalStatus = processResult.Status;
                    }
                }
            }

            _logger.LogInformation("Start updating process info for observations");
            await SaveProcessInfo(processInfo);
            _logger.LogInformation("Finish updating process info for observations");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="artportalenObservationProcessor"></param>
        /// <param name="clamPortalObservationProcessor"></param>
        /// <param name="fishDataObservationProcessor"></param>
        /// <param name="kulObservationProcessor"></param>
        /// <param name="mvmObservationProcessor"></param>
        /// <param name="norsObservationProcessor"></param>
        /// <param name="sersObservationProcessor"></param>
        /// <param name="sharkObservationProcessor"></param>
        /// <param name="virtualHerbariumObservationProcessor"></param>
        /// <param name="dwcaObservationProcessor"></param>
        /// <param name="taxonCache"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="instanceManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="processTaxaJob"></param>
        /// <param name="areaHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public ProcessJob(IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IArtportalenObservationProcessor artportalenObservationProcessor,
            IClamPortalObservationProcessor clamPortalObservationProcessor,
            IFishDataObservationProcessor fishDataObservationProcessor,
            IKulObservationProcessor kulObservationProcessor,
            IMvmObservationProcessor mvmObservationProcessor,
            INorsObservationProcessor norsObservationProcessor,
            ISersObservationProcessor sersObservationProcessor,
            ISharkObservationProcessor sharkObservationProcessor,
            IVirtualHerbariumObservationProcessor virtualHerbariumObservationProcessor,
            IDwcaObservationProcessor dwcaObservationProcessor,
            ICache<int, Taxon> taxonCache,
            ICache<int, DataProvider> dataProviderCache,
            IInstanceManager instanceManager,
            IValidationManager validationManager,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ProcessConfiguration processConfiguration,
            ILogger<ProcessJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _taxonCache = taxonCache ??
                          throw new ArgumentNullException(nameof(taxonCache));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _instanceManager = instanceManager ?? throw new ArgumentNullException(nameof(instanceManager));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));

            if (clamPortalObservationProcessor == null)
                throw new ArgumentNullException(nameof(clamPortalObservationProcessor));
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
            _processorByType = new Dictionary<DataProviderType, IProcessor>
            {
                {DataProviderType.ArtportalenObservations, artportalenObservationProcessor},
                {DataProviderType.ClamPortalObservations, clamPortalObservationProcessor},
                {DataProviderType.FishDataObservations, fishDataObservationProcessor},
                {DataProviderType.SersObservations, sersObservationProcessor},
                {DataProviderType.NorsObservations, norsObservationProcessor},
                {DataProviderType.KULObservations, kulObservationProcessor},
                {DataProviderType.MvmObservations, mvmObservationProcessor},
                {DataProviderType.SharkObservations, sharkObservationProcessor},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationProcessor},
                {DataProviderType.DwcA, dwcaObservationProcessor}
            };

            _exportContainer = processConfiguration?.Export_Container ??
                               throw new ArgumentNullException(nameof(processConfiguration));
            _runIncrementalAfterFull = processConfiguration.RunIncrementalAfterFull;
        }


        /// <inheritdoc />
        [DisplayName("Process Observations [Mode={1}]")]
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
                false,
                false,
                cancellationToken);
        }

        /// <inheritdoc />
        [DisplayName("Process verbatim observations for all active providers")]
        public async Task<bool> RunAsync(
            bool cleanStart,
            bool copyFromActiveOnFail,
            IJobCancellationToken cancellationToken)
        {
            _dataProviderCache.Clear();

            var dataProviders = await _dataProviderCache.GetAllAsync();
            var dataProvidersToProcess = dataProviders.Where(dataProvider => dataProvider.IsActive).ToList();
            return await RunAsync(
                dataProvidersToProcess,
                JobRunModes.Full,
                true,
                copyFromActiveOnFail,
                cancellationToken);
        }
    }
}