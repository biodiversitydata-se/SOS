using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
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
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ProcessJob : ProcessJobBase, IProcessJob
    {
        private readonly IDwcArchiveFileWriterCoordinator _dwcArchiveFileWriterCoordinator;
        private readonly IAreaHelper _areaHelper;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IInstanceManager _instanceManager;
        private readonly IValidationManager _validationManager;
        private readonly ILogger<ProcessJob> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly Dictionary<DataProviderType, IProcessor> _processorByType;
        private readonly IProcessTaxaJob _processTaxaJob;

        private List<DataProvider> GetDataProvidersToProcess(List<string> dataProviderIdOrIdentifiers,
            List<DataProvider> allDataProviders)
        {
            return allDataProviders.Where(dataProvider =>
                    dataProviderIdOrIdentifiers.Any(dataProvider.EqualsIdOrIdentifier))
                .ToList();
        }

        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="type"></param>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="toggleInstanceOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(
            List<DataProvider> dataProvidersToProcess,
            JobRunModes mode,
            bool cleanStart,
            bool copyFromActiveOnFail,
            bool toggleInstanceOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                //-----------------
                // 1. Arrange
                //-----------------

                var processStart = DateTime.Now;
                _processedObservationRepository.Mode = mode;

                //-----------------
                // 2. Validation
                //-----------------
                if (!dataProvidersToProcess.Any())
                {
                    return false;
                }

                // Use current taxa if we are in incremental mode, to speed things up
                if (mode == JobRunModes.Full)
                {
                    //----------------------------------------------------------------------
                    // 3. Process taxa
                    //----------------------------------------------------------------------
                    _logger.LogInformation("Start processing taxonomy");

                    if (!await _processTaxaJob.RunAsync())
                    {
                        _logger.LogError("Failed to process taxonomy");
                        return false;
                    }

                    _logger.LogInformation("Finish processing taxonomy");
                }

                //--------------------------------------
                // 4. Get taxonomy
                //--------------------------------------
                _logger.LogInformation("Start getting processed taxa");
                var taxa = await _processedTaxonRepository.GetAllAsync();
                if (!taxa?.Any() ?? true)
                {
                    _logger.LogWarning("Failed to get processed taxa");
                    return false;
                }

                var taxonById = taxa.ToDictionary(m => m.Id, m => m);
                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogInformation("Finish getting processed taxa");

                //------------------------------------------------------------------------
                // 5. Ensure ElasticSearch (ES) index ProcessedObservation-{0/1} exists
                //    Also empty the MongoDb collection InvalidObservation-{0/1}
                //
                // If cleanStart == true => Always clear the ES index.
                // If cleanStart == false => Keep existing data. Just create ES index if it doesn't exist.
                //------------------------------------------------------------------------
                bool newCollection;
                if (cleanStart && mode == JobRunModes.Full)
                {
                    _logger.LogInformation(
                        $"Start clear ElasticSearch index: {_processedObservationRepository.IndexName}");
                    await _processedObservationRepository.ClearCollectionAsync();
                    newCollection = true;
                    _logger.LogInformation(
                        $"Finish clear ElasticSearch index: {_processedObservationRepository.IndexName}");
                }
                else
                {
                    _logger.LogInformation("Start ensure collection exists");
                    // Create ES index ProcessedObservation-{0/1} if it doesn't exist.
                    newCollection = await _processedObservationRepository.VerifyCollectionAsync();
                    _logger.LogInformation("Finish ensure collection exists");
                }

                cancellationToken?.ThrowIfCancellationRequested();

                //--------------------------------------
                // 6.  Verify MongoDB InvalidObservation-{0/1} collection.
                //--------------------------------------
                await _validationManager.VerifyCollectionAsync(mode);

                cancellationToken?.ThrowIfCancellationRequested();
                //--------------------------------------
                // 7. Get ProviderInfo
                //--------------------------------------
                var providerInfoByDataProvider = new Dictionary<DataProvider, ProviderInfo>();
                var metaDataProviderInfo = await GetProviderInfoAsync(new Dictionary<string, DataProviderType>
                {
                    {nameof(Area), DataProviderType.Areas},
                    {nameof(ProcessedTaxon), DataProviderType.Taxa}
                });

                //------------------------------------------------------------------------
                // 8. Create observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                if (mode == JobRunModes.Full)
                {
                    _dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
                }

                var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
                foreach (var dataProvider in dataProvidersToProcess)
                {
                    var processor = _processorByType[dataProvider.Type];
                    processTaskByDataProvider.Add(dataProvider,
                        processor.ProcessAsync(dataProvider, taxonById, mode, cancellationToken));

                    // Get harvest info and create a provider info object that we can add processing info to later
                    var harvestInfoId = HarvestInfo.GetIdFromDataProvider(dataProvider);
                    var harvestInfo = await GetHarvestInfoAsync(harvestInfoId); // todo - decide where we should store harvestInfo
                    var providerInfo = CreateProviderInfo(dataProvider, harvestInfo, processStart);
                    providerInfo.MetadataInfo = metaDataProviderInfo
                        .Where(mdp => new[] { DataProviderType.Taxa }.Contains(mdp.DataProviderType)).ToArray();
                    providerInfoByDataProvider.Add(dataProvider, providerInfo);
                }

                var processingResult = await Task.WhenAll(processTaskByDataProvider.Values);
                var success = processingResult.All(t => t.Status == RunStatus.Success);

                // Don't create a dwc file in incremental mode
                if (mode == JobRunModes.Full)
                {
                    //----------------------------------------------------------------------------
                    // 9. End create DwC CSV files and merge the files into multiple DwC-A files.
                    //----------------------------------------------------------------------------
                    await _dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles();
                }

                //----------------------------------------------
                // 10. Update provider info 
                //----------------------------------------------
                foreach (var task in processTaskByDataProvider)
                {
                    var vi = providerInfoByDataProvider[task.Key];
                    vi.ProcessCount = mode == JobRunModes.Full ? task.Value.Result.Count : vi.ProcessCount + task.Value.Result.Count;
                    vi.ProcessEnd = task.Value.Result.End;
                    vi.ProcessStart = task.Value.Result.Start;
                    vi.ProcessStatus = task.Value.Result.Status;
                }

                //-----------------------------------------
                // 11. Save process info
                //-----------------------------------------
                _logger.LogInformation("Start updating process info for observations");
                foreach (var dataProvider in dataProvidersToProcess)
                {
                    await _dataProviderManager.UpdateProcessInfo(
                        dataProvider.Id,
                        _processedObservationRepository.InactiveInstanceName,
                        providerInfoByDataProvider[dataProvider]);
                }

                await SaveProcessInfo(
                    _processedObservationRepository.InactiveInstanceName,
                    processStart,
                    providerInfoByDataProvider.Sum(pi => pi.Value.ProcessCount ?? 0),
                    success ? RunStatus.Success : RunStatus.Failed,
                    providerInfoByDataProvider.Values);
                _logger.LogInformation("Finish updating process info for observations");

                if (mode == JobRunModes.Full)
                {
                    //----------------------------------------------------------------------------
                    // 12. If a data provider failed to process and it was not Artportalen,
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

                //---------------------------------
                // 13. Create ElasticSearch index
                //---------------------------------
                if (success)
                {
                    if (newCollection)
                    {
                        _logger.LogInformation("Start creating indexes");
                        await _processedObservationRepository.CreateIndexAsync();
                        _logger.LogInformation("Finish creating indexes");
                    }

                    if (mode == JobRunModes.Full)
                    {
                        // Enqueue incremental harvest/process job to Hangfire in order to get latest sightings
                        var jobId = BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunAsync(JobRunModes.IncrementalInactiveInstance,
                            cancellationToken));

                        _logger.LogInformation($"Incremental harvest/process job Job with Id={jobId} was enqueued");
                    }

                    if (toggleInstanceOnSuccess)
                    {
                        _logger.LogInformation("Toggle instance");
                        await _processedObservationRepository.SetActiveInstanceAsync(_processedObservationRepository
                            .InActiveInstance);
                    }
                }

                _logger.LogInformation($"Processing done: {success}");

                //------------------------
                // 14. Store area cache
                //------------------------
                _logger.LogDebug("Persist area cache");
                _areaHelper.PersistCache();

                //-------------------------------
                // 15. Return processing result
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
        /// <param name="processedTaxonRepository"></param>
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
            IProcessedTaxonRepository processedTaxonRepository,
            IDataProviderManager dataProviderManager,
            IInstanceManager instanceManager,
            IValidationManager validationManager,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ILogger<ProcessJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _processedTaxonRepository = processedTaxonRepository ??
                                        throw new ArgumentNullException(nameof(processedTaxonRepository));
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
        }


        /// <inheritdoc />
        public async Task<bool> RunAsync(
        List<string> dataProviderIdOrIdentifiers,
        JobRunModes mode,
        IJobCancellationToken cancellationToken)
        {
            return await RunAsync(dataProviderIdOrIdentifiers, mode, mode == JobRunModes.Full, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            JobRunModes mode,
            bool toggleInstanceOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            var allDataProviders = await _dataProviderManager.GetAllDataProvidersAsync();
            List<DataProvider> dataProvidersToProcess;
            if (dataProviderIdOrIdentifiers != null && dataProviderIdOrIdentifiers.Count > 0)
            {
                dataProvidersToProcess = GetDataProvidersToProcess(dataProviderIdOrIdentifiers, allDataProviders);
            }
            else
            {
                dataProvidersToProcess = allDataProviders;
            }

            return await RunAsync(
                dataProvidersToProcess,
                mode,
                mode == JobRunModes.Full,
                false,
                toggleInstanceOnSuccess,
            cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            bool cleanStart,
            bool copyFromActiveOnFail,
            bool toggleInstanceOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            var dataProviders = await _dataProviderManager.GetAllDataProvidersAsync();
            var dataProvidersToProcess = dataProviders.Where(dataProvider => dataProvider.IsActive).ToList();
            return await RunAsync(
                dataProvidersToProcess,
                JobRunModes.Full,
                true,
                copyFromActiveOnFail,
                toggleInstanceOnSuccess,
                cancellationToken);
        }
    }
}