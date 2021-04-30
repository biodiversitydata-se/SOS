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
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation processor.
    /// </summary>
    public class DwcaObservationProcessor : ObservationProcessorBase<DwcaObservationProcessor>,
        IDwcaObservationProcessor
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _processedVocabularyRepository;

        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            IDarwinCoreArchiveVerbatimRepository dwcaVerbatimRepository, 
            DwcaObservationFactory observationFactory,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var verbatimObservationsBatch = await dwcaVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");

                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Start processing {dataProvider.Identifier} batch ({startId}-{endId})");

                var observations = new List<Observation>();

                foreach (var verbatimObservation in verbatimObservationsBatch)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                    
                    if (processedObservation == null)
                    {
                        continue;
                    }

                    processedObservation.DataProviderId = dataProvider.Id;
                    _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                    observations.Add(processedObservation);
                }

                Logger.LogDebug($"Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");

                return await ValidateAndStoreObservations(dataProvider, JobRunModes.Full, false, observations, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process {dataProvider.Identifier} sightings from id: {startId} to id: {endId} failed");
                throw;
            }
            finally
            {
                SemaphoreBatch.Release();
            }
        }

        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider.Identifier} verbatim observations");
            try
            {
                using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                        dataProvider,
                        _verbatimClient,
                        Logger);

                var observationFactory = await DwcaObservationFactory.CreateAsync(
                    dataProvider,
                    taxa,
                    _processedVocabularyRepository,
                    _areaHelper);

                var minId = 1;
                var maxId = await dwcArchiveVerbatimRepository.GetMaxIdAsync();
                var processBatchTasks = new List<Task<int>>();

                while (minId <= maxId)
                {
                    await SemaphoreBatch.WaitAsync();

                    var batchEndId = minId + WriteBatchSize - 1;
                    processBatchTasks.Add(ProcessBatchAsync(dataProvider, 
                        minId, 
                        batchEndId,
                        dwcArchiveVerbatimRepository,
                        observationFactory,
                        cancellationToken));
                    minId = batchEndId + 1;
                }

                await Task.WhenAll(processBatchTasks);

                return (processBatchTasks.Sum(t => t.Result), 0);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider} observation processing was canceled.");
                return (0, 0);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider} sightings");
                return (0, 0);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public DwcaObservationProcessor(
            IVerbatimClient verbatimClient,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<DwcaObservationProcessor> logger) :
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.DwcA;
    }
}