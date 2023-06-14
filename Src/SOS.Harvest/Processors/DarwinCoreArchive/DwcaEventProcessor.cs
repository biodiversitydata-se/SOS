using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Search.Filters;
using Hangfire.Server;
using SOS.Harvest.Processors.Interfaces;
using System.Collections.Concurrent;
using MongoDB.Driver;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A event processor.
    /// </summary>
    public class DwcaEventProcessor : EventProcessorBase<DwcaEventProcessor, DwcEventOccurrenceVerbatim, IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>>,
        IDwcaEventProcessor
    {
        private readonly IVerbatimClient _verbatimClient;        
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _vocabularyRepository;
        public override DataProviderType Type => DataProviderType.DwcA;

        public DwcaEventProcessor(
            //IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            //IArtportalenVerbatimRepository artportalenVerbatimRepository // observation verbatim repository
            IVerbatimClient verbatimClient,
            IProcessedObservationCoreRepository processedObservationRepository,
            IEventRepository observationEventRepository,
            IAreaHelper areaHelper,
            IVocabularyRepository vocabularyRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<DwcaEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, validationManager, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient;
            _processedObservationRepository = processedObservationRepository;
            _areaHelper = areaHelper;
            _vocabularyRepository = vocabularyRepository;
        }

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, Logger);
            DwcaEventFactory dwcaEventFactory = await DwcaEventFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessEventsAsync(
                dataProvider,
                dwcaEventFactory,
                dwcCollectionRepository.EventRepository,
                cancellationToken);
        }

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            IEventFactory<DwcEventOccurrenceVerbatim> eventFactory,
            IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int> eventVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Event - Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var verbatimEventsBatch = await eventVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Event - Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                if (verbatimEventsBatch == null || verbatimEventsBatch.Count() == 0)
                {
                    Logger.LogError($"Event batch is Empty, {dataProvider.Identifier} batch ({startId}-{endId})");
                    return (0, 0, 0);
                }

                Logger.LogDebug($"Event - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");
                var processedEvents = new ConcurrentDictionary<string, Event>();
                foreach (var verbatimEvent in verbatimEventsBatch)
                {
                    var processedEvent = eventFactory.CreateEventObservation(verbatimEvent);
                    if (processedEvent == null)
                    {                        
                        Logger.LogInformation($"Processed event is null, EventId: {verbatimEvent.EventID})");
                        continue;
                    }
                    processedEvents.TryAdd(processedEvent.EventId, processedEvent);
                }

                await UpdateEventOccurrencesAsync(processedEvents, dataProvider);
                Logger.LogDebug($"Event - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");
                return await ValidateAndStoreEvents(dataProvider, processedEvents.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Event - Process {dataProvider.Identifier} event from id: {startId} to id: {endId} failed");
                throw;
            }
            finally
            {
                ProcessManager.Release();
            }
        }

        private async Task UpdateEventOccurrencesAsync(ConcurrentDictionary<string, Event> processedEvents, DataProvider dataProvider)
        {
            var processedOccurrencesByEventId = await GetEventObservationsDictionaryAsync(processedEvents, dataProvider);
            foreach (var eventPair in processedEvents)
            {
                if (processedOccurrencesByEventId.TryGetValue(eventPair.Key.ToLower(), out var occurrenceIds))
                {
                    eventPair.Value.VerbatimOccurrenceIds = eventPair.Value.OccurrenceIds;
                    eventPair.Value.OccurrenceIds = occurrenceIds;
                }
            }
        }

        private async Task<Dictionary<string, List<string>>> GetEventObservationsDictionaryAsync(ConcurrentDictionary<string, Event> processedEvents, DataProvider dataProvider)
        {
            var filter = new SearchFilter(0);
            //filter.IsPartOfDataStewardshipDataset = true;
            filter.EventIds = processedEvents.Keys.ToList();
            List<Lib.Models.Search.Result.EventOccurrenceAggregationItem> eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
            Dictionary<string, List<string>> occurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId.ToLower(), m => m.OccurrenceIds);
            foreach (var eventPair in processedEvents)
            {                
                if (occurrenceIdsByEventId.TryGetValue(eventPair.Key.ToLower(), out var occurrenceIds))
                {
                    if (occurrenceIds != null && eventPair.Value.OccurrenceIds != null && occurrenceIds.Count != eventPair.Value.OccurrenceIds.Count)
                        Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed={occurrenceIds.Count}, EventId={eventPair.Key}, DataProvider={dataProvider}");
                    eventPair.Value.OccurrenceIds = occurrenceIds;
                }
                else
                {
                    occurrenceIdsByEventId.Add(eventPair.Key, new List<string>());
                    if (eventPair.Value.OccurrenceIds != null && eventPair.Value.OccurrenceIds.Count > 0)
                        Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed=0, EventId={eventPair.Key}, DataProvider={dataProvider}");
                    eventPair.Value.OccurrenceIds = null;
                }
            }

            return occurrenceIdsByEventId;
        }
    }
}