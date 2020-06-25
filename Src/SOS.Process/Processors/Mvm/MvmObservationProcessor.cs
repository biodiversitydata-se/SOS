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
using SOS.Process.Processors.Mvm.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Mvm
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class MvmObservationProcessor : ObservationProcessorBase<MvmObservationProcessor>, IMvmObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mvmObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public MvmObservationProcessor(IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ILogger<MvmObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator,
            logger)
        {
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(mvmObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.MvmObservations;

        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new MvmObservationFactory(dataProvider, taxa);

            using var cursor = await _mvmObservationVerbatimRepository.GetAllByCursorAsync();

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
                    Logger.LogDebug($"MVM Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var committedObservations = await CommitBatchAsync(dataProvider, observations);
                verbatimCount += committedObservations?.Count() ?? 0;
                var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(committedObservations, dataProvider);
                Logger.LogDebug($"MVM Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}