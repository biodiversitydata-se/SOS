using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Artportalen
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ArtportalenObservationProcessor : ObservationProcessorBase<ArtportalenObservationProcessor>,
        IArtportalenObservationProcessor
    {
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ILogger<ArtportalenObservationProcessor> logger) : base(processedObservationRepository,
            fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _processConfiguration =
                processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }

            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <summary>
        ///     Process all observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            if (_processConfiguration.ParallelProcessing)
            {
                return await ProcessObservationsParallel(dataProvider, taxa, cancellationToken);
            }

            // Sequential processing is used for easier debugging.
            return await ProcessObservationsSequential(dataProvider, taxa, cancellationToken);
        }

        private async Task<int> ProcessObservationsParallel(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, taxa, _processedFieldMappingRepository);
            // Get min and max id from db
            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + _processedFieldMappingRepository.BatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, batchStartId, batchEndId, observationFactory,
                    cancellationToken));
                batchStartId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        /// <summary>
        ///     Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="observationFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            ArtportalenObservationFactory observationFactory,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Artportalen batch ({startId}-{endId})");
                var verbatimObservationsBatch = await _artportalenVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Artportalen batch ({startId}-{endId})");

                Logger.LogDebug($"Start processing Artportalen batch ({startId}-{endId})");
                var processedObservationsBatch =
                    observationFactory.CreateProcessedObservations(verbatimObservationsBatch);
                Logger.LogDebug($"Finish processing Artportalen batch ({startId}-{endId})");

                Logger.LogDebug($"Start storing Artportalen batch ({startId}-{endId})");
                var successCount = await CommitBatchAsync(dataProvider, processedObservationsBatch.ToArray());
                Logger.LogDebug($"Finish storing Artportalen batch ({startId}-{endId})");

                Logger.LogDebug($"Start writing Artportalen CSV ({startId}-{endId})");
                var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(processedObservationsBatch, dataProvider,$"{startId}-{endId}");
                Logger.LogDebug($"Finish writing Artportalen CSV ({startId}-{endId})");

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

        private async Task<int> ProcessObservationsSequential(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, taxa, _processedFieldMappingRepository);
            ICollection<ProcessedObservation> sightings = new List<ProcessedObservation>();
            using var cursor = await _artportalenVerbatimRepository.GetAllByCursorAsync();
            int batchId = 0;

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                sightings.Add(observationFactory.CreateProcessedObservation(verbatimObservation));
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(dataProvider, sightings);
                    var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider, batchId++.ToString());
                    sightings.Clear();
                    Logger.LogDebug($"Artportalen sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(dataProvider, sightings);
                var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider, batchId++.ToString());
                Logger.LogDebug($"Artportalen sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}