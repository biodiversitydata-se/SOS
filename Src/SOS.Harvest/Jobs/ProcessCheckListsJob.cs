using System.ComponentModel;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <inheritdoc cref="IProcessChecklistsJob"/>
    public class ProcessChecklistsJob : ProcessJobBase, IProcessChecklistsJob
    {
        private readonly IAreaHelper _areaHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly IProcessedChecklistRepository _processedChecklistRepository;
        private readonly ILogger<ProcessChecklistsJob> _logger;

        private readonly Dictionary<DataProviderType, IChecklistProcessor> _processorByType;


        private async Task InitializeAreaHelperAsync()
        {
            _logger.LogDebug("Start initialize area cache");
            await _areaHelper.InitializeAsync();
            _logger.LogDebug("Finish initialize area cache");
        }

        private async Task InitializeElasticSearchAsync()
        {
            _logger.LogInformation(
                $"Start clear ElasticSearch index: {_processedChecklistRepository.UniqueIndexName}");
            await _processedChecklistRepository.ClearCollectionAsync();

            _logger.LogInformation(
                $"Finish clear ElasticSearch index: {_processedChecklistRepository.UniqueIndexName}");
        }

        /// <summary>
        /// Disable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task DisableIndexingAsync()
        {
            _logger.LogInformation($"Start disable indexing ({_processedChecklistRepository.UniqueIndexName})");
            await _processedChecklistRepository.DisableIndexingAsync();
            _logger.LogInformation($"Finish disable indexing ({_processedChecklistRepository.UniqueIndexName})");
        }

        /// <summary>
        /// Enable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task EnableIndexingAsync()
        {
            _logger.LogInformation($"Start enable indexing ({_processedChecklistRepository.UniqueIndexName})");
            await _processedChecklistRepository.EnableIndexingAsync();
            _logger.LogInformation($"Finish enable indexing ({_processedChecklistRepository.UniqueIndexName})");
        }

        /// <summary>
        ///  Run process job
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(
            IEnumerable<DataProvider> dataProvidersToProcess,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                //-----------------
                // 1. Arrange
                //-----------------
                _processedChecklistRepository.LiveMode = false;

                //-----------------
                // 2. Validation
                //-----------------
                if (!dataProvidersToProcess.Any())
                {
                    return false;
                }

                //-----------------
                // 3. Initialization
                //-----------------
                await InitializeAreaHelperAsync();

                // Init indexes
                await InitializeElasticSearchAsync();

                // Disable indexing for public and protected index
                await DisableIndexingAsync();

                //------------------------------------------------------------------------
                // 4. Create checklists processing tasks, and wait for them to complete
                //------------------------------------------------------------------------
                var success = await ProcessVerbatim(dataProvidersToProcess, cancellationToken);

                //---------------------------------
                // 5. On success enable indexing and switch instance
                //---------------------------------
                if (success)
                {
                    // Enable indexing 
                    await EnableIndexingAsync();

                    // Toggle active instance if we are done
                    _logger.LogInformation($"Toggle instance {_processedChecklistRepository.ActiveInstance} => {_processedChecklistRepository.InActiveInstance}");
                    await _processedChecklistRepository.SetActiveInstanceAsync(_processedChecklistRepository
                        .InActiveInstance);

                    // Clear processed configuration cache in observation API
                    _logger.LogInformation($"Start clear processed configuration cache at search api");
                    await _cacheManager.ClearAsync(Cache.ProcessedConfiguration);
                    _logger.LogInformation($"Finish clear processed configuration cache at search api");
                }

                _logger.LogInformation($"Processing done: {success}");

                //-------------------------------
                // 8. Return processing result
                //-------------------------------
                return success ? true : throw new Exception("Failed to process checklists.");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process checklists job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Process checklists job failed.");
                throw new Exception("Process checklists job failed.");
            }
        }

        /// <summary>
        /// Process verbatim observations
        /// </summary>
        /// <param name="dataProvidersToProcess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> ProcessVerbatim(IEnumerable<DataProvider> dataProvidersToProcess, IJobCancellationToken cancellationToken)
        {
            var processStart = DateTime.Now;

            var processTaskByDataProvider = new Dictionary<DataProvider, Task<ProcessingStatus>>();
            foreach (var dataProvider in dataProvidersToProcess)
            {
                if (!dataProvider.IsActive)
                {
                    continue;
                }

                var processor = _processorByType[dataProvider.Type];
                processTaskByDataProvider.Add(dataProvider,
                    processor.ProcessAsync(dataProvider, cancellationToken));
            }

            var success = (await Task.WhenAll(processTaskByDataProvider.Values)).All(t => t.Status == RunStatus.Success);

            await UpdateProcessInfoAsync(processStart, processTaskByDataProvider, success);
            return success;
        }

        /// <summary>
        /// Update process info
        /// </summary>
        /// <param name="processStart"></param>
        /// <param name="processTaskByDataProvider"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        private async Task UpdateProcessInfoAsync(
            DateTime processStart,
            IDictionary<DataProvider,
                Task<ProcessingStatus>> processTaskByDataProvider,
            bool success)
        {

            if (!processTaskByDataProvider?.Any() ?? true)
            {
                return;
            }

            var providersInfo = new List<ProviderInfo>();

            foreach (var taskProvider in processTaskByDataProvider)
            {
                var provider = taskProvider.Key;
                var processResult = taskProvider.Value.Result;

                if (processResult == null)
                {
                    continue;
                }

                // Get harvest info and create a provider info object 
                var harvestInfo = await GetHarvestInfoAsync(provider.ChecklistIdentifier);
                var providerInfo = new ProviderInfo(provider)
                {
                    HarvestCount = harvestInfo?.Count,
                    HarvestEnd = harvestInfo?.End,
                    HarvestNotes = harvestInfo?.Notes,
                    HarvestStart = harvestInfo?.Start,
                    HarvestStatus = harvestInfo?.Status,
                    PublicProcessCount = processResult.PublicCount,
                    ProtectedProcessCount = processResult.ProtectedCount,
                    ProcessEnd = processResult.End,
                    ProcessStart = processResult.Start,
                    ProcessStatus = processResult.Status
                };

                providersInfo.Add(providerInfo);

                var processInfo = new ProcessInfo(_processedChecklistRepository.UniqueIndexName, processStart)
                {
                    PublicCount = processTaskByDataProvider.Sum(pi => pi.Value.Result.PublicCount),
                    End = DateTime.Now,
                    MetadataInfo = null,
                    ProvidersInfo = providersInfo,
                    Status = success ? RunStatus.Success : RunStatus.Failed
                };

                _logger.LogInformation("Start updating process info for observations");
                await SaveProcessInfo(processInfo);
                _logger.LogInformation("Finish updating process info for observations");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedChecklistRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="artportalenChecklistProcessor"></param>
        /// <param name="dwcaChecklistProcessor"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="areaHelper"></param>
        /// <param name="cacheManager"></param>
        /// <param name="logger"></param>
        public ProcessChecklistsJob(IProcessedChecklistRepository processedChecklistRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IArtportalenChecklistProcessor artportalenChecklistProcessor,
            IDwcaChecklistProcessor dwcaChecklistProcessor,
            IDataProviderCache dataProviderCache,
            IAreaHelper areaHelper,
            ICacheManager cacheManager,
            ILogger<ProcessChecklistsJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedChecklistRepository = processedChecklistRepository ??
                                            throw new ArgumentNullException(nameof(processedChecklistRepository));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));

            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (artportalenChecklistProcessor == null)
                throw new ArgumentNullException(nameof(artportalenChecklistProcessor));

            _processorByType = new Dictionary<DataProviderType, IChecklistProcessor>
            {
                {DataProviderType.ArtportalenObservations, artportalenChecklistProcessor},
                {DataProviderType.DwcA, dwcaChecklistProcessor}
            };
        }

        /// <inheritdoc />
        [DisplayName("Process Check Lists")]
        public async Task<bool> RunAsync(
            IEnumerable<string> dataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            var checklistDataProviders = (await _dataProviderCache.GetAllAsync()).Where(dp => dp.IsActive && dp.SupportChecklists);

            if (!checklistDataProviders?.Any() ?? true)
            {
                _logger.LogInformation("No data providers support checklists");
                return false;
            }

            IEnumerable<DataProvider> dataProvidersToProcess;
            if (dataProviderIdOrIdentifiers?.Any() ?? false)
            {
                dataProvidersToProcess = checklistDataProviders.Where(dataProvider =>
                        dataProviderIdOrIdentifiers.Any(dataProvider.EqualsIdOrIdentifier))
                    .ToArray();
            }
            else
            {
                dataProvidersToProcess = checklistDataProviders
                    .ToArray();
            }

            return await RunAsync(
                dataProvidersToProcess,
                cancellationToken);
        }
    }
}