using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.ObservationDatabase.Interfaces;

namespace SOS.Process.Processors.ObservationDatabase
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ObservationDatabaseProcessor : ObservationProcessorBase<ObservationDatabaseProcessor>, IObservationDatabaseProcessor
    {
        private readonly IObservationDatabaseVerbatimRepository _observationDatabaseVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly IDiffusionManager _diffusionManager;

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var publicBatchId = 0;
            var protectedBatchId = 0;
            var publicProcessedCount = 0;
            var protectedProcessedCount = 0;
            var publicObservations = new List<Observation>();
            var protectedObservations = new List<Observation>();
            var observationFactory = new ObservationDatabaseObservationFactory(dataProvider, taxa);

            using var cursor = await _observationDatabaseVerbatimRepository.GetAllByCursorAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                cancellationToken?.ThrowIfCancellationRequested();

                var observation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(observation);

                if (observation.Occurrence.ProtectionLevel > 2)
                {
                    observation.Protected = true;
                    protectedObservations.Add(observation);

                    if (IsBatchFilledToLimit(protectedObservations.Count))
                    {
                        protectedBatchId++;

                        protectedProcessedCount += await ValidateAndStoreObservation(dataProvider, true, protectedObservations, protectedBatchId.ToString());
                        protectedObservations.Clear();
                        Logger.LogDebug($"Observation database protected observations processed: {protectedProcessedCount}");
                    }

                    //If it is a protected sighting, public users should not be possible to find it in the current month 
                    if ((verbatimObservation.StartDate.Year == DateTime.Now.Year || verbatimObservation.EndDate.Year == DateTime.Now.Year) &&
                        (verbatimObservation.StartDate.Month == DateTime.Now.Month || verbatimObservation.EndDate.Month == DateTime.Now.Month))
                    {
                        return;
                    }

                    // Recreate observation to make a new object
                    observation = observationFactory.CreateProcessedObservation(verbatimObservation);
                    // Diffuse protected observation before adding it to public index. Clone it to not affect protected obs
                    _diffusionManager.DiffuseObservation(observation);
                }

                // Add public observation
                publicObservations.Add(observation);

                if (IsBatchFilledToLimit(publicObservations.Count))
                {
                    publicBatchId++;

                    publicProcessedCount += await ValidateAndStoreObservation(dataProvider, false, publicObservations, publicBatchId.ToString());
                    publicObservations.Clear();
                    Logger.LogDebug($"Observation database public observations processed: {publicProcessedCount}");
                }
            });

            cancellationToken?.ThrowIfCancellationRequested();

            // Commit remaining batch (not filled to limit).
            if (protectedObservations.Any())
            {
                protectedBatchId++;

                protectedProcessedCount += await ValidateAndStoreObservation(dataProvider, true, protectedObservations, protectedBatchId.ToString());
                protectedObservations.Clear();
                Logger.LogDebug($"Observation database protected observations processed: {protectedProcessedCount}");
            }

            if (publicObservations.Any())
            {
                publicBatchId++;

                publicProcessedCount += await ValidateAndStoreObservation(dataProvider, false, publicObservations, publicBatchId.ToString());
                publicObservations.Clear();
                Logger.LogDebug($"Observation database public observations processed: {publicProcessedCount}");
            }

            return protectedProcessedCount;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationDatabaseVerbatimRepository"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedProtectedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ObservationDatabaseProcessor(IObservationDatabaseVerbatimRepository observationDatabaseVerbatimRepository,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IProcessedProtectedObservationRepository processedProtectedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IDiffusionManager diffusionManager,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            ILogger<ObservationDatabaseProcessor> logger) : 
                base(processedPublicObservationRepository, processedProtectedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _observationDatabaseVerbatimRepository = observationDatabaseVerbatimRepository ??
                                                     throw new ArgumentNullException(nameof(observationDatabaseVerbatimRepository));
            _diffusionManager = diffusionManager ?? throw new ArgumentNullException(nameof(diffusionManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.ObservationDatabase;
    }
}