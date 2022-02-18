using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive.Interfaces;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Jobs
{
    /// <inheritdoc cref="IProcessCheckListsJob"/>
    public class ProcessCheckListsJob : ProcessJobBase, IProcessCheckListsJob
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly IProcessedCheckListRepository _processedCheckListRepository;
        private readonly ILogger<ProcessCheckListsJob> _logger;

        private readonly Dictionary<DataProviderType, ICheckListProcessor> _processorByType;


        private async Task InitializeAreaHelperAsync()
        {
            _logger.LogDebug("Start initialize area cache");
            await _areaHelper.InitializeAsync();
            _logger.LogDebug("Finish initialize area cache");
        }

        private async Task InitializeElasticSearchAsync()
        {
            _logger.LogInformation(
                $"Start clear ElasticSearch index: {_processedCheckListRepository.UniqueIndexName}");
            await _processedCheckListRepository.ClearCollectionAsync();

            _logger.LogInformation(
                $"Finish clear ElasticSearch index: {_processedCheckListRepository.UniqueIndexName}");
        }

        /// <summary>
        /// Disable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task DisableIndexingAsync()
        {
            _logger.LogInformation($"Start disable indexing ({_processedCheckListRepository.UniqueIndexName})");
            await _processedCheckListRepository.DisableIndexingAsync();
            _logger.LogInformation($"Finish disable indexing ({_processedCheckListRepository.UniqueIndexName})");
        }

        /// <summary>
        /// Enable Elasticsearch indexing
        /// </summary>
        /// <returns></returns>
        private async Task EnableIndexingAsync()
        {
            _logger.LogInformation($"Start enable indexing ({_processedCheckListRepository.UniqueIndexName})");
            await _processedCheckListRepository.EnableIndexingAsync();
            _logger.LogInformation($"Finish enable indexing ({_processedCheckListRepository.UniqueIndexName})");
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
                _processedCheckListRepository.LiveMode = false;

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
                // 4. Create check lists processing tasks, and wait for them to complete
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
                    _logger.LogInformation($"Toggle instance {_processedCheckListRepository.ActiveInstance} => {_processedCheckListRepository.InActiveInstance}");
                    await _processedCheckListRepository.SetActiveInstanceAsync(_processedCheckListRepository
                        .InActiveInstance);
                }

                _logger.LogInformation($"Processing done: {success}");

                //-------------------------------
                // 8. Return processing result
                //-------------------------------
                return success ? true : throw new Exception("Failed to process check lists.");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process check lists job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Process check lists job failed.");
                throw new Exception("Process check lists job failed.");
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
                var harvestInfo = await GetHarvestInfoAsync(provider.CheckListIdentifier);
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

                var processInfo = new ProcessInfo(_processedCheckListRepository.UniqueIndexName, processStart)
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
        /// <param name="processedCheckListRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="artportalenCheckListProcessor"></param>
        /// <param name="dwcaCheckListProcessor"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessCheckListsJob(IProcessedCheckListRepository processedCheckListRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IArtportalenCheckListProcessor artportalenCheckListProcessor,
            IDwcaCheckListProcessor dwcaCheckListProcessor,
            IDataProviderCache dataProviderCache,
            IAreaHelper areaHelper,
            ILogger<ProcessCheckListsJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedCheckListRepository = processedCheckListRepository ??
                                            throw new ArgumentNullException(nameof(processedCheckListRepository));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));

            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (artportalenCheckListProcessor == null)
                throw new ArgumentNullException(nameof(artportalenCheckListProcessor));

            _processorByType = new Dictionary<DataProviderType, ICheckListProcessor>
            {
                {DataProviderType.ArtportalenObservations, artportalenCheckListProcessor},
                {DataProviderType.DwcA, dwcaCheckListProcessor}
            };
        }

        /// <inheritdoc />
        [DisplayName("Process Check Lists")]
        public async Task<bool> RunAsync(
            IEnumerable<string> dataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            var checkListDataProviders = (await _dataProviderCache.GetAllAsync()).Where(dp => dp.IsActive && dp.SupportCheckLists);

            if (!checkListDataProviders?.Any() ?? true)
            {
                return false;
            }

            IEnumerable<DataProvider> dataProvidersToProcess;
            if (dataProviderIdOrIdentifiers?.Any() ?? false)
            {
                dataProvidersToProcess = checkListDataProviders.Where(dataProvider =>
                        dataProviderIdOrIdentifiers.Any(dataProvider.EqualsIdOrIdentifier))
                    .ToArray();
            }
            else
            {
                dataProvidersToProcess = checkListDataProviders
                    .ToArray();
            }

            return await RunAsync(
                dataProvidersToProcess,
                cancellationToken);
        }
    }
}