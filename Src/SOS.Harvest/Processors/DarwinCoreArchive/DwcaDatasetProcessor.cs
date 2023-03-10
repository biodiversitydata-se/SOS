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
using SOS.Lib.Models.Processed.DataStewardship.Event;
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
        private readonly IEventRepository _observationEventRepository;

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
            _observationEventRepository = observationEventRepository ?? throw new ArgumentNullException(nameof(observationEventRepository));
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

                foreach (var verbatimDataset in datasetsBatch)
                {
                    var dataset = datasetFactory.CreateProcessedDataset(verbatimDataset);

                    if (dataset == null)
                    {
                        continue;
                    }

                    // Add dataset
                    datasets.TryAdd(dataset.Id.ToString(), dataset);
                }

                await GetDatasetEvents(datasets);

                Logger.LogDebug($"Dataset - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");
                return await ValidateAndStoreDatasets(dataProvider, datasets.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
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

        private async Task GetDatasetEvents(ConcurrentDictionary<string, Dataset> processedDatasets)
        {
            // todo get events.
            // implement ProcessedObservationCoreRepository.GetAllAggregationItemsListAsync() in ObservationEventRepository


            //var filter = new SearchFilter(0);
            //filter.DataStewardshipDatasetIds = processedDatasets.Keys.ToList();
            //var eventIds = await _observationEventRepository.GetAllAggregationItemsAsync(filter, "event.eventId");
            ////return eventIds?.Select(m => m.AggregationKey).ToList();

            //var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
            //var occurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId, m => m.OccurrenceIds);
            //foreach (var eventPair in processedDatasets)
            //{
            //    if (occurrenceIdsByEventId.TryGetValue(eventPair.Key, out var occurrenceIds))
            //    {
            //        if (occurrenceIds != null && eventPair.Value.OccurrenceIds != null && occurrenceIds.Count != eventPair.Value.OccurrenceIds.Count)
            //            Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed={occurrenceIds.Count}");
            //        eventPair.Value.OccurrenceIds = occurrenceIds;
            //    }
            //    else
            //    {
            //        if (eventPair.Value.OccurrenceIds != null && eventPair.Value.OccurrenceIds.Count > 0)
            //            Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed=0");
            //        eventPair.Value.OccurrenceIds = null;
            //    }
            //}
        }
    }
}