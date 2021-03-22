using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Nors.Interfaces;

namespace SOS.Process.Processors.Nors
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class NorsObservationProcessor : ObservationProcessorBase<NorsObservationProcessor>,
        INorsObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly INorsObservationVerbatimRepository _norsObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="norsObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public NorsObservationProcessor(INorsObservationVerbatimRepository norsObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<NorsObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _norsObservationVerbatimRepository = norsObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(norsObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.NorsObservations;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var batchId = 0;
            var processedCount = 0;
            ICollection<Observation> observations = new List<Observation>();
            var observationFactory = new NorsObservationFactory(dataProvider, taxa);

            using var cursor = await _norsObservationVerbatimRepository.GetAllByCursorAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    batchId++;

                    processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                    observations.Clear();
                    Logger.LogDebug($"NORS observations processed: {processedCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();

                batchId++;

                processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                observations.Clear();
                Logger.LogDebug($"NORS observations processed: {processedCount}");
            }

            return (processedCount, 0);
        }
    }
}