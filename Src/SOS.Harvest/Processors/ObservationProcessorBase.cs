using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;

namespace SOS.Harvest.Processors
{
    public abstract class ObservationProcessorBase<TClass, TVerbatim, TVerbatimRepository> 
        where TVerbatim : IEntity<int>
        where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int>
    {
        private readonly IDiffusionManager _diffusionManager;
        private readonly IProcessManager _processManager;
        protected readonly IProcessTimeManager TimeManager;

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
            var elasticSearchWriteTimerSessionId = Guid.Empty;
            try
            {
                elasticSearchWriteTimerSessionId =
                    TimeManager.Start(ProcessTimeManager.TimerTypes.ElasticSearchWrite);

                if (vocabularyValueResolver.Configuration.ResolveValues)
                {
                    // used for testing purpose for easier debugging of vocabulary mapped data.
                    vocabularyValueResolver
                        .ResolveVocabularyMappedValues(
                            processedObservations);
                }

                Logger.LogDebug($"Start storing {dataProvider.Identifier} batch: {batchId}");
                var processedCount =
                    await ProcessedObservationRepository.AddManyAsync(processedObservations, protectedData);

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
            finally
            {
                TimeManager.Stop(ProcessTimeManager.TimerTypes.ElasticSearchWrite, elasticSearchWriteTimerSessionId);
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
                return await ProcessedObservationRepository.DeleteByOccurrenceIdAsync(occurrenceIds, protectedObservations);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to delete batch by occurrence id's");
                return false;
            }
        }

        private void PopulateDataQuality(Observation observation)
        {
            if (observation.Event?.StartDate == null ||
                (observation.Taxon?.Id ?? 0) == 0 ||
                (observation.Location?.DecimalLatitude ?? 0) == 0 ||
                (observation.Location?.DecimalLongitude ?? 0) == 0)
            {
                return;
            }
            // Round coordinates to 5 decimals (roughly 1m)
            var source = $"{observation.Event.StartDate.Value.ToUniversalTime().ToString("s")}-{observation.Taxon.Id}-{Math.Round(observation.Location.DecimalLongitude.Value, 5)}/{Math.Round(observation.Location.DecimalLatitude.Value, 5)}";
           
            observation.DataQuality = new DataQuality
            {
                UniqueKey = source.ToHash()
            };
        }

        /// <summary>
        ///  Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="mode"></param>
        /// <param name="observationFactory"></param>
        /// <param name="observationVerbatimRepository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<(int publicCount, int protectedCount)> FetchAndProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            JobRunModes mode,
            IObservationFactory<TVerbatim> observationFactory,
            TVerbatimRepository observationVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var batchId = $"{startId}-{endId}";
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching {dataProvider.Identifier} batch ({batchId})");
                var mongoDbReadTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.MongoDbRead);
                var verbatimObservationsBatch = await observationVerbatimRepository.GetBatchAsync(startId, endId);
                TimeManager.Stop(ProcessTimeManager.TimerTypes.MongoDbRead, mongoDbReadTimerSessionId);
                Logger.LogDebug($"Finish fetching {dataProvider.Identifier} batch ({batchId})");

