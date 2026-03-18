using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Collections.Concurrent;

namespace SOS.Harvest.Processors.Mvm
{
    /// <summary>
    /// MVM event processor.
    /// </summary>
    public class MvmEventProcessor : EventProcessorBase<MvmEventProcessor, MvmSampleVerbatim, IVerbatimRepositoryBase<MvmSampleVerbatim, int>>,
        IMvmEventProcessor
    {        
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        public override DataProviderType Type => DataProviderType.MvmObservations;

        public MvmEventProcessor(          
            IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,           
            IEventRepository processedEventRepository,
            IAreaHelper areaHelper,
            IVocabularyRepository vocabularyRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<MvmEventProcessor> logger) :
                base(processedEventRepository, processManager, processTimeManager, validationManager, processConfiguration, logger)
        {
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository;          
            _areaHelper = areaHelper;
            _vocabularyRepository = vocabularyRepository;
        }

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {            
            MvmEventFactory mvmEventFactory = await MvmEventFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            return await ProcessEventsAsync(
                dataProvider,
                mvmEventFactory,
                _mvmObservationVerbatimRepository,
                cancellationToken);
        }

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            IEventFactory<MvmSampleVerbatim> eventFactory,
            IVerbatimRepositoryBase<MvmSampleVerbatim, int> eventVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug("Event - Start fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
                var verbatimEventsBatch = await eventVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug("Event - Finish fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
                if (verbatimEventsBatch == null || verbatimEventsBatch.Count() == 0)
                {
                    Logger.LogError("Event batch is Empty, {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
                    return (0, 0, 0);
                }

                Logger.LogDebug("Event - Start processing {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
                var processedEvents = new ConcurrentDictionary<string, Event>();
                foreach (var verbatimEvent in verbatimEventsBatch)
                {
                    string verbatimEventId = $"Mvm:Event:{verbatimEvent.SampleId}";
                    if (!processedEvents.ContainsKey(verbatimEventId))
                    {
                        var processedEvent = eventFactory.CreateEventObservation(verbatimEvent);
                        if (processedEvent == null)
                        {
                            Logger.LogInformation("Processed event is null, EventId: {@eventId})", verbatimEventId);
                            continue;
                        }
                        processedEvents.TryAdd(processedEvent.EventId, processedEvent);
                    }
                }

                //await UpdateEventOccurrencesAsync(processedEvents, dataProvider);
                Logger.LogDebug("Event - Finish processing {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
                return await ValidateAndStoreEvents(dataProvider, processedEvents.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Event - Process {@dataProvider} event from id: {@batchStartId} to id: {@batchEndId} failed", dataProvider.Identifier, startId, endId);
                throw;
            }
            finally
            {
                ProcessManager.Release($"{dataProvider}, Batch={startId}-{endId}");
            }
        }
    }
}