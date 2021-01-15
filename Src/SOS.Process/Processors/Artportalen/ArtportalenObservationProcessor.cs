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
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Delete batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="verbatimIds"></param>
        /// <returns></returns>
        private async Task<bool> DeleteBatchAsync(
            ICollection<int> sightingIds)
        {
            try
            {
                return await ProcessRepository.DeleteArtportalenBatchAsync(sightingIds);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to delete AP batch");
                return false;
            }

        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<ArtportalenObservationProcessor> logger,
            IAreaHelper areaHelper) : 
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _processConfiguration =
                processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));

            _areaHelper =
              areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }

            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,            
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            return await ProcessAsync(dataProvider, taxa, _areaHelper, false, mode, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessProtectedObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,            
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            return await ProcessAsync(dataProvider, taxa, _areaHelper, true, mode, cancellationToken);
        }

        /// <summary>
        /// Process verbatim
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessAsync(
            DataProvider dataProvider,            
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IAreaHelper areaHelper,
            bool protectedObservations,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, taxa, _processedVocabularyRepository, areaHelper, mode != JobRunModes.Full, protectedObservations);
            _artportalenVerbatimRepository.IncrementalMode = mode != JobRunModes.Full;

            if (_processConfiguration.ParallelProcessing)
            {
                // 1. process public observations
                return await ProcessObservationsParallel(dataProvider, observationFactory, protectedObservations, mode, cancellationToken);
            }

            // Sequential processing is used for easier debugging.
            return await ProcessObservationsSequential(dataProvider, observationFactory, taxa, mode, cancellationToken);
        }

        private async Task<int> ProcessObservationsParallel(
            DataProvider dataProvider,
            ArtportalenObservationFactory observationFactory,
            bool protectedObservations,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            _artportalenVerbatimRepository.ProtectedObservations = protectedObservations;
            ProcessRepository.Protected = protectedObservations;

            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + ProcessRepository.WriteBatchSize - 1;
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
        /// <param name="mode"></param>
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

                return await ValidateAndStoreObservations(dataProvider, processedObservationsBatch, mode, $"{startId}-{endId}", cancellationToken);
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
            ArtportalenObservationFactory observationFactory,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<Observation> observations = new List<Observation>();
           
            using var cursor = await _artportalenVerbatimRepository.GetAllByCursorAsync();
            var batchId = 0;
           
            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                observations.Add(observationFactory.CreateProcessedObservation(verbatimObservation));
                if (IsBatchFilledToLimit(observations.Count))
                {
                    verbatimCount += await ValidateAndStoreObservations(dataProvider, observations, mode, batchId++.ToString(), cancellationToken);
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                verbatimCount += await ValidateAndStoreObservations(dataProvider, observations, mode, batchId.ToString(), cancellationToken);
            }

            return verbatimCount;
        }

        private async Task<int> ValidateAndStoreObservations(
            DataProvider dataProvider,
            ICollection<Observation> observations,
            JobRunModes mode, 
            string batchId,
            IJobCancellationToken cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            observations = await ValidateAndRemoveInvalidObservations(dataProvider, observations, batchId);

            if (mode != JobRunModes.Full)
            {
                Logger.LogDebug($"Start deleteing live data ({batchId})");
                var success = await DeleteBatchAsync(observations.Select(v => v.ArtportalenInternal.SightingId).ToArray());
                Logger.LogDebug($"Finish deleteing live data ({batchId}) {success}");
            }

            var verbatimCount = await CommitBatchAsync(dataProvider, observations, batchId);

            if (mode == JobRunModes.Full)
            {
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider, batchId);
            }

            observations.Clear();
            Logger.LogDebug($"Artportalen sightings processed: {verbatimCount}");

            return verbatimCount;
        }
    }
}