                return await ProcessBatchAsync(dataProvider, verbatimObservationsBatch, batchId, mode,
                    observationFactory);
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Fetch and Process {dataProvider.Identifier} observations, batch {batchId} failed");
                throw;
            }
            finally
            {
                _processManager.Release();
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
            var csvWriteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.CsvWrite);

            LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservations);
            vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
            var success = await dwcArchiveFileWriterCoordinator.WriteObservations(processedObservations, dataProvider, batchId);

            TimeManager.Stop(ProcessTimeManager.TimerTypes.CsvWrite, csvWriteTimerSessionId);

            Logger.LogDebug($"Finish writing {dataProvider.Identifier} CSV ({batchId}) - {success}");

            return success;
        }

        protected readonly IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator;
        protected readonly IVocabularyValueResolver vocabularyValueResolver;
        protected readonly ILogger<TClass> Logger;
        protected readonly IProcessedObservationRepository ProcessedObservationRepository;
        protected readonly IValidationManager ValidationManager;

        protected bool EnableDiffusion { get; }

        /// <summary>
        /// Constructor for public and protected
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationProcessorBase(
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<TClass> logger)
        {
            ProcessedObservationRepository = processedObservationRepository ??
                                             throw new ArgumentNullException(nameof(processedObservationRepository));
            _diffusionManager = diffusionManager ?? throw new ArgumentNullException(nameof(diffusionManager));

            this.vocabularyValueResolver = vocabularyValueResolver ??
                                           throw new ArgumentNullException(nameof(vocabularyValueResolver));
            this.dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));
            ValidationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));

            EnableDiffusion = processConfiguration?.Diffusion ?? false;
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            TimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected abstract Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        /// <inheritdoc />
        protected async Task<(int publicCount, int protectedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            JobRunModes mode,
            IObservationFactory<TVerbatim> observationFactory,
            TVerbatimRepository observationVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var startId = 1;
            var maxId = await observationVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<(int publicCount, int protectedCount)>>();

            while (startId <= maxId)
            {
                await _processManager.WaitAsync();

                var batchEndId = startId + WriteBatchSize - 1;
                processBatchTasks.Add(FetchAndProcessBatchAsync(
                    dataProvider,
                    startId, 
                    batchEndId, 
                    mode, 
                    observationFactory,
                    observationVerbatimRepository,
                    cancellationToken));
                startId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return (processBatchTasks.Sum(t => t.Result.publicCount), processBatchTasks.Sum(t => t.Result.protectedCount));
        }

        protected async Task<(int publicCount, int protectedCount)> ProcessBatchAsync(
            DataProvider dataProvider,
            IEnumerable<TVerbatim> verbatimObservationsBatch,
            string batchId,
            JobRunModes mode,
            IObservationFactory<TVerbatim> observationFactory)
        {
            try
            {
                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return (0, 0);
                }

                Logger.LogDebug($"Start processing {dataProvider.Identifier} batch ({batchId})");

                var publicObservations = new ConcurrentDictionary<string, Observation>();
                var protectedObservations = new ConcurrentDictionary<string, Observation>();

                Parallel.ForEach(verbatimObservationsBatch, verbatimObservation =>
                {
                    var processTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ProcessObservation);
                    var observation = observationFactory.CreateProcessedObservation(verbatimObservation, false);
                    TimeManager.Stop(ProcessTimeManager.TimerTypes.ProcessObservation, processTimerSessionId);

                    if (observation == null)
                    {
                        return;
                    }

                    // Populate data quality property
                    PopulateDataQuality(observation);

                    // If  observation is protected
                    if (observation.Occurrence.SensitivityCategory > 2 || observation.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage)
                    {
                        observation.Protected = true;
                        observation.Sensitive = true;
                        protectedObservations.TryAdd(observation.Occurrence.OccurrenceId, observation);

                        //If it is a protected sighting, public users should not be possible to find it in the current month 
                        if (!EnableDiffusion || (observation.Occurrence.SensitivityCategory > 2 && (observation.Event?.StartDate?.Year ?? 0) == DateTime.Now.Year || (observation?.Event?.EndDate?.Year ?? 0) == DateTime.Now.Year) &&
                            ((observation.Event?.StartDate?.Month ?? 0) == DateTime.Now.Month || (observation?.Event?.EndDate?.Month ?? 0) == DateTime.Now.Month))
                        {
                            return;
                        }

                        // Recreate observation, diffused if provider supports diffusing 
                        processTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ProcessObservation);
                        observation = observationFactory.CreateProcessedObservation(verbatimObservation, true);

                        // Populate data quality property
                        PopulateDataQuality(observation);
                        TimeManager.Stop(ProcessTimeManager.TimerTypes.ProcessObservation, processTimerSessionId);

                        // If provider don't support diffusion, we provide it for them
                        if (observation.DiffusionStatus != DiffusionStatus.DiffusedByProvider)
                        {
                            var diffuseTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.Diffuse);

                            // Diffuse protected observation before adding it to public index. 
                            _diffusionManager.DiffuseObservation(observation);

                            TimeManager.Stop(ProcessTimeManager.TimerTypes.Diffuse, diffuseTimerSessionId);
                        }
                    }

                    // Add public observation
                    publicObservations.TryAdd(observation.Occurrence.OccurrenceId, observation);
                });

                Logger.LogDebug($"Finish processing {dataProvider.Identifier} batch ({batchId})");

                // If incremental harvest
                if (mode != JobRunModes.Full)
                {
                    Logger.LogDebug($"Start deleting {dataProvider.Identifier} data {batchId}");
                    var occurrenceIds = publicObservations.Select(o => o.Key).ToHashSet();
                    occurrenceIds.UnionWith(protectedObservations.Select(o => o.Key));

                    var elasticSearchDeleteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ElasticsearchDelete);

                    // Make sure no old data exists in elastic
                    var deleteTasks = new[]
                    {
                        DeleteBatchAsync(false, occurrenceIds),
                        DeleteBatchAsync(true, occurrenceIds)
                    };
                    var deleteResult = await Task.WhenAll(deleteTasks);

                    TimeManager.Stop(ProcessTimeManager.TimerTypes.ElasticsearchDelete, elasticSearchDeleteTimerSessionId);

                    Logger.LogDebug($"Finish deleting {dataProvider.Identifier} data {batchId}: {deleteResult.All(r => r)}");
                }

                // Store observations
                var validateAndStoreTasks = new[]
                {
                    ValidateAndStoreObservations(dataProvider, mode, false, publicObservations.Values, $"{batchId}"),
                    ValidateAndStoreObservations(dataProvider, mode, true, protectedObservations.Values, $"{batchId}")
                };

                await Task.WhenAll(validateAndStoreTasks);

                var publicCount = validateAndStoreTasks[0].Result;
                var protectedCount = validateAndStoreTasks[1].Result;

                return (publicCount, protectedCount);
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process {dataProvider.Identifier} observations, batch {batchId} failed");
                throw;
            }
        }

        protected async Task<int> ValidateAndStoreObservations(DataProvider dataProvider, JobRunModes mode, bool protectedObservations, ICollection<Observation> observations, string batchId)
        {
            if (!observations?.Any() ?? true)
            {
                return 0;
            }
            observations =
                await ValidateAndRemoveInvalidObservations(dataProvider, observations, batchId);

            if (!observations?.Any() ?? true)
            {
                return 0;
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
            var validateObservationsTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ValidateObservations);
            var invalidObservations = ValidationManager.ValidateObservations(ref observations, dataProvider);
            TimeManager.Stop(ProcessTimeManager.TimerTypes.ValidateObservations, validateObservationsTimerSessionId);

            var mongoDbWriteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.MongoDbWrite);
            await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
            TimeManager.Stop(ProcessTimeManager.TimerTypes.MongoDbWrite, mongoDbWriteTimerSessionId);
            Logger.LogDebug($"End validating {dataProvider.Identifier} batch: {batchId}");

            return observations;
        }

        protected int WriteBatchSize => ProcessedObservationRepository.WriteBatchSize;

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
                    if (!await ProcessedObservationRepository.DeleteProviderDataAsync(dataProvider, false))
                    {
                        Logger.LogError($"Failed to delete {dataProvider.Identifier} public data");
                    }

                    if (!await ProcessedObservationRepository.DeleteProviderDataAsync(dataProvider, true))
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