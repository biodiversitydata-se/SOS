using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Harvest.Processors.Interfaces;
using Hangfire.Server;
using System.Collections.Concurrent;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A dataset processor.
    /// </summary>
    public class DwcaDatasetProcessor : DatasetProcessorBase<DwcaDatasetProcessor, DwcVerbatimDataset, IVerbatimRepositoryBase<DwcVerbatimDataset, int>>,
        IDwcaDatasetProcessor
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IEventRepository _eventRepository;        

        public override DataProviderType Type => DataProviderType.DwcA;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedDatasetsRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>        
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcaDatasetProcessor(
            IVerbatimClient verbatimClient,
            IEventRepository observationEventRepository,
            IDatasetRepository processedDatasetsRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,            
            ProcessConfiguration processConfiguration,
            ILogger<DwcaDatasetProcessor> logger) :
                base(processedDatasetsRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _eventRepository = observationEventRepository ?? throw new ArgumentNullException(nameof(observationEventRepository));
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessDatasetsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(
                dataProvider,
                _verbatimClient,
                Logger);

            var datasetFactory = new DwcaDatasetFactory(dataProvider, TimeManager, ProcessConfiguration);

            return await base.ProcessDatasetsAsync(
                dataProvider,
                datasetFactory,
                dwcCollectionRepository.DatasetRepository,
                cancellationToken);
        }

        protected override async Task<int> ProcessBatchAsync(
            DataProvider dataProvider, 
            int startId, 
            int endId, IDatasetFactory<DwcVerbatimDataset> datasetFactory, 
            IVerbatimRepositoryBase<DwcVerbatimDataset, int> datasetVerbatimRepository, 
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Datasets - Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var datasetsBatch = await datasetVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Datasets - Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");

                if (!datasetsBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Dataset - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");
                var datasets = new ConcurrentDictionary<string, Dataset>();

                foreach (var verbatimDataset in datasetsBatch!)
                {
                    var dataset = datasetFactory.CreateProcessedDataset(verbatimDataset);
                    CheckData(verbatimDataset, dataset, dataProvider);
                    if (dataset == null)
                    {
                        continue;
                    }

                    // Add dataset
                    datasets.TryAdd(dataset.Identifier.ToString(), dataset);
                }

                await UpdateDatasetEventsAsync(datasets, dataProvider);                
                Logger.LogDebug($"Dataset - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");
                return await ValidateAndStoreDatasets(dataProvider, datasets.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Datasets - Process {dataProvider.Identifier} datasets from id: {startId} to id: {endId} failed");
                throw;
            }
            finally
            {
                ProcessManager.Release();
            }
        }

        private void CheckData(DwcVerbatimDataset verbatimDataset, Dataset? dataset, DataProvider dataProvider)
        {
            try
            {
                if (verbatimDataset == null) Logger.LogWarning($"verbatimDataset is null for {dataProvider}");
                if (dataset == null) Logger.LogWarning($"dataset is null for {dataProvider}");

                if (verbatimDataset != null)
                {
                    if (verbatimDataset.EventIds == null || verbatimDataset.EventIds.Count == 0)
                    {
                        Logger.LogWarning($"verbatimDataset contains no eventIds for {dataProvider}");
                    }
                }

                if (dataset != null)
                {
                    if (dataset.EventIds == null || dataset.EventIds.Count == 0)
                    {
                        Logger.LogWarning($"dataset contains no eventIds for {dataProvider}");
                    }
                }
            }
            catch { }            
        }

        private async Task UpdateDatasetEventsAsync(ConcurrentDictionary<string, Dataset> processedDatasets, DataProvider dataProvider)
        {
            var processedEventsByDatasetId = await GetDatasetEventsDictionaryAsync(processedDatasets, dataProvider);
            foreach (var datasetPair in processedDatasets)
            {
                if (processedEventsByDatasetId.TryGetValue(datasetPair.Key.ToLower(), out var eventIds))
                {
                    datasetPair.Value.VerbatimEventIds = datasetPair.Value.EventIds;
                    datasetPair.Value.EventIds = eventIds;
                }
            }
        }

        private async Task<Dictionary<string, List<string>>> GetDatasetEventsDictionaryAsync(ConcurrentDictionary<string, Dataset> processedDatasets, DataProvider dataProvider)
        {
            var filter = new EventSearchFilter();
            filter.DatasetIds = processedDatasets.Keys.ToList();
            var datasetEventIds = await _eventRepository.GetAllAggregationItemsListAsync<string, string>(filter, "dataStewardship.datasetIdentifier", "eventId");
            Dictionary<string, List<string>> eventIdsByDatasetId = datasetEventIds.ToDictionary(m => m.AggregationKey.ToLower(), m => m.Items);            
            DebugLogEventIdsByDatasetId(processedDatasets, eventIdsByDatasetId, _eventRepository.IndexName);

            foreach (var datasetPair in processedDatasets)
            {
                if (eventIdsByDatasetId.TryGetValue(datasetPair.Key.ToLower(), out var eventIds))
                {
                    if (eventIds != null && datasetPair.Value.EventIds != null && eventIds.Count != datasetPair.Value.EventIds.Count)
                        Logger.LogInformation($"Dataset.EventIds differs. #Verbatim={datasetPair.Value.EventIds.Count}, #Processed={eventIds.Count}, DatasetId={datasetPair.Key}, DataProvider={dataProvider}");
                    datasetPair.Value.EventIds = eventIds;
                }
                else
                {
                    eventIdsByDatasetId.Add(datasetPair.Key, new List<string>());
                    if (datasetPair.Value.EventIds != null && datasetPair.Value.EventIds.Count > 0)
                        Logger.LogInformation($"Dataset.EventIds differs. #Verbatim={datasetPair.Value.EventIds.Count}, #Processed=0, DatasetId={datasetPair.Key}, DataProvider={dataProvider}");
                    datasetPair.Value.EventIds = null;
                }
            }

            return eventIdsByDatasetId;
        }

        private void DebugLogEventIdsByDatasetId(ConcurrentDictionary<string, Dataset> processedDatasets, Dictionary<string, List<string>> eventIdsByDatasetId, string indexName)
        {
            Logger.LogDebug($"GetDatasetEventsDictionaryAsync() executed GetAllAggregationItemsListAsync() with the following parameters: IndexName={indexName}, DatasetIds filter: {string.Join(", ", processedDatasets.Keys.ToList())}");
            if (eventIdsByDatasetId == null)
            {
                Logger.LogWarning($"eventIdsByDatasetId is null");
            }
            else
            {
                Logger.LogDebug("eventIdsByDatasetId have the following values:");

                foreach (var pair in eventIdsByDatasetId)
                {
                    string strVal = "null";
                    if (pair.Value != null)
                    {
                        strVal = $"{pair.Value.Count} items";
                    }
                    Logger.LogDebug($"Key=\"{pair.Key}\", Value={strVal}");
                }
            }
        }
    }
}