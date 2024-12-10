using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Extensions;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
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
using System.Collections.Concurrent;
using System.Text;

namespace SOS.Harvest.Processors
{
    public abstract class ObservationProcessorBase<TClass, TVerbatim, TVerbatimRepository> : ProcessorBase<TClass>
        where TVerbatim : IEntity<int>
        where TVerbatimRepository : IVerbatimRepositoryBase<TVerbatim, int>
    {
        private readonly IDiffusionManager _diffusionManager;
        private readonly bool _logGarbageCharFields;

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

                Logger.LogDebug("Start storing {@dataProvider} batch: {@batchId}" + $"{(protectedData ? " [protected]" : "")}", dataProvider.Identifier, batchId);
                var processedCount =
                    await ProcessedObservationRepository.AddManyAsync(processedObservations, protectedData);

                Logger.LogDebug("Finish storing {@dataProvider} batch: {@batchId} ({@processedCount})" + $"{(protectedData ? " [protected]" : "")}", dataProvider.Identifier, batchId, processedCount);

                return processedCount;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning(e, "Failed to commit batch: {@batchId} for {@dataProvider}, attempt: " + $"{attempt}{(protectedData ? " [protected]" : "")}", batchId, dataProvider.Identifier);
                    Thread.Sleep(attempt * 200);
                    attempt++;
                    return await CommitBatchAsync(dataProvider, protectedData, processedObservations, batchId, attempt);
                }

                Logger.LogError(e, "Failed to commit batch:{@batchId} for {@dataProvider}" + $"{(protectedData ? " [protected]" : "")}", batchId, dataProvider.Identifier);
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
            bool sensitiveObservations,
            ICollection<string> occurrenceIds)
        {
            try
            {
                return await ProcessedObservationRepository.DeleteByOccurrenceIdAsync(occurrenceIds, sensitiveObservations);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to delete batch by occurrence id's");
                return false;
            }
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
        private async Task<(int publicCount, int protectedCount, int failedCount)> FetchAndProcessBatchAsync(
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
                Logger.LogDebug("Start fetching {@dataProvider} batch ({@batchId})", dataProvider.Identifier, batchId);
                var mongoDbReadTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.MongoDbRead);

                // Make up to 3 attempts with 0,5 sek sleep between the attempts 
                var verbatimObservationsBatch = await PollyHelper.GetRetryPolicy(3, 500).ExecuteAsync(async () =>
                {
                    return await observationVerbatimRepository.GetBatchAsync(startId, endId);
                });

                TimeManager.Stop(ProcessTimeManager.TimerTypes.MongoDbRead, mongoDbReadTimerSessionId);
                Logger.LogDebug("Finish fetching {@dataProvider} batch ({@batchId})", dataProvider.Identifier, batchId);

                return await ProcessBatchAsync(dataProvider, verbatimObservationsBatch, batchId, mode,
                    observationFactory);
            }
            catch (JobAbortedException)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Fetch and Process {@dataProvider} observations, batch {@batchId} failed", dataProvider.Identifier, batchId);
                throw;
            }
            finally
            {
                ProcessManager.Release();
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

            Logger.LogDebug("Start writing {@dataProvider} CSV ({@batchId})", dataProvider.Identifier, batchId);
            var csvWriteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.CsvWrite);

            LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservations);
            vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, Cultures.en_GB, true);
            var success = await dwcArchiveFileWriterCoordinator.WriteHeaderlessDwcaFileParts(processedObservations, dataProvider, batchId);

            TimeManager.Stop(ProcessTimeManager.TimerTypes.CsvWrite, csvWriteTimerSessionId);

            Logger.LogDebug("Finish writing {@dataProvider} CSV ({@batchId}) - " + success, dataProvider.Identifier, batchId);

            return success;
        }

        protected readonly IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator;
        protected readonly IVocabularyValueResolver vocabularyValueResolver;
        protected readonly IProcessedObservationCoreRepository ProcessedObservationRepository;
        protected readonly IValidationManager ValidationManager;
        protected readonly IUserObservationRepository _userObservationRepository;

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
        /// <param name="userObservationRepository"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationProcessorBase(
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            IUserObservationRepository? userObservationRepository,
            ProcessConfiguration processConfiguration,
            ILogger<TClass> logger) : base(processManager, processTimeManager, processConfiguration, logger)
        {
            ProcessedObservationRepository = processedObservationRepository ??
                                             throw new ArgumentNullException(nameof(processedObservationRepository));
            _diffusionManager = diffusionManager ?? throw new ArgumentNullException(nameof(diffusionManager));

            this.vocabularyValueResolver = vocabularyValueResolver ??
                                           throw new ArgumentNullException(nameof(vocabularyValueResolver));
            this.dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));
            ValidationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));

            EnableDiffusion = processConfiguration?.Diffusion ?? false;
            _logGarbageCharFields = processConfiguration?.LogGarbageCharFields ?? false;
            _userObservationRepository = userObservationRepository!;
        }

        /// <summary>
        /// Virtual function, must be overrided 
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        /// <inheritdoc />
        protected async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            JobRunModes mode,
            IObservationFactory<TVerbatim> observationFactory,
            TVerbatimRepository observationVerbatimRepository,
            IJobCancellationToken cancellationToken)
        {
            var startId = 1;
            var maxId = await observationVerbatimRepository.GetMaxIdAsync();
            var observationCount = await observationVerbatimRepository.CountAllDocumentsAsync();
            var processBatchTasks = new List<Task<(int publicCount, int protectedCount, int failedCount)>>();
            Logger.LogInformation("Start processing {@dataProvider} data. MaxId={@maxId}, Mode={@mode}, Count={@observationCount}", dataProvider.Identifier, maxId, mode, observationCount);            

            while (startId <= maxId)
            {
                await ProcessManager.WaitAsync();

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
            return (processBatchTasks.Sum(t => t.Result.publicCount), processBatchTasks.Sum(t => t.Result.protectedCount), processBatchTasks.Sum(t => t.Result.failedCount));
        }

        protected async Task<(int publicCount, int protectedCount, int FailedCount)> ProcessBatchAsync(
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
                    return (0, 0, 0);
                }

                Logger.LogDebug("Start processing {@dataProvider} batch ({@batchId})", dataProvider.Identifier, batchId);
                ConcurrentDictionary<string, Observation> publicObservations, sensitiveObservations;
                ProcessObservationsBatch(                    
                    verbatimObservationsBatch, 
                    observationFactory,                     
                    out publicObservations, 
                    out sensitiveObservations);
                verbatimObservationsBatch = null!;

                Logger.LogDebug("Finish processing {@dataProvider} batch ({@batchId})", dataProvider.Identifier, batchId);

                if (_logGarbageCharFields)
                {                    
                    LogGarbageCharFields(dataProvider, publicObservations.Values, sensitiveObservations.Values);
                }

                // If incremental harvest
                if (mode != JobRunModes.Full)
                {
                    Logger.LogDebug("Start deleting {@dataProvider} data {@batchId}", dataProvider.Identifier, batchId);
                    var occurrenceIds = publicObservations.Select(o => o.Key).ToHashSet();
                    occurrenceIds.UnionWith(sensitiveObservations.Select(o => o.Key));

                    var elasticSearchDeleteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ElasticsearchDelete);

                    // Make sure no old data exists in elastic
                    var deleteTasks = new[]
                    {
                        DeleteBatchAsync(false, occurrenceIds),
                        DeleteBatchAsync(true, occurrenceIds)
                    };
                    var deleteResult = await Task.WhenAll(deleteTasks);
                    TimeManager.Stop(ProcessTimeManager.TimerTypes.ElasticsearchDelete, elasticSearchDeleteTimerSessionId);

                    Logger.LogDebug("Finish deleting {@dataProvider} data {@batchId}: " +$"{deleteResult.All(r => r)}", dataProvider.Identifier, batchId);
                }

                // Store observations
                var validateAndStoreTasks = new[]
                {
                    ValidateAndStoreObservations(dataProvider, mode, false, publicObservations.Values, $"{batchId}"),
                    ValidateAndStoreObservations(dataProvider, mode, true, sensitiveObservations.Values, $"{batchId}")
                };

                await Task.WhenAll(validateAndStoreTasks);

                var publicCount = validateAndStoreTasks[0].Result.SuccessCount;
                var protectedCount = validateAndStoreTasks[1].Result.SuccessCount;
                var failedCount = validateAndStoreTasks[0].Result.FailedCount + validateAndStoreTasks[1].Result.FailedCount;

                return (publicCount, protectedCount, failedCount);
            }
            catch (JobAbortedException)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Process {@dataProvider} observations, batch {@batchId} failed", dataProvider.Identifier, batchId);
                throw;
            }
        }

        public static void ProcessObservationsBatch(                
                IEnumerable<TVerbatim> verbatimObservationsBatch, 
                IObservationFactory<TVerbatim> observationFactory,                
                out ConcurrentDictionary<string, Observation> publicObservations, 
                out ConcurrentDictionary<string, Observation> sensitiveObservations)
        {
            publicObservations = new ConcurrentDictionary<string, Observation>();
            sensitiveObservations = new ConcurrentDictionary<string, Observation>();
            foreach (var verbatimObservation in verbatimObservationsBatch!)
            {
                Observation? observation;
                if (observationFactory.IsVerbatimObservationDiffusedByProvider(verbatimObservation))
                {
                    observation = observationFactory.CreateProcessedObservation(verbatimObservation, true);
                    if (observation.ShallBeProtected())
                    {
                        // If the observation shall be protected, then create the observation with real coordinates.
                        observation = observationFactory.CreateProcessedObservation(verbatimObservation, false);
                    }
                    else
                    {
                        // Add duplicate observation with real coordinates to sensitive index.
                        var observationWithRealCoordinates = observationFactory.CreateProcessedObservation(verbatimObservation, false);
                        if (observationWithRealCoordinates != null)
                        {
                            observationWithRealCoordinates.Sensitive = true;
                            observationWithRealCoordinates.HasGeneralizedObservationInOtherIndex = true;
                            observationWithRealCoordinates.Occurrence.SensitivityCategory = 3;
                            observationWithRealCoordinates.AccessRights = new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage };
                            sensitiveObservations.TryAdd(observationWithRealCoordinates.Occurrence!.OccurrenceId, observationWithRealCoordinates);
                        }
                    }
                }
                else
                {
                    observation = observationFactory.CreateProcessedObservation(verbatimObservation, false);
                }

                if (observation == null)
                {
                    continue;
                }

                // If observation is protected
                if (observation.ShallBeProtected())
                {
                    observation.Sensitive = true;
                    sensitiveObservations.TryAdd(observation.Occurrence!.OccurrenceId, observation);

                    // Don't support system diffusion for now. Maybe later.
                    ////If it is a protected sighting, public users should not be possible to find it in the current month 
                    //if (!EnableDiffusion ||
                    //    (
                    //        observation.Occurrence.SensitivityCategory > 2 &&
                    //        (observation.Event?.StartDate?.Year ?? 0) == DateTime.Now.Year ||
                    //        (observation?.Event?.EndDate?.Year ?? 0) == DateTime.Now.Year
                    //    ) &&
                    //    (
                    //        (observation!.Event?.StartDate?.Month ?? 0) == DateTime.Now.Month ||
                    //        (observation?.Event?.EndDate?.Month ?? 0) == DateTime.Now.Month
                    //    )
                    //)
                    //{
                    //    continue;
                    //}

                    //// Recreate observation, diffused if provider supports diffusing 
                    //processTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ProcessObservation);
                    //observation = observationFactory.CreateProcessedObservation(verbatimObservation, true);

                    //// If provider don't support diffusion, we provide it for them
                    //if (observation!.DiffusionStatus != DiffusionStatus.DiffusedByProvider)
                    //{
                    //    if (!dataProvider.AllowSystemDiffusion)
                    //    {
                    //        continue;
                    //    }
                    //    var diffuseTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.Diffuse);

                    //    // Diffuse protected observation before adding it to public index. 
                    //    _diffusionManager.DiffuseObservation(observation);
                    //}
                }
                else
                {
                    // Add public observation
                    publicObservations.TryAdd(observation.Occurrence!.OccurrenceId, observation);
                }
            }
        }        

        private void LogGarbageCharFields(
            DataProvider dataProvider, 
            ICollection<Observation> publicObservations,
            ICollection<Observation> sensitiveObservations)
        {
            var objectHelper = new ObjectHelper();            
            foreach (var observation in publicObservations)
            {
                var propsWithGarabageChars = objectHelper.GetPropertiesWithGarbageChars(observation);
                if (propsWithGarabageChars.Any())
                {
                    Logger.LogDebug("Garbage chars {@dataProvider}, id: {@occurrenceId}, field/s: " + $"{string.Join('|', propsWithGarabageChars)}", dataProvider.Identifier, observation.Occurrence?.OccurrenceId);
                }
            }

            foreach (var observation in sensitiveObservations)
            {
                var propsWithGarabageChars = objectHelper.GetPropertiesWithGarbageChars(observation);
                if (propsWithGarabageChars.Any())
                {
                    Logger.LogDebug("Garbage chars {@dataProvider}, id: {@occurrenceId}, field/s: " + $"{string.Join('|', propsWithGarabageChars)}", dataProvider.Identifier, observation.Occurrence?.OccurrenceId);
                }
            }
        }

        protected async Task<(int SuccessCount, int FailedCount)> ValidateAndStoreObservations(DataProvider dataProvider, JobRunModes mode, bool sensitiveObservations, ICollection<Observation> observations, string batchId)
        {
            if (!observations?.Any() ?? true)
            {
                return (0, 0);
            }

            // Translate vocabularies to swedish, default values in db
            vocabularyValueResolver.ResolveVocabularyMappedValues(observations, Cultures.sv_SE, true);

            var preValidationCount = observations!.Count;
            observations =
                await ValidateAndRemoveInvalidObservations(dataProvider, observations, batchId);

            if (!observations?.Any() ?? true)
            {
                return (0, preValidationCount);
            }

            var processedCount = await CommitBatchAsync(dataProvider, sensitiveObservations, observations!, batchId);

            if (mode == JobRunModes.Full && !sensitiveObservations && dwcArchiveFileWriterCoordinator.Enabled)
            {
                await WriteObservationsToDwcaCsvFiles(observations!, dataProvider, batchId);
            }

            if (ProcessConfiguration.ProcessUserObservation && mode == JobRunModes.Full && dataProvider.Id == DataProviderIdentifiers.ArtportalenId && !sensitiveObservations)
            {
                Logger.LogDebug("Add User Observations. BatchId={@batchId}, Protected={@sensitiveObservations}, Count={@observationCount}", batchId, sensitiveObservations, observations!.Count);
                var userObservations = observations.ToUserObservations();
                await _userObservationRepository.AddManyAsync(userObservations);
            }

            observations!.Clear();

            return (processedCount, preValidationCount - processedCount);
        }

        protected async Task<ICollection<Observation>> ValidateAndRemoveInvalidObservations(
            DataProvider dataProvider,
            ICollection<Observation> observations,
            string batchId)
        {
            Logger.LogDebug("Start validating {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);
            var validateObservationsTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.ValidateObservations);
            var invalidObservations = ValidationManager.ValidateObservations(ref observations, dataProvider);
            TimeManager.Stop(ProcessTimeManager.TimerTypes.ValidateObservations, validateObservationsTimerSessionId);
            if (dataProvider.Id == DataProviderIdentifiers.ArtportalenId)
            {
                LogInvalidObservations(dataProvider, invalidObservations);
            }

            var mongoDbWriteTimerSessionId = TimeManager.Start(ProcessTimeManager.TimerTypes.MongoDbWrite);
            await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
            TimeManager.Stop(ProcessTimeManager.TimerTypes.MongoDbWrite, mongoDbWriteTimerSessionId);
            Logger.LogDebug("End validating {@dataProvider} batch: {@batchId}", dataProvider.Identifier, batchId);

            return observations;
        }

        private void LogInvalidObservations(DataProvider dataProvider, ICollection<Lib.Models.Processed.Validation.InvalidObservation> invalidObservations)
        {
            const int maxChars = 5000;

            try
            {
                if (invalidObservations == null) return;
                Logger.LogWarning("Invalid observations for {@dataProvider}. Count={@invalidObservationsCount}.", dataProvider.Identifier, invalidObservations.Count);
                StringBuilder sb = new StringBuilder();
                foreach (var observation in invalidObservations.Take(10))
                {
                    Logger.LogWarning("Invalid observation for DataProvider={@dataProvider}, OccurrenceId={@occurrenceId}, Defects= " + $"{string.Join(", ", observation.Defects.Select(m => m.Information))}", 
                        dataProvider.Identifier, observation.OccurrenceID);
                }                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred in LogInvalidObservations(). DataProvider={@dataProvider}", dataProvider.Identifier);
            }
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
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider.Identifier} verbatim observations");
            var startTime = DateTime.Now;

            try
            {
                Logger.LogDebug("Start processing {@dataProvider} data", dataProvider.Identifier);
                var processCount = await ProcessObservationsAsync(dataProvider, taxa, dwcaVocabularyById, mode, cancellationToken);
                Logger.LogInformation("Finish processing {@dataProvider} data. publicCount={@publicProcessCount}, protectedCount={@sensitiveProcessCount}, failedCount={@invalidCount}", dataProvider.Identifier, processCount.publicCount, processCount.protectedCount, processCount.failedCount);

                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, processCount.publicCount, processCount.protectedCount, processCount.failedCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("{@dataProvider} observation processing was canceled.", dataProvider.Identifier);
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process {@dataProvider} sightings", dataProvider.Identifier);
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }
    }
}