﻿using System;
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
using SOS.Process.Processors.FishData.Interfaces;

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
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public FishDataObservationProcessor(
            IFishDataObservationVerbatimRepository fishDataObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<FishDataObservationProcessor> logger) : 
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _fishDataObservationVerbatimRepository = fishDataObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(fishDataObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.FishDataObservations;

        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<Observation> observations = new List<Observation>();
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
                    var invalidObservations = ValidationManager.ValidateObservations(ref observations,dataProvider);
                    await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                    verbatimCount += await CommitBatchAsync(dataProvider, observations);
                    await WriteObservationsToDwcaCsvFiles(observations, dataProvider);
                    observations.Clear();
                    Logger.LogDebug($"Fish Data Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                var invalidObservations = ValidationManager.ValidateObservations(ref observations, dataProvider);
                await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
                verbatimCount += await CommitBatchAsync(dataProvider, observations);
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider);
                Logger.LogDebug($"Fish Data Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}