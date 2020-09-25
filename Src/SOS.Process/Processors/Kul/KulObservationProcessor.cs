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
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Kul
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class KulObservationProcessor : ObservationProcessorBase<KulObservationProcessor>, IKulObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public KulObservationProcessor(IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<KulObservationProcessor> logger) : 
                base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.KULObservations;

        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new KulObservationFactory(dataProvider, taxa);

            using var cursor = await _kulObservationVerbatimRepository.GetAllByCursorAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var invalidObservations = ValidationManager.ValidateObservations(ref observations);
                    await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                    verbatimCount += await CommitBatchAsync(dataProvider, observations);
                    await WriteObservationsToDwcaCsvFiles(observations, dataProvider);
                    observations.Clear();
                    Logger.LogDebug($"KUL Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var invalidObservations = ValidationManager.ValidateObservations(ref observations);
                await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                verbatimCount += await CommitBatchAsync(dataProvider, observations);
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider);
                Logger.LogDebug($"KUL Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}