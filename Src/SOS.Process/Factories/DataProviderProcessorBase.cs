using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Factories
{
    public abstract class DataProviderProcessorBase<TEntity> : ProcessBaseFactory<TEntity>
    {
        protected readonly IFieldMappingResolverHelper FieldMappingResolverHelper;
        public abstract DataProvider DataProvider { get; }

        protected DataProviderProcessorBase(
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<TEntity> logger) : base(processedSightingRepository, logger)
        {
            FieldMappingResolverHelper = fieldMappingResolverHelper ?? throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
        }

        public virtual async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogDebug($"Start Processing {DataProvider} verbatim observations");
            var startTime = DateTime.Now;
            try
            {
                Logger.LogDebug($"Start deleting {DataProvider} data");
                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider))
                {
                    Logger.LogError($"Failed to delete {DataProvider} data");
                    return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
                }
                Logger.LogDebug($"Finish deleting {DataProvider} data");

                Logger.LogDebug($"Start processing {DataProvider} data");
                var verbatimCount = await ProcessObservations(taxa, cancellationToken);
                Logger.LogDebug($"Finish processing {DataProvider} data.");

                return RunInfo.Success(DataProvider, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{DataProvider} observation processing was canceled.");
                return RunInfo.Cancelled(DataProvider, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {DataProvider} sightings");
                return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
            }
        }

        protected abstract Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken);
        

        protected async Task<int> CommitBatchAsync(ICollection<ProcessedSighting> sightings)
        {
            FieldMappingResolverHelper.ResolveFieldMappedValues(sightings);
            var successCount = await ProcessRepository.AddManyAsync(sightings);

            return successCount;
        }

        protected bool IsBatchFilledToLimit(int count)
        {
            return count % ProcessRepository.BatchSize == 0;
        }
    }
}