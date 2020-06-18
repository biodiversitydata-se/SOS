using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Mvm.Interfaces;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Processors.Shark.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
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
        private readonly ICopyFieldMappingsJob _copyFieldMappingsJob;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IInstanceManager _instanceManager;
        private readonly ILogger<ProcessJob> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly Dictionary<DataProviderType, IProcessor> _processorByType;
        private readonly IProcessTaxaJob _processTaxaJob;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalObservationProcessor"></param>
        /// <param name="kulObservationProcessor"></param>
        /// <param name="mvmObservationProcessor"></param>
        /// <param name="norsObservationProcessor"></param>
        /// <param name="sersObservationProcessor"></param>
        /// <param name="sharkObservationProcessor"></param>
        /// <param name="virtualHerbariumObservationProcessor"></param>
        /// <param name="artportalenObservationProcessor"></param>
        /// <param name="dwcaObservationProcessor"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="instanceManager"></param>
        /// <param name="copyFieldMappingsJob"></param>
        /// <param name="processTaxaJob"></param>
        /// <param name="areaHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public ProcessJob(IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalObservationProcessor clamPortalObservationProcessor,
            IKulObservationProcessor kulObservationProcessor,
            IMvmObservationProcessor mvmObservationProcessor,
            INorsObservationProcessor norsObservationProcessor,
            ISersObservationProcessor sersObservationProcessor,
            ISharkObservationProcessor sharkObservationProcessor,
            IVirtualHerbariumObservationProcessor virtualHerbariumObservationProcessor,
            IArtportalenObservationProcessor artportalenObservationProcessor,
            IDwcaObservationProcessor dwcaObservationProcessor,
            IDataProviderManager dataProviderManager,
            IProcessedTaxonRepository processedTaxonRepository,
            IInstanceManager instanceManager,
            ICopyFieldMappingsJob copyFieldMappingsJob,
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
            _copyFieldMappingsJob =
                copyFieldMappingsJob ?? throw new ArgumentNullException(nameof(copyFieldMappingsJob));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _instanceManager = instanceManager ?? throw new ArgumentNullException(nameof(instanceManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));

            if (clamPortalObservationProcessor == null)
                throw new ArgumentNullException(nameof(clamPortalObservationProcessor));
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
                {DataProviderType.SersObservations, sersObservationProcessor},
                {DataProviderType.NorsObservations, norsObservationProcessor},
                {DataProviderType.KULObservations, kulObservationProcessor},
                {DataProviderType.MvmObservations, mvmObservationProcessor},
                {DataProviderType.SharkObservations, sharkObservationProcessor},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationProcessor},
                {DataProviderType.DwcA, dwcaObservationProcessor}
            };
        }

        public async Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            bool cleanStart,
            bool copyFromActiveOnFail,
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
                cleanStart,
                copyFromActiveOnFail,
                toggleInstanceOnSuccess,
                cancellationToken);
        }

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
                cleanStart,
                copyFromActiveOnFail,
                toggleInstanceOnSuccess,
                cancellationToken);
        }

        private List<DataProvider> GetDataProvidersToProcess(List<string> dataProviderIdOrIdentifiers,
            List<DataProvider> allDataProviders)
        {
            return allDataProviders.Where(dataProvider =>
                    dataProviderIdOrIdentifiers.Any(dataProvider.EqualsIdOrIdentifier))
                .ToList();
        }

        public async Task<bool> RunAsync(
            List<DataProvider> dataProvidersToProcess,
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

                //-----------------
                // 2. Validation
                //-----------------
                if (!dataProvidersToProcess.Any())
                {
                    return false;
                }

                //----------------------------------------------------------------------
                // 3. Copy field mappings and taxa from sos-verbatim to sos-processed
                //----------------------------------------------------------------------
                _logger.LogInformation("Start copying taxonomy and fieldmapping from verbatim to processed db");
                var metadataTasks = new[]
                {
                    _copyFieldMappingsJob.RunAsync(),
                    _processTaxaJob.RunAsync()
                };
                await Task.WhenAll(metadataTasks);
                if (!metadataTasks.All(t => t.Result))
                {
                    _logger.LogError("Failed to copy taxonomy and fieldmapping from verbatim to processed db");
                    return false;
                }

                _logger.LogInformation("Finish copying taxonomy and fieldmapping from verbatim to processed db");


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
                if (cleanStart)
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
                    // Create ES index ProcessedObservation-{0/1} if it doesn't exist. Empty MongoDB InvalidObservation-{0/1} collection.
                    newCollection = await _processedObservationRepository.VerifyCollectionAsync();
                    _logger.LogInformation("Finish ensure collection exists");
                }

                cancellationToken?.ThrowIfCancellationRequested();

                //--------------------------------------
                // 6. Get ProviderInfo
                //--------------------------------------
                var providerInfoByDataProvider = new Dictionary<DataProvider, ProviderInfo>();
                var metaDataProviderInfo = await GetProviderInfoAsync(new Dictionary<string, DataProviderType>
                {
                    {nameof(Area), DataProviderType.Areas},
                    {nameof(ProcessedTaxon), DataProviderType.Taxa}
                });

                //------------------------------------------------------------------------
                // 7. Create observation processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                _dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
                var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
                foreach (var dataProvider in dataProvidersToProcess)
                {
                    var processor = _processorByType[dataProvider.Type];
                    processTaskByDataProvider.Add(dataProvider,
                        processor.ProcessAsync(dataProvider, taxonById, cancellationToken));

                    // Get harvest info and create a provider info object that we can add processing info to later
                    var harvestInfoId = HarvestInfo.GetIdFromDataProvider(dataProvider);
                    var harvestInfo = await GetHarvestInfoAsync(harvestInfoId);
                    var harvestInfo2 = dataProvider.HarvestInfo; // todo - decide where we should store harvestInfo
                    var providerInfo = CreateProviderInfo(dataProvider, harvestInfo, processStart);
                    providerInfo.MetadataInfo = metaDataProviderInfo
                        .Where(mdp => new[] {DataProviderType.Taxa}.Contains(mdp.DataProviderType)).ToArray();
                    providerInfoByDataProvider.Add(dataProvider, providerInfo);
                }

                var processingResult = await Task.WhenAll(processTaskByDataProvider.Values);
                var success = processTaskByDataProvider.Values.All(t => t.Result.Status == RunStatus.Success);

                //----------------------------------------------------------------------------
                // 8. End create DwC CSV files and merge the files into multiple DwC-A files.
                //----------------------------------------------------------------------------
                await _dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles();
                _dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();

                //----------------------------------------------
                // 9. Update provider info 
                //----------------------------------------------
                foreach (var task in processTaskByDataProvider)
                {
                    var vi = providerInfoByDataProvider[task.Key];
                    vi.ProcessCount = task.Value.Result.Count;
                    vi.ProcessEnd = task.Value.Result.End;
                    vi.ProcessStart = task.Value.Result.Start;
                    vi.ProcessStatus = task.Value.Result.Status;
                }

                //-----------------------------------------
                // 10. Save process info
                //-----------------------------------------
                _logger.LogInformation("Start updating process info for observations");
                foreach (var dataProvider in dataProvidersToProcess)
                {
                    await _dataProviderManager.UpdateProcessInfo(
                        dataProvider.Id,
                        _processedObservationRepository.InactiveCollectionName,
                        providerInfoByDataProvider[dataProvider]);
                }

                await SaveProcessInfo(
                    _processedObservationRepository.InactiveCollectionName,
                    processStart,
                    providerInfoByDataProvider.Sum(pi => pi.Value.ProcessCount ?? 0),
                    success ? RunStatus.Success : RunStatus.Failed,
                    providerInfoByDataProvider.Values);
                _logger.LogInformation("Finish updating process info for observations");

                //----------------------------------------------------------------------------
                // 11. If a data provider failed to process and it was not Artportalen,
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

                //---------------------------------
                // 12. Create ElasticSearch index
                //---------------------------------
                if (success)
                {
                    if (newCollection)
                    {
                        _logger.LogInformation("Start creating indexes");
                        await _processedObservationRepository.CreateIndexAsync();
                        _logger.LogInformation("Finish creating indexes");
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
                // 13. Store area cache
                //------------------------
                _logger.LogDebug("Persist area cache");
                _areaHelper.PersistCache();

                //-------------------------------
                // 14. Return processing result
                //-------------------------------
                return success ? true : throw new Exception("Process sightings job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Process sightings job failed");
                throw new Exception("Process sightings job failed");
            }
            finally
            {
                _dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();
            }
        }

        private string GetHarvestInfoId(DataProvider dataProvider)
        {
            switch (dataProvider.Type)
            {
                case DataProviderType.DwcA:
                    return $"{nameof(DwcObservationVerbatim)}-{dataProvider.Identifier}";
                default:
                    return dataProvider.Type.ToString();
            }
        }
    }
}