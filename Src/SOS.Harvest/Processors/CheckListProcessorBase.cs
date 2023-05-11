using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors
{
    public abstract class ChecklistProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
        where TVerbatim : IEntity<int>
        where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int> 
    {
        /// <summary>
        /// Commit batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="processedChecklists"></param>
        /// <param name="batchId"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<int> CommitBatchAsync(
            DataProvider dataProvider,
            ICollection<Checklist> processedChecklists,
            string batchId,
            byte attempt = 1)
        {
            try
            {
                Logger.LogDebug($"Checklist - Start storing {dataProvider.Identifier} batch: {batchId}");
                var processedCount =
                    await ProcessedChecklistRepository.AddManyAsync(processedChecklists);

                Logger.LogDebug($"Checklist - Finish storing {dataProvider.Identifier} batch: {batchId} ({processedCount})");

                return processedCount;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning(e, $"Checklist - Failed to commit batch: {batchId} for {dataProvider}, attempt: {attempt}");
                    System.Threading.Thread.Sleep(attempt * 200);
                    attempt++;
                    return await CommitBatchAsync(dataProvider, processedChecklists, batchId, attempt);
                }

                Logger.LogError(e, $"Checklist - Failed to commit batch:{batchId} for {dataProvider}");
                throw;
            }

        }

        /// <summary>
        ///  Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="checklistFactory"></param>
        /// <param name="checklistVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            IChecklistFactory<TVerbatim> checklistFactory,
            TVerbatimRepository checklistVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Checklist - Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var verbatimChecklistBatch = await checklistVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Checklist - Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");

                if (!verbatimChecklistBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Checklist - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");

                var checklists = new ConcurrentDictionary<string, Checklist>();

                foreach (var verbatimChecklist in verbatimChecklistBatch)
                {
                    var checklist = checklistFactory.CreateProcessedChecklist(verbatimChecklist);

                    if (checklist == null)
                    {
                        continue;
                    }

                    // Add checklist
                    checklists.TryAdd(checklist.Id, checklist);
                }
               
                Logger.LogDebug($"Checklist - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");

                return await ValidateAndStoreChecklists(dataProvider, checklists.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Checklist - Process {dataProvider.Identifier} checklists from id: {startId} to id: {endId} failed");
                throw;
            }
            finally
            {
                ProcessManager.Release();
            }
        }

        protected readonly IProcessedChecklistRepository ProcessedChecklistRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedChecklistRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="logger"></param>
        protected ChecklistProcessorBase(
            IProcessedChecklistRepository processedChecklistRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<TClass> logger) : base(processManager, processTimeManager, processConfiguration, logger)
        {
            ProcessedChecklistRepository = processedChecklistRepository ??
                                           throw new ArgumentNullException(nameof(processedChecklistRepository));
        }

        /// <summary>
        /// Method to override in parent class 
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<int> ProcessChecklistsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Parent class will use call this to process data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="checklistFactory"></param>
        /// <param name="checklistVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<int> ProcessChecklistsAsync(
            DataProvider dataProvider,
            IChecklistFactory<TVerbatim> checklistFactory,
            TVerbatimRepository checklistVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var startId = 1;
            var maxId = await checklistVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<int>>();

            while (startId <= maxId)
            {
                await ProcessManager.WaitAsync();

                var batchEndId = startId + 1000 - 1;
                processBatchTasks.Add(ProcessBatchAsync(
                    dataProvider,
                    startId,
                    batchEndId,
                    checklistFactory,
                    checklistVerbatimRepository,
                    cancellationToken));
                startId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        protected async Task<int> ValidateAndStoreChecklists(DataProvider dataProvider, ICollection<Checklist> checklists, string batchId)
        {
            if (!checklists?.Any() ?? true)
            {
                return 0;
            }

            var processedCount = await CommitBatchAsync(dataProvider, checklists, batchId);

            return processedCount;
        }

        public abstract DataProviderType Type { get; }

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
                Logger.LogDebug($"Checklist - Start processing {dataProvider.Identifier} checklists");
                var processCount = await ProcessChecklistsAsync(dataProvider, cancellationToken);

                Logger.LogInformation($"Checklist - Finish processing {dataProvider.Identifier} checklists.");

                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount, 0, 0);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider.Identifier} checklist processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider.Identifier} checklists");
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }
    }
}