using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;

namespace SOS.Process.Processors.VirtualHerbarium
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class VirtualHerbariumObservationProcessor : ObservationProcessorBase<VirtualHerbariumObservationProcessor>,
        IVirtualHerbariumObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IVirtualHerbariumObservationVerbatimRepository _virtualHerbariumObservationVerbatimRepository;

        private async Task<int> AddBatch(DataProvider dataProvider, ICollection<Observation> observations)
        {
           
            var invalidObservations = ValidationManager.ValidateObservations(ref observations, dataProvider);
            await ValidationManager.AddInvalidObservationsToDb(invalidObservations);
            var processedCount = await CommitBatchAsync(dataProvider, observations);
            await WriteObservationsToDwcaCsvFiles(observations, dataProvider);

            Logger.LogDebug($"Virtual Herbarium Sightings processed: {processedCount}");

            return processedCount;
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processedCount = 0;
                var observationFactory = new VirtualHerbariumObservationFactory(dataProvider, taxa);
                ICollection<Observation> observations = new List<Observation>();
                using var cursor = await _virtualHerbariumObservationVerbatimRepository.GetAllByCursorAsync();

                // Process and commit in batches.
                await cursor.ForEachAsync(async verbatimObservation =>
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);

                    if (processedObservation != null)
                    {
                        _areaHelper.AddAreaDataToProcessedObservation(processedObservation);

                        observations.Add(processedObservation);
                        if (IsBatchFilledToLimit(observations.Count))
                        {
                            processedCount += await AddBatch(dataProvider, observations);
                            observations = new List<Observation>();
                        }
                    }
                });

                // Commit remaining batch (not filled to limit).
                if (observations.Any())
                {
                    processedCount += await AddBatch(dataProvider, observations);
                }

                return processedCount;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process Virtual Herbarium Sightings");
                return 0;
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationProcessor(
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<VirtualHerbariumObservationProcessor> logger) : 
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ??
                                                             throw new ArgumentNullException(
                                                                 nameof(virtualHerbariumObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.VirtualHerbariumObservations;

    }
}