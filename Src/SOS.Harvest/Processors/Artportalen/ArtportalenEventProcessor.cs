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
            IObservationEventRepository observationEventRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;            
        }

        protected override async Task<int> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {            
            int nrAddedEvents = await AddObservationEventsAsync(dataProvider);
            return nrAddedEvents;

            //var eventFactory = new ArtportalenEventFactory(dataProvider, TimeManager, ProcessConfiguration);
            //return await base.ProcessEventsAsync(
            //    dataProvider,
            //    eventFactory,
            //    null, //IArtportalenVerbatimRepository artportalenVerbatimRepository
            //    cancellationToken);
        }

        private async Task<int> AddObservationEventsAsync(DataProvider dataProvider)
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
                Dictionary<string, List<string>> totalOccurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId.ToLower(), m => m.OccurrenceIds);
                var chunks = totalOccurrenceIdsByEventId.Chunk(batchSize);
                int eventCount = 0;

                foreach (var chunk in chunks) // todo - do this step in parallel
                {
                    var occurrenceIdsByEventId = chunk.ToDictionary(m => m.Key, m => m.Value);
                    var firstOccurrenceIdInEvents = occurrenceIdsByEventId.Select(m => m.Value.First());
                    var observations = await _processedObservationRepository.GetObservationsAsync(firstOccurrenceIdInEvents, _observationEventOutputFields, false);
                    var events = new List<ObservationEvent>();
                    foreach (var observation in observations)
                    {                        
                        var occurrenceIds = occurrenceIdsByEventId[observation.Event.EventId.ToLower()];
                        var eventModel = observation.ToObservationEvent(occurrenceIds);
                        events.Add(eventModel);
                    }

                    // write to ES
                    eventCount += await ValidateAndStoreEvents(dataProvider, events, "");                    
                }

                Logger.LogInformation("End AddObservationEventsAsync()");
                return eventCount;                
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Add data stewardship events failed.");
                return 0;
            }
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