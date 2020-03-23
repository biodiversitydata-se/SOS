using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Processors
{
    public abstract class ObservationProcessorBase<TEntity>
    {
        protected readonly IProcessedObservationRepository ProcessRepository;
        protected readonly ILogger<TEntity> Logger;
        protected readonly IFieldMappingResolverHelper FieldMappingResolverHelper;
        public abstract ObservationProvider DataProvider { get; }

        protected ObservationProcessorBase(
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FieldMappingResolverHelper = fieldMappingResolverHelper ?? throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
        }

        public virtual async Task<ProcessingStatus> ProcessAsync(
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
                    return ProcessingStatus.Failed(DataProvider, startTime, DateTime.Now);
                }
                Logger.LogDebug($"Finish deleting {DataProvider} data");

                Logger.LogDebug($"Start processing {DataProvider} data");
                var verbatimCount = await ProcessObservations(taxa, cancellationToken);
                Logger.LogDebug($"Finish processing {DataProvider} data.");

                return ProcessingStatus.Success(DataProvider, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{DataProvider} observation processing was canceled.");
                return ProcessingStatus.Cancelled(DataProvider, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {DataProvider} sightings");
                return ProcessingStatus.Failed(DataProvider, startTime, DateTime.Now);
            }

          return null;
        }

        protected abstract Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken);
        

        protected async Task<int> CommitBatchAsync(ICollection<ProcessedObservation> processedObservations)
        {
            FieldMappingResolverHelper.ResolveFieldMappedValues(processedObservations);
            var successCount = await ProcessRepository.AddManyAsync(processedObservations);

            return successCount;
        }

        protected bool IsBatchFilledToLimit(int count)
        {
            return count % ProcessRepository.BatchSize == 0;
        }
    }
}