using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Collections.Concurrent;

namespace SOS.Harvest.Processors;

public abstract class DatasetProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
    where TVerbatim : IEntity<int>
    where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int>
{
    protected IDatasetRepository DatasetRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="observationDatasetRepository"></param>
    /// <param name="processManager"></param>
    /// <param name="logger"></param>
    protected DatasetProcessorBase(
        IDatasetRepository observationDatasetRepository,
        IProcessManager processManager,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        ILogger<TClass> logger) : base(processManager, processTimeManager, processConfiguration, logger)
    {
        DatasetRepository = observationDatasetRepository ??
                                       throw new ArgumentNullException(nameof(observationDatasetRepository));
    }

    /// <summary>
    /// Commit batch
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="processedDatasets"></param>
    /// <param name="batchId"></param>
    /// <param name="attempt"></param>
    /// <returns></returns>
    private async Task<int> CommitBatchAsync(
        DataProvider dataProvider,
        ICollection<Dataset> processedDatasets,
        string batchId,
        byte attempt = 1)
    {
        try
        {
            Logger.LogDebug("Datasets - Start storing {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);
            var processedCount = await DatasetRepository.AddManyAsync(processedDatasets);
            Logger.LogDebug("Datasets - Finish storing {@dataProvider} batch: {@batchId} ({@datasetProcessCount})", dataProvider.Identifier, batchId, processedCount);

            return processedCount;
        }
        catch (Exception e)
        {
            if (attempt < 3)
            {
                Logger.LogWarning(e, "Datasets - Failed to commit batch: {@batchId} for {@dataProvider}, attempt: " + attempt, batchId, dataProvider);
                Thread.Sleep(attempt * 200);
                attempt++;
                return await CommitBatchAsync(dataProvider, processedDatasets, batchId, attempt);
            }

            Logger.LogError(e, "Datasets - Failed to commit batch:{@batchId} for {@dataProvider}", batchId, dataProvider);
            throw;
        }

    }

    /// <summary>
    ///  Process a batch of data
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="startId"></param>
    /// <param name="endId"></param>
    /// <param name="datasetFactory"></param>
    /// <param name="datasetVerbatimRepository"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> ProcessBatchAsync(
        DataProvider dataProvider,
        int startId,
        int endId,
        IDatasetFactory<TVerbatim> datasetFactory,
        TVerbatimRepository datasetVerbatimRepository,
        IJobCancellationToken cancellationToken)
    {
        try
        {
            cancellationToken?.ThrowIfCancellationRequested();
            Logger.LogDebug("Datasets - Start fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);
            var datasetsBatch = await datasetVerbatimRepository.GetBatchAsync(startId, endId);
            Logger.LogDebug("Datasets - Finish fetching {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);

            if (!datasetsBatch?.Any() ?? true)
            {
                return 0;
            }

            Logger.LogDebug("Dataset - Start processing {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);

            var datasets = new ConcurrentDictionary<string, Dataset>();

            foreach (var verbatimDataset in datasetsBatch!)
            {
                var dataset = datasetFactory.CreateProcessedDataset(verbatimDataset);

                if (dataset == null)
                {
                    continue;
                }

                // Add dataset
                datasets.TryAdd(dataset.Identifier.ToString(), dataset);
            }

            Logger.LogDebug("Dataset - Finish processing {@dataProvider} batch ({@batchStartId}-{@batchEndId})", dataProvider.Identifier, startId, endId);

            return await ValidateAndStoreDatasets(dataProvider, datasets.Values, $"{startId}-{endId}");
        }
        catch (JobAbortedException)
        {
            // Throw cancelation again to let function above handle it
            throw;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Datasets - Process {@dataProvider} datasets from id: {@batchStartId} to id: {@batchEndId} failed", dataProvider.Identifier, startId, endId);
            throw;
        }
        finally
        {                
            ProcessManager.Release($"{dataProvider}, Batch={startId}-{endId}");
        }
    }

    /// <summary>
    /// Method to override in parent class 
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<int> ProcessDatasetsAsync(
        DataProvider dataProvider,
        IJobCancellationToken cancellationToken);

    /// <summary>
    /// Parent class will use call this to process data
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="datasetFactory"></param>
    /// <param name="datasetVerbatimRepository"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<int> ProcessDatasetsAsync(
        DataProvider dataProvider,
        IDatasetFactory<TVerbatim> datasetFactory,
        TVerbatimRepository datasetVerbatimRepository,
        IJobCancellationToken cancellationToken)
    {
        var startId = 1;
        var maxId = await datasetVerbatimRepository.GetMaxIdAsync();
        var processBatchTasks = new List<Task<int>>();

        while (startId <= maxId)
        {
            var batchEndId = startId + 1000 - 1;                

            await ProcessManager.WaitAsync($"{dataProvider}, Batch={startId}-{batchEndId}");
            processBatchTasks.Add(ProcessBatchAsync(
                dataProvider,
                startId,
                batchEndId,
                datasetFactory,
                datasetVerbatimRepository,
                cancellationToken));
            startId = batchEndId + 1;
        }

        await Task.WhenAll(processBatchTasks);

        return processBatchTasks.Sum(t => t.Result);
    }

    protected async Task<int> ValidateAndStoreDatasets(DataProvider dataProvider, ICollection<Dataset> datasets, string batchId)
    {
        if (!datasets?.Any() ?? true)
        {
            return 0;
        }

        var processedCount = await CommitBatchAsync(dataProvider, datasets!, batchId);

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
            Logger.LogDebug("Dataset - Start processing {@dataProvider} datasets", dataProvider.Identifier);
            var processCount = await ProcessDatasetsAsync(dataProvider, cancellationToken);

            Logger.LogInformation("Dataset - Finish processing {@dataProvider} datasets.", dataProvider.Identifier);

            return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount, 0, 0);
        }
        catch (JobAbortedException)
        {
            Logger.LogInformation("{@dataProvider} dataset processing was canceled.", dataProvider.Identifier);
            return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to process {@dataProvider} datasets", dataProvider.Identifier);
            return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
        }
    }
}