using System;
using System.Collections.Generic;
using System.Linq;
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
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
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
        private readonly IAreaHelper _areaHelper;
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcaVerbatimRepository;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly IVocabularyRepository _processedVocabularyRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dwcaVerbatimRepository"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public DwcaObservationProcessor(IDarwinCoreArchiveVerbatimRepository dwcaVerbatimRepository,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<DwcaObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _dwcaVerbatimRepository =
                dwcaVerbatimRepository ?? throw new ArgumentNullException(nameof(dwcaVerbatimRepository));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processConfiguration =
                processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }
        }

        public override DataProviderType Type => DataProviderType.DwcA;

        public async Task<bool> DoesVerbatimDataExist()
        {
            var collectionExist = await _dwcaVerbatimRepository.CheckIfCollectionExistsAsync();
            return collectionExist;
        }

        public override async Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider.Identifier} verbatim observations");
            var startTime = DateTime.Now;
            try
            {
                var dataExists = await _dwcaVerbatimRepository.CheckIfCollectionExistsAsync(dataProvider.Id, dataProvider.Identifier);
                if (!dataExists)
                {
                    Logger.LogInformation($"Processing {dataProvider} failed because no harvested data existed.");
                    return ProcessingStatus.Failed(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
                }

                Logger.LogDebug($"Start deleting {dataProvider.Identifier} data");
                if (!await PublicRepository.DeleteProviderDataAsync(dataProvider))
                {
                    Logger.LogError($"Failed to delete {dataProvider.Identifier} data");
                }
                Logger.LogDebug($"Finish deleting {dataProvider.Identifier} data");

                Logger.LogDebug($"Start processing {dataProvider.Identifier} data");
                var verbatimCount = await ProcessObservationsSequential(
                    dataProvider,
                    taxa,
                    cancellationToken);
                
                Logger.LogInformation($"Finish processing {dataProvider.Identifier} data.");
                return ProcessingStatus.Success(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider} observation processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider} sightings");
                return ProcessingStatus.Failed(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
            }
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<int> ProcessObservationsSequential(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var batchId = 0;
            var processedCount = 0;
            var observationFactory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                taxa,
                _processedVocabularyRepository,
                _areaHelper);
            ICollection<Observation> observations = new List<Observation>();
            using var cursor = await _dwcaVerbatimRepository.GetAllByCursorAsync(dataProvider.Id, dataProvider.Identifier);
            int counter = 0;
            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                processedObservation.DataProviderId = dataProvider.Id;
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    batchId++;

                    processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                    observations.Clear();
                    Logger.LogDebug($"{dataProvider.Name} observations processed: {processedCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();

                batchId++;

                processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                observations.Clear();
                Logger.LogDebug($"{dataProvider.Name} observations processed: {processedCount}");
            }

            return processedCount;
        }
    }
}