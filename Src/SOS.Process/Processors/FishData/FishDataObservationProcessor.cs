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
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.FishData.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.FishData
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class FishDataObservationProcessor : ObservationProcessorBase<FishDataObservationProcessor>, IFishDataObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IFishDataObservationVerbatimRepository _fishDataObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fishDataObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public FishDataObservationProcessor(
            IFishDataObservationVerbatimRepository fishDataObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ILogger<FishDataObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator,
            logger)
        {
            _fishDataObservationVerbatimRepository = fishDataObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(fishDataObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.FishDataObservations;

        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new FishDataObservationFactory(dataProvider, taxa);

            using var cursor = await _fishDataObservationVerbatimRepository.GetAllByCursorAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var committedObservations = await CommitBatchAsync(dataProvider, observations);
                    verbatimCount += committedObservations?.Count() ?? 0;
                    var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(committedObservations, dataProvider);
                    observations.Clear();
                    Logger.LogDebug($"Fish Data Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var committedObservations = await CommitBatchAsync(dataProvider, observations);
                verbatimCount += committedObservations?.Count() ?? 0;
                var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(committedObservations, dataProvider);
                Logger.LogDebug($"Fish Data Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}