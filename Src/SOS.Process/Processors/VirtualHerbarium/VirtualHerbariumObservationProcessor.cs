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

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var batchId = 0;
                var processedCount = 0;
                ICollection<Observation> observations = new List<Observation>();
                var observationFactory = new VirtualHerbariumObservationFactory(dataProvider, taxa);
                
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
                            cancellationToken?.ThrowIfCancellationRequested();

                            batchId++;

                            processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                            observations.Clear();
                            Logger.LogDebug($"Virtual Herbarium observations processed: {processedCount}");
                        }
                    }
                });

                // Commit remaining batch (not filled to limit).
                if (observations.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    batchId++;

                    processedCount += await ValidateAndStoreObservation(dataProvider, false, observations, batchId.ToString());
                    observations.Clear();
                    Logger.LogDebug($"Virtual Herbarium observations processed: {processedCount}");
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
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationProcessor(
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<VirtualHerbariumObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ??
                                                             throw new ArgumentNullException(
                                                                 nameof(virtualHerbariumObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.VirtualHerbariumObservations;

    }
}