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
            IObservationEventRepository observationEventRepository,
            IAreaHelper areaHelper,
            IVocabularyRepository vocabularyRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<DwcaEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient;
            _processedObservationRepository = processedObservationRepository;
            _areaHelper = areaHelper;
            _vocabularyRepository = vocabularyRepository;
        }

        protected override async Task<int> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, Logger);
            DwcaEventFactory dwcaEventFactory = await DwcaEventFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessEventsAsync(
                dataProvider,
                dwcaEventFactory,
                dwcCollectionRepository.EventRepository,
                cancellationToken);

            // Artportalen implementation
            //int nrAddedEvents = await AddObservationEventsAsync(dataProvider);
            //return nrAddedEvents;

            //using var dwcArchiveVerbatimRepository = new EventOccurrenceDarwinCoreArchiveVerbatimRepository(
            //    dataProvider,
            //    _verbatimClient,
            //    Logger);

            //var checklistFactory = await DwcaChecklistFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            //return await base.ProcessChecklistsAsync(
            //    dataProvider,
            //    checklistFactory,
            //    dwcArchiveVerbatimRepository,
            //    cancellationToken);
        }

        protected override async Task<int> ProcessBatchAsync(
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
                if (!verbatimEventsBatch?.Any() ?? true) return 0;

                Logger.LogDebug($"Event - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");
                var processedEvents = new ConcurrentDictionary<string, ObservationEvent>();
                foreach (var verbatimEvent in verbatimEventsBatch)
                {
                    var processedEvent = eventFactory.CreateEventObservation(verbatimEvent);
                    if (processedEvent == null) continue;
                    processedEvents.TryAdd(processedEvent.EventId, processedEvent);
                }

                await GetEventObservations(processedEvents);
                Logger.LogDebug($"Event - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");
                return await ValidateAndStoreEvents(dataProvider, processedEvents.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
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

        private async Task GetEventObservations(ConcurrentDictionary<string, ObservationEvent> processedEvents)
        {
            var filter = new SearchFilter(0);
            //filter.IsPartOfDataStewardshipDataset = true;
            filter.EventIds = processedEvents.Keys.ToList();
            var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);
            var occurrenceIdsByEventId = eventOccurrenceIds.ToDictionary(m => m.EventId.ToLower(), m => m.OccurrenceIds);
            foreach (var eventPair in processedEvents)
            {
                if (occurrenceIdsByEventId.TryGetValue(eventPair.Key.ToLower(), out var occurrenceIds))
                {
                    if (occurrenceIds != null && eventPair.Value.OccurrenceIds != null && occurrenceIds.Count != eventPair.Value.OccurrenceIds.Count)
                        Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed={occurrenceIds.Count}, EventId={eventPair.Key}");
                    eventPair.Value.OccurrenceIds = occurrenceIds;
                }
                else
                {
                    if (eventPair.Value.OccurrenceIds != null && eventPair.Value.OccurrenceIds.Count > 0)
                        Logger.LogInformation($"Event.OccurrenceIds differs. #Verbatim={eventPair.Value.OccurrenceIds.Count}, #Processed=0, EventId={eventPair.Key}");
                    eventPair.Value.OccurrenceIds = null;
                }
            }
        }

        //protected override async Task<int> ProcessBatchAsync(
        //    DataProvider dataProvider, 
        //    int startId, 
        //    int endId, 
        //    IEventFactory<DwcEventOccurrenceVerbatim> eventFactory, 
        //    IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int> eventVerbatimRepository, 
        //    IJobCancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        cancellationToken?.ThrowIfCancellationRequested();
        //        Logger.LogDebug($"Event - Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
        //        var verbatimEventsBatch = await eventVerbatimRepository.GetBatchAsync(startId, endId);
        //        Logger.LogDebug($"Event - Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");
        //        if (!verbatimEventsBatch?.Any() ?? true) return 0;

        //        Logger.LogDebug($"Event - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");
        //        var processedEvents = new ConcurrentDictionary<string, ObservationEvent>();
        //        foreach (var verbatimEvent in verbatimEventsBatch)
        //        {
        //            var processedEvent = eventFactory.CreateEventObservation(verbatimEvent);
        //            if (processedEvent == null) continue;
        //            processedEvents.TryAdd(processedEvent.Id, processedEvent);
        //        }

        //        CheckIfOcurrencesExists(processedEvents);
        //        Logger.LogDebug($"Event - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");
        //        return await ValidateAndStoreEvents(dataProvider, processedEvents.Values, $"{startId}-{endId}");
        //    }
        //    catch (JobAbortedException e)
        //    {
        //        // Throw cancelation again to let function above handle it
        //        throw;
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError(e, $"Event - Process {dataProvider.Identifier} event from id: {startId} to id: {endId} failed");
        //        throw;
        //    }
        //    finally
        //    {
        //        ProcessManager.Release();
        //    }
        //}

        //private static void CheckIfOcurrencesExists(ConcurrentDictionary<string, ObservationEvent> processedEvents)
        //{
        //    // Check if processed observations exists. This is a naive implementation and can probably be improved.
        //    HashSet<string> occurrenceIds = new HashSet<string>();
        //    foreach (var ev in processedEvents.Values)
        //    {
        //        if (ev.OccurrenceIds != null)
        //        {
        //            foreach (var occurrenceId in ev.OccurrenceIds)
        //            {
        //                occurrenceIds.Add(occurrenceId);
        //            }
        //        }
        //    }

        //    //HashSet<string> notFoundOccurrenceIds = _processedObservationRepository. // todo - get all not found occurrenceIds.
        //    HashSet<string> notFoundOccurrenceIds = new HashSet<string>();
        //    if (notFoundOccurrenceIds.Count > 0)
        //    {
        //        foreach (var ev in processedEvents.Values)
        //        {
        //            if (ev.OccurrenceIds != null)
        //            {
        //                ev.OccurrenceIds = ev.OccurrenceIds.Except(notFoundOccurrenceIds).ToList();
        //            }
        //        }
        //    }
        //}

        // Artportalen implementation
        //private async Task<int> AddObservationEventsAsync(DataProvider dataProvider)
        //{            
        //    Logger.LogInformation("Start AddObservationEventsAsync()");
        //    int batchSize = 5000;
        //    var filter = new SearchFilter(0);
        //    filter.IsPartOfDataStewardshipDataset = true;
        //    Logger.LogInformation($"AddObservationEventsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
        //    var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);   
        //}        

    }
}