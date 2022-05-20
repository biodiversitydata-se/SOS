using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;

namespace SOS.Harvest.Processors
{
    public abstract class CheckListProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
        where TVerbatim : IEntity<int>
        where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int> 
    {
        /// <summary>
        /// Commit batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="processedCheckLists"></param>
        /// <param name="batchId"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<int> CommitBatchAsync(
            DataProvider dataProvider,
            ICollection<CheckList> processedCheckLists,
            string batchId,
            byte attempt = 1)
        {
            try
            {
                Logger.LogDebug($"Checklist - Start storing {dataProvider.Identifier} batch: {batchId}");
                var processedCount =
                    await ProcessedCheckListRepository.AddManyAsync(processedCheckLists);

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
                    return await CommitBatchAsync(dataProvider, processedCheckLists, batchId, attempt);
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
        /// <param name="checkListFactory"></param>
        /// <param name="checkListVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            ICheckListFactory<TVerbatim> checkListFactory,
            TVerbatimRepository checkListVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Checklist - Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var verbatimCheckListBatch = await checkListVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Checklist - Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");

                if (!verbatimCheckListBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Checklist - Start processing {dataProvider.Identifier} batch ({startId}-{endId})");

                var checkLists = new ConcurrentDictionary<string, CheckList>();

                foreach (var verbatimCheckList in verbatimCheckListBatch)
                {
                    var checkList = checkListFactory.CreateProcessedCheckList(verbatimCheckList);

                    if (checkList == null)
                    {
                        continue;
                    }

                    // Add checklist
                    checkLists.TryAdd(checkList.Id, checkList);
                }
               
                Logger.LogDebug($"Checklist - Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");

                return await ValidateAndStoreCheckLists(dataProvider, checkLists.Values, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Checklist - Process {dataProvider.Identifier} check lists from id: {startId} to id: {endId} failed");
                throw;
            }
            finally
            {
                ProcessManager.Release();
            }
        }

        protected readonly IProcessedCheckListRepository ProcessedCheckListRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedCheckListRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="logger"></param>
        protected CheckListProcessorBase(
            IProcessedCheckListRepository processedCheckListRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ILogger<TClass> logger) : base(processManager, processTimeManager, logger)
        {
            ProcessedCheckListRepository = processedCheckListRepository ??
                                           throw new ArgumentNullException(nameof(processedCheckListRepository));
        }

        /// <summary>
        /// Method to override in parent class 
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<int> ProcessCheckListsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Parent class will use call this to process data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="checkListFactory"></param>
        /// <param name="checkListVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<int> ProcessCheckListsAsync(
            DataProvider dataProvider,
            ICheckListFactory<TVerbatim> checkListFactory,
            TVerbatimRepository checkListVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var startId = 1;
            var maxId = await checkListVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<int>>();

            while (startId <= maxId)
            {
                await ProcessManager.WaitAsync();

                var batchEndId = startId + 1000 - 1;
                processBatchTasks.Add(ProcessBatchAsync(
                    dataProvider,
                    startId,
                    batchEndId,
                    checkListFactory,
                    checkListVerbatimRepository,
                    cancellationToken));
                startId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        protected async Task<int> ValidateAndStoreCheckLists(DataProvider dataProvider, ICollection<CheckList> checkLists, string batchId)
        {
            if (!checkLists?.Any() ?? true)
            {
                return 0;
            }

            var processedCount = await CommitBatchAsync(dataProvider, checkLists, batchId);

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
                Logger.LogDebug($"Checklist - Start processing {dataProvider.Identifier} check lists");
                var processCount = await ProcessCheckListsAsync(dataProvider, cancellationToken);

                Logger.LogInformation($"Checklist - Finish processing {dataProvider.Identifier} check lists.");

                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount, 0, 0);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider.Identifier} check list processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider.Identifier} check lists");
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }
    }
}