using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Sers
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class SersObservationProcessor : ObservationProcessorBase<SersObservationProcessor>,
        ISersObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sersObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public SersObservationProcessor(ISersObservationVerbatimRepository sersObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<SersObservationProcessor> logger) : 
                base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(sersObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.SersObservations;

        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            bool incrementalMode,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new SersObservationFactory(dataProvider, taxa);

            using var cursor = await _sersObservationVerbatimRepository.GetAllByCursorAsync();

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
                    await dwcArchiveFileWriterCoordinator.WriteObservations(observations, dataProvider);
                    observations.Clear();
                    Logger.LogDebug($"SERS Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var invalidObservations = ValidationManager.ValidateObservations(ref observations);
                await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                verbatimCount += await CommitBatchAsync(dataProvider, observations);
                await dwcArchiveFileWriterCoordinator.WriteObservations(observations, dataProvider);
                Logger.LogDebug($"SERS Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}