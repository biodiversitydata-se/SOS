using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Configuration.Process;
using DnsClient.Internal;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Extensions;
using System.Text.Json;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Artportalen event processor.
    /// </summary>
    public class ArtportalenEventProcessor : EventProcessorBase<ArtportalenEventProcessor, ArtportalenObservationVerbatim, IVerbatimRepositoryBase<ArtportalenObservationVerbatim, int>>,
        IArtportalenEventProcessor
    {
        private readonly IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> _artportalenVerbatimRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenEventProcessor(
            //IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            //IArtportalenVerbatimRepository artportalenVerbatimRepository // observation verbatim repository
            IProcessedObservationCoreRepository processedObservationRepository,            
            IEventRepository observationEventRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, validationManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;            
        }

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {            
            var processCount = await AddObservationEventsAsync(dataProvider);
            return processCount;
        }

        private async Task<(int publicCount, int protectedCount, int failedCount)> AddObservationEventsAsync(DataProvider dataProvider)
        {
            try
            {
                Logger.LogInformation("Start AddObservationEventsAsync()");                
                int batchSize = 5000;
                var filter = new SearchFilter(0);
                filter.IsPartOfDataStewardshipDataset = true;
                filter.DataProviderIds = new List<int> { 1 };
                Logger.LogInformation($"AddObservationEventsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
                var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
                Dictionary<string, List<string>> totalOccurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId.ToLower(), m => m.OccurrenceIds, StringComparer.OrdinalIgnoreCase);
                var chunks = totalOccurrenceIdsByEventId.Chunk(batchSize);
                (int publicCount, int protectedCount, int failedCount) eventCountSum = new(0, 0, 0);

                foreach (KeyValuePair<string, List<string>>[] chunk in chunks) // todo - do this step in parallel
                {
                    int nrErrors = 0;
                    Dictionary<string, List<string>> occurrenceIdsByEventId = chunk.ToDictionary(m => m.Key, m => m.Value, StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, string> eventIdByOccurrenceId = CreateEventIdByOccurrenceIdDictionary(occurrenceIdsByEventId);
                    var firstOccurrenceIdInEvents = occurrenceIdsByEventId.Select(m => m.Value.First());
                    var observations = await _processedObservationRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
                    var events = new List<Event>();
                    foreach (var observation in observations)
                    {
                        string eventId = observation.Event.EventId.ToLower();
                        if (occurrenceIdsByEventId.TryGetValue(eventId, out var occurrenceIds))
                        {
                            var eventModel = observation.ToEvent(occurrenceIds, dataProvider.Id);
                            events.Add(eventModel);
                        }
                        else
                        {
                            if (nrErrors == 0)
                            {
                                string expectedEventId = eventIdByOccurrenceId.GetValueOrDefault(observation.Occurrence.OccurrenceId.ToLower(), "NotFound");
                                Logger.LogError($"Couldnt find the following event in occurrenceIdsByEventId: EventId={eventId}. OccurrenceId={observation.Occurrence.OccurrenceId}. ExpectedEventId={expectedEventId}. The following eventIds exists in the dictionary: {JsonSerializer.Serialize(occurrenceIdsByEventId.Keys)}");
                            }
                            nrErrors++;
                        }
                    }

                    if (nrErrors > 0)
                    {
                        Logger.LogInformation($"Number of errors in AddObservationEventsAsync={nrErrors}");
                    }

                    // write to ES
                    var eventCount = await ValidateAndStoreEvents(dataProvider, events, "");                    
                    eventCountSum.publicCount += eventCount.publicCount;
                    eventCountSum.protectedCount += eventCount.protectedCount;
                    eventCountSum.failedCount += eventCount.failedCount;
                }

                Logger.LogInformation("End AddObservationEventsAsync()");
                return eventCountSum;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Add data stewardship events failed.");
                return (0, 0, 0);
            }
        }

        private static Dictionary<string, string> CreateEventIdByOccurrenceIdDictionary(Dictionary<string, List<string>> occurrenceIdsByEventId)
        {
            var eventIdByOccurrenceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in occurrenceIdsByEventId)
            {
                foreach (var occurrenceId in pair.Value)
                {
                    eventIdByOccurrenceId.TryAdd(occurrenceId.ToLower(), pair.Key);
                }
            }

            return eventIdByOccurrenceId;
        }

        private readonly List<string> _observationEventOutputFields = new List<string>()
        {
            "occurrence",
            "location",
            "event",
            "dataStewardshipDatasetId",
            "institutionCode",
        };
    }
}