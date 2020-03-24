using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Artportalen
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ArtportalenObservationProcessor : ObservationProcessorBase<ArtportalenObservationProcessor>, Interfaces.IArtportalenObservationProcessor
    {
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly SemaphoreSlim _semaphore;
        public override ObservationProvider DataProvider => ObservationProvider.Artportalen;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(
            IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ?? throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }

            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        /// <summary>
        /// Process all observations
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = await ArtportalenObservationFactory.CreateAsync(taxa, _processedFieldMappingRepository);
            // Get min and max id from db
            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + _processedFieldMappingRepository.BatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(batchStartId, batchEndId, observationFactory, cancellationToken));
                batchStartId = batchEndId + 1;
            }
            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        /// <summary>
        /// Process a batch of data
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="observationFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            int startId,
            int endId,
            ArtportalenObservationFactory observationFactory,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Artportalen batch ({ startId }-{ endId })");
                var verbatimObservationsBatch = await _artportalenVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Artportalen batch ({ startId }-{ endId })");

                Logger.LogDebug($"Start processing Artportalen batch ({ startId }-{ endId })");
                var processedObservationsBatch = observationFactory.CreateProcessedObservations(verbatimObservationsBatch);
                Logger.LogDebug($"Finish processing Artportalen batch ({ startId }-{ endId })");

                Logger.LogDebug($"Start storing Artportalen batch ({ startId }-{ endId })");
                var successCount = await CommitBatchAsync(processedObservationsBatch.ToArray());
                Logger.LogDebug($"Finish storing Artportalen batch ({ startId }-{ endId })");
                
                return successCount;
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw e;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process Artportalen sightings from id: {startId} to id: {endId} failed");
            }
            finally
            {
                _semaphore.Release();
            }

            return 0;
        }
    }
}
