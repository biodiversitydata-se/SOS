using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Event;

namespace SOS.Harvest.Processors
{
    public abstract class EventProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
        where TVerbatim : IEntity<int>
        where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int> 
    {
        protected readonly IObservationEventRepository ObservationEventRepository;
        public abstract DataProviderType Type { get; }

        /// <summary>
        /// Method to override in parent class 
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<int> ProcessEventsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationEventRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="logger"></param>
        protected EventProcessorBase(
            IObservationEventRepository observationEventRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<TClass> logger) : base(processManager, processTimeManager, processConfiguration, logger)
        {
            ObservationEventRepository = observationEventRepository ??
                                           throw new ArgumentNullException(nameof(observationEventRepository));
        }

        /// <summary>
        /// Process observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            try
            {
                Logger.LogDebug($"Event - Start processing {dataProvider.Identifier} events");
                var processCount = await ProcessEventsAsync(dataProvider, cancellationToken);
                Logger.LogInformation($"Event - Finish processing {dataProvider.Identifier} events.");
                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount, 0, 0);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider.Identifier} event processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider.Identifier} events");
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }

        /// <summary>
        ///  Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="eventFactory"></param>
        /// <param name="eventVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            IEventFactory<TVerbatim> eventFactory,
            TVerbatimRepository eventVerbatimRepository,
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
                    processedEvents.TryAdd(processedEvent.Id, processedEvent);
                }
               
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

        /// <summary>
        /// Commit batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="processedEvents"></param>
        /// <param name="batchId"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<int> CommitBatchAsync(
            DataProvider dataProvider,
            ICollection<ObservationEvent> processedEvents,
            string batchId,
            byte attempt = 1)
        {
            try
            {
                Logger.LogDebug($"Event - Start storing {dataProvider.Identifier} batch: {batchId}");
                var processedCount = await ObservationEventRepository.AddManyAsync(processedEvents);
                Logger.LogDebug($"Event - Finish storing {dataProvider.Identifier} batch: {batchId} ({processedCount})");
                return processedCount;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning(e, $"Event - Failed to commit batch: {batchId} for {dataProvider}, attempt: {attempt}");
                    System.Threading.Thread.Sleep(attempt * 200);
                    attempt++;
                    return await CommitBatchAsync(dataProvider, processedEvents, batchId, attempt);
                }

                Logger.LogError(e, $"Event - Failed to commit batch:{batchId} for {dataProvider}");
                throw;
            }
        }

        /// <summary>
        /// Parent class will use call this to process data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="eventFactory"></param>
        /// <param name="eventVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<int> ProcessEventsAsync(
            DataProvider dataProvider,
            IEventFactory<TVerbatim> eventFactory,
            TVerbatimRepository eventVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var startId = 1;
            var maxId = await eventVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<int>>();

            while (startId <= maxId)
            {
                await ProcessManager.WaitAsync();

                var batchEndId = startId + 1000 - 1;
                processBatchTasks.Add(ProcessBatchAsync(
                    dataProvider,
                    startId,
                    batchEndId,
                    eventFactory,
                    eventVerbatimRepository,
                    cancellationToken));
                startId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);
            return processBatchTasks.Sum(t => t.Result);
        }

        protected async Task<int> ValidateAndStoreEvents(DataProvider dataProvider, ICollection<ObservationEvent> events, string batchId)
        {
            if (!events?.Any() ?? true) return 0;
            var processedCount = await CommitBatchAsync(dataProvider, events, batchId);
            return processedCount;
        }   
    }
}