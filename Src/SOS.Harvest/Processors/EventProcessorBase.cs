using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Collections.Concurrent;

namespace SOS.Harvest.Processors;

public abstract class EventProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
    where TVerbatim : IEntity<int>
    where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int>
{
    protected readonly IEventRepository ObservationEventRepository;
    protected readonly IValidationManager ValidationManager;
    public abstract DataProviderType Type { get; }

    /// <summary>
    /// Method to override in parent class 
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<(int publicCount, int protectedCount, int failedCount)> ProcessEventsAsync(
        DataProvider dataProvider,
        IJobCancellationToken cancellationToken);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="observationEventRepository"></param>
    /// <param name="processManager"></param>
    /// <param name="logger"></param>
    protected EventProcessorBase(
        IEventRepository observationEventRepository,
        IProcessManager processManager,
        IProcessTimeManager processTimeManager,
        IValidationManager validationManager,
        ProcessConfiguration processConfiguration,
        ILogger<TClass> logger) : base(processManager, processTimeManager, processConfiguration, logger)
    {
        ObservationEventRepository = observationEventRepository ??
                                       throw new ArgumentNullException(nameof(observationEventRepository));
        ValidationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
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
            Logger.LogDebug("Event - Start processing {@dataProvider} events", dataProvider.Identifier);
            var processCount = await ProcessEventsAsync(dataProvider, cancellationToken);
            Logger.LogInformation("Event - Finish processing {@dataProvider} events. publicCount={processCount.publicCount}, protectedCount={processCount.protectedCount}, failedCount={processCount.failedCount}", dataProvider.Identifier, processCount.publicCount, processCount.protectedCount, processCount.failedCount);

            return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount.publicCount, processCount.protectedCount, processCount.failedCount);
        }
        catch (JobAbortedException)
        {
            Logger.LogInformation("{@dataProvider} event processing was canceled.", dataProvider.Identifier);
            return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to process {@dataProvider} events", dataProvider.Identifier);
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
    protected virtual async Task<(int publicCount, int protectedCount, int failedCount)> ProcessBatchAsync(
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
            Logger.LogDebug("Event - Start fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
            var verbatimEventsBatch = await eventVerbatimRepository.GetBatchAsync(startId, endId);
            Logger.LogDebug("Event - Finish fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
            if (!verbatimEventsBatch?.Any() ?? true) return (0, 0, 0);

            Logger.LogDebug("Event - Start processing {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
            var processedEvents = new ConcurrentDictionary<string, Event>();
            foreach (var verbatimEvent in verbatimEventsBatch!)
            {
                var processedEvent = eventFactory.CreateEventObservation(verbatimEvent);
                if (processedEvent == null) continue;
                processedEvents.TryAdd(processedEvent.EventId, processedEvent);
            }

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
        ICollection<Event> processedEvents,
        string batchId,
        byte attempt = 1)
    {
        try
        {
            Logger.LogDebug("Event - Start storing {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);
            var processedCount = await ObservationEventRepository.AddManyAsync(processedEvents);
            Logger.LogDebug("Event - Finish storing {@dataProvider} batch: {@batchId} ({@eventProcessCount})", dataProvider.Identifier, batchId, processedCount);
            return processedCount;
        }
        catch (Exception e)
        {
            if (attempt < 3)
            {
                Logger.LogWarning(e, "Event - Failed to commit batch: {@batchId} for {@dataProvider}, attempt: " + attempt, batchId, dataProvider.Identifier);
                System.Threading.Thread.Sleep(attempt * 200);
                attempt++;
                return await CommitBatchAsync(dataProvider, processedEvents, batchId, attempt);
            }

            Logger.LogError(e, "Event - Failed to commit batch:{@batchId} for {@dataProvider}", batchId, dataProvider.Identifier);
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
    protected async Task<(int publicCount, int protectedCount, int failedCount)> ProcessEventsAsync(
        DataProvider dataProvider,
        IEventFactory<TVerbatim> eventFactory,
        TVerbatimRepository eventVerbatimRepository,
        IJobCancellationToken cancellationToken)
    {
        var startId = 1;
        var maxId = await eventVerbatimRepository.GetMaxIdAsync();
        var processBatchTasks = new List<Task<(int publicCount, int protectedCount, int failedCount)>>();

        while (startId <= maxId)
        {
            var batchEndId = startId + 1000 - 1;
            const int maxRetries = 3;
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                if (await ProcessManager.WaitAsync($"{dataProvider}, Batch={startId}-{batchEndId}"))
                {
                    break;
                }

                retryCount++;
                Logger.LogWarning("Attempt {retryCount} failed for {dataProvider} event batch {startId}-{batchEndId}. Retrying...", dataProvider, retryCount, startId, batchEndId);
                if (retryCount == maxRetries)
                {
                    Logger.LogError("Failed to process {dataProvider} event batch {startId}-{batchEndId} after {maxRetries} attempts.", dataProvider, startId, batchEndId, maxRetries);
                    throw new InvalidOperationException($"Failed to process event batch {startId}-{batchEndId} after {maxRetries} attempts.");
                }
            }
            
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
        var results = processBatchTasks
            .Where(t => t.Status == TaskStatus.RanToCompletion)
            .Select(t => t.Result)
            .ToList();

        return (
            results.Sum(r => r.publicCount),
            0,
            results.Sum(r => r.failedCount)
        );
    }

    protected async Task<(int publicCount, int protectedCount, int failedCount)> ValidateAndStoreEvents(DataProvider dataProvider, ICollection<Event> events, string batchId)
    {
        if (!events?.Any() ?? true) return (0, 0, 0);

        var preValidationCount = events!.Count;
        events = await ValidateAndRemoveInvalidEvents(dataProvider, events, batchId);

        if (!events?.Any() ?? true)
        {
            return (0, 0, preValidationCount);
        }

        var processedCount = await CommitBatchAsync(dataProvider, events!, batchId);

        events!.Clear();
        return (processedCount, 0, preValidationCount - processedCount);
    }



    protected async Task<ICollection<Event>> ValidateAndRemoveInvalidEvents(
        DataProvider dataProvider,
        ICollection<Event> events,
        string batchId)
    {
        Logger.LogDebug("Start events validation {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);

        var invalidEvents = ValidationManager.ValidateEvents(ref events, dataProvider);
        await ValidationManager.AddInvalidEventsToDb(invalidEvents);

        Logger.LogDebug("End events validation {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);

        return events;
    }
}