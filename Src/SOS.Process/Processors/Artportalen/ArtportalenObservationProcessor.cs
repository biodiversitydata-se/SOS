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
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;

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
        private readonly IFieldMappingRepository _processedFieldMappingRepository;
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
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<ArtportalenObservationProcessor> logger) : 
                base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator, validationManager, logger)
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
        ///  Process all observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            if (_processConfiguration.ParallelProcessing)
            {
                return await ProcessObservationsParallel(dataProvider, taxa, mode, cancellationToken);
            }

            // Sequential processing is used for easier debugging.
            return await ProcessObservationsSequential(dataProvider, taxa, mode, cancellationToken);
        }

        private async Task<int> ProcessObservationsParallel(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, taxa, _processedFieldMappingRepository, mode != JobRunModes.Full);
            // Get min and max id from db

            _artportalenVerbatimRepository.IncrementalMode = mode != JobRunModes.Full;
            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + _processedFieldMappingRepository.BatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, batchStartId, batchEndId, mode, observationFactory,
                    cancellationToken));
                batchStartId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        /// <summary>
        /// Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="incrementalMode"></param>
        /// <param name="observationFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            JobRunModes mode,
            ArtportalenObservationFactory observationFactory,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Artportalen batch ({startId}-{endId})");
                var verbatimObservationsBatch = await _artportalenVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Artportalen batch ({startId}-{endId})");

                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Start processing Artportalen batch ({startId}-{endId})");
                var processedObservationsBatch =
                    observationFactory.CreateProcessedObservations(verbatimObservationsBatch);
                Logger.LogDebug($"Finish processing Artportalen batch ({startId}-{endId})");

                Logger.LogDebug($"Start validating Artportalen batch ({startId}-{endId})");
                var invalidObservations = ValidationManager.ValidateObservations(ref processedObservationsBatch);
                await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                Logger.LogDebug($"End validating Artportalen batch ({startId}-{endId})");

                if (mode != JobRunModes.Full)
                {
                    Logger.LogDebug($"Start deleteing live data ({startId}-{endId})");
                    var success = await  DeleteProviderBatchAsync(dataProvider, verbatimObservationsBatch.Select(v => v.Id).ToArray());
                    Logger.LogDebug($"Finish deleteing live data ({startId}-{endId}) {success}");
                }

                Logger.LogDebug($"Start storing Artportalen batch ({startId}-{endId})");
                var successCount = await CommitBatchAsync(dataProvider, processedObservationsBatch.ToArray());
                Logger.LogDebug($"Finish storing Artportalen batch ({startId}-{endId})");

                if (mode == JobRunModes.Full)
                {
                    Logger.LogDebug($"Start writing Artportalen CSV ({startId}-{endId})");
                    await WriteObservationsToDwcaCsvFiles(processedObservationsBatch, dataProvider, $"{startId}-{endId}");
                    Logger.LogDebug($"Finish writing Artportalen CSV ({startId}-{endId})");
                }
                   
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
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, taxa, _processedFieldMappingRepository, mode != JobRunModes.Full);
            ICollection<Observation> observations = new List<Observation>();
            _artportalenVerbatimRepository.IncrementalMode = mode != JobRunModes.Full;
            using var cursor = await _artportalenVerbatimRepository.GetAllByCursorAsync();
            var batchId = 0;
           
            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                observations.Add(observationFactory.CreateProcessedObservation(verbatimObservation));
                if (IsBatchFilledToLimit(observations.Count))
                {
                    verbatimCount += await ValidateAndStoreObservations(dataProvider, observations, mode, batchId++, cancellationToken);
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                verbatimCount += await ValidateAndStoreObservations(dataProvider, observations, mode, batchId, cancellationToken);
            }

            return verbatimCount;
        }

        private async Task<int> ValidateAndStoreObservations(
            DataProvider dataProvider,
            ICollection<Observation> observations,
            JobRunModes mode, 
            int batchId,
            IJobCancellationToken cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();
            var invalidObservations = ValidationManager.ValidateObservations(ref observations);
            await ValidationManager.AddInvalidObservationsToDb(invalidObservations);

            if (mode != JobRunModes.Full)
            {
                Logger.LogDebug("Start deleteing live data");
                var success = await DeleteProviderBatchAsync(dataProvider, observations.Select(v => v.VerbatimId).ToArray());
                Logger.LogDebug($"Finish deleteing live data {success}");
            }

            var verbatimCount = await CommitBatchAsync(dataProvider, observations);

            if (mode == JobRunModes.Full)
            {
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider, batchId.ToString());
            }

            observations.Clear();
            Logger.LogDebug($"Artportalen sightings processed: {verbatimCount}");

            return verbatimCount;
        }
    }
}