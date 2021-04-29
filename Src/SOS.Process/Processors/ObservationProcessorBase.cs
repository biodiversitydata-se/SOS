using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Process.Processors
{
    public abstract class ObservationProcessorBase<TEntity>
    {
        protected readonly SemaphoreSlim SemaphoreBatch;

        /// <summary>
        /// Commit batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="protectedData"></param>
        /// <param name="processedObservations"></param>
        /// <param name="batchId"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<int> CommitBatchAsync(
            DataProvider dataProvider,
            bool protectedData,
            ICollection<Observation> processedObservations,
            string batchId,
            byte attempt = 1)
        {
            try
            {
                if (vocabularyValueResolver.Configuration.ResolveValues)
                {
                    // used for testing purpose for easier debugging of vocabulary mapped data.
                    vocabularyValueResolver
                        .ResolveVocabularyMappedValues(
                            processedObservations);
                }

                Logger.LogDebug($"Start storing {dataProvider.Identifier} batch: {batchId}");
                var processedCount = protectedData
                    ? await ProtectedRepository.AddManyAsync(processedObservations)
                    : await PublicRepository.AddManyAsync(processedObservations);
                Logger.LogDebug($"Finish storing {dataProvider.Identifier} batch: {batchId} ({processedCount})");

                return processedCount;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning(e, $"Failed to commit batch: {batchId} for {dataProvider}, attempt: {attempt}");
                    System.Threading.Thread.Sleep(attempt * 200);
                    attempt++;
                    return await CommitBatchAsync(dataProvider, protectedData, processedObservations, batchId, attempt);
                }

                Logger.LogError(e, $"Failed to commit batch:{batchId} for {dataProvider}");
                throw;
            }

        }

        /// <summary>
        /// Delete batch
        /// </summary>
        /// <param name="occurrenceIds"></param>
        /// <returns></returns>
        private async Task<bool> DeleteBatchAsync(
            bool protectedObservations,
            ICollection<string> occurrenceIds)
        {
            try
            {
                return protectedObservations ?
                    await ProtectedRepository.DeleteByOccurrenceIdAsync(occurrenceIds) :
                    await PublicRepository.DeleteByOccurrenceIdAsync(occurrenceIds);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to delete batch by occurrence id's");
                return false;
            }
        }

        /// <summary>
        /// Resolve vocabulary mapped values and then write the observations to DwC-A CSV files.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        private async Task<bool> WriteObservationsToDwcaCsvFiles(
            IEnumerable<Observation> processedObservations,
            DataProvider dataProvider,
            string batchId = "")
        {

            Logger.LogDebug($"Start writing {dataProvider.Identifier} CSV ({batchId})");
            vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
            var success = await dwcArchiveFileWriterCoordinator.WriteObservations(processedObservations, dataProvider, batchId);

            Logger.LogDebug($"Finish writing {dataProvider.Identifier} CSV ({batchId}) - {success}");

            return success;
        }

        protected readonly IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator;
        protected readonly IVocabularyValueResolver vocabularyValueResolver;
        protected readonly ILogger<TEntity> Logger;
        protected readonly IProcessedPublicObservationRepository PublicRepository;
        protected readonly IProcessedProtectedObservationRepository ProtectedRepository;
        protected readonly IValidationManager ValidationManager;

        protected bool EnableDiffusion { get; }

        /// <summary>
        /// Constructor for public only 
        /// </summary>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        protected ObservationProcessorBase(IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<TEntity> logger) : this(processedPublicObservationRepository, null, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processConfiguration, logger)
        {
        }

        /// <summary>
        /// Constructor for public and protected
        /// </summary>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedProtectedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        protected ObservationProcessorBase(IProcessedPublicObservationRepository processedPublicObservationRepository,
            IProcessedProtectedObservationRepository processedProtectedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<TEntity> logger)
        {
            PublicRepository = processedPublicObservationRepository ??
                               throw new ArgumentNullException(nameof(processedPublicObservationRepository));

            ProtectedRepository = processedProtectedObservationRepository;

            this.vocabularyValueResolver = vocabularyValueResolver ??
                                           throw new ArgumentNullException(nameof(vocabularyValueResolver));
            this.dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));
            ValidationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            EnableDiffusion = processConfiguration?.Diffusion ?? false;
            SemaphoreBatch = new SemaphoreSlim(processConfiguration?.NoOfThreads ?? throw new ArgumentNullException(nameof(processConfiguration)));

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected abstract Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        protected async Task<int> ValidateAndStoreObservations(DataProvider dataProvider, JobRunModes mode, bool protectedObservations, ICollection<Observation> observations, string batchId)
        {
            if (!observations?.Any() ?? true)
            {
                return 0;
            }

            observations =
                await ValidateAndRemoveInvalidObservations(dataProvider, observations, batchId);

            if (mode != JobRunModes.Full)
            {
                Logger.LogDebug($"Start deleting {dataProvider.Identifier} live data {batchId}");
                var occurrenceIds = observations.Select(o => o.Occurrence.OccurrenceId).ToArray();
                var success = await DeleteBatchAsync(protectedObservations, occurrenceIds);

                // If provider supports protected observations and diffusion is disabled,
                // make sure the observation don't exists in both public and protected index
                if (dataProvider.SupportProtectedHarvest && !EnableDiffusion)
                {
                    success = await DeleteBatchAsync(!protectedObservations, occurrenceIds);
                }

                Logger.LogDebug($"Finish deleting {dataProvider.Identifier} live data {batchId}: {success}");
            }

            var processedCount = await CommitBatchAsync(dataProvider, protectedObservations, observations, batchId);

            if (mode == JobRunModes.Full && !protectedObservations)
            {
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider, batchId);
            }
            observations.Clear();

            return processedCount;
        }

        protected async Task<ICollection<Observation>> ValidateAndRemoveInvalidObservations(
            DataProvider dataProvider,
            ICollection<Observation> observations,
            string batchId)
        {
            Logger.LogDebug($"Start validating {dataProvider.Identifier} batch: {batchId}");
            var invalidObservations = ValidationManager.ValidateObservations(ref observations, dataProvider);
            await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
            Logger.LogDebug($"End validating {dataProvider.Identifier} batch: {batchId}");

            return observations;
        }

        protected bool IsBatchFilledToLimit(int count)
        {
            return count % PublicRepository.BatchSize == 0;
        }

        protected int WriteBatchSize => PublicRepository.WriteBatchSize;

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
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider.Identifier} verbatim observations");
            var startTime = DateTime.Now;

            try
            {
                if (mode == JobRunModes.Full)
                {
                    Logger.LogDebug($"Start deleting {dataProvider.Identifier} data");
                    if (!await PublicRepository.DeleteProviderDataAsync(dataProvider))
                    {
                        Logger.LogError($"Failed to delete {dataProvider.Identifier} public data");
                    }

                    if (dataProvider.SupportProtectedHarvest && !await ProtectedRepository.DeleteProviderDataAsync(dataProvider))
                    {
                        Logger.LogError($"Failed to delete {dataProvider.Identifier} protected data");
                    }

                    Logger.LogDebug($"Finish deleting {dataProvider.Identifier} data");
                }

                Logger.LogDebug($"Start processing {dataProvider.Identifier} data");
                var processCount = await ProcessObservations(dataProvider, taxa, mode, cancellationToken);

                Logger.LogInformation($"Finish processing {dataProvider.Identifier} data.");

                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount.publicCount, processCount.protectedCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider.Identifier} observation processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider.Identifier} sightings");
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }
    }
}