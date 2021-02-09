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
using SOS.Process.Processors.ClamPortal.Interfaces;

namespace SOS.Process.Processors.ClamPortal
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ClamPortalObservationProcessor : ObservationProcessorBase<ClamPortalObservationProcessor>,
        IClamPortalObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public ClamPortalObservationProcessor(IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<ClamPortalObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.ClamPortalObservations;

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var batchId = 0;
            var processedCount = 0;
            ICollection<Observation> observations = new List<Observation>();
            var observationFactory = new ClamPortalObservationFactory(dataProvider, taxa);

            using var cursor = await _clamObservationVerbatimRepository.GetAllByCursorAsync();
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
                    Logger.LogDebug($"Clam Portal observations processed: {processedCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();

                batchId++;
                processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());

                Logger.LogDebug($"Clam Portal observations processed: {processedCount}");
            }

            return processedCount;
        }
    }
}