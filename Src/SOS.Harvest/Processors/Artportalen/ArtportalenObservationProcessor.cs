using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ArtportalenObservationProcessor : ObservationProcessorBase<ArtportalenObservationProcessor, ArtportalenObservationVerbatim, IArtportalenVerbatimRepository>,
        IArtportalenObservationProcessor
    {
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly string _artPortalenUrl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="userObservationRepository"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ISightingRepository sightingRepository,
            IUserObservationRepository userObservationRepository,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenObservationProcessor> logger) : 
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, userObservationRepository, processConfiguration, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _artPortalenUrl = processConfiguration?.ArtportalenUrl ?? throw new ArgumentNullException(nameof(processConfiguration));
        }

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,            
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, 
                    taxa, 
                    _processedVocabularyRepository,
                    mode != JobRunModes.Full, 
                    _artPortalenUrl, 
                    TimeManager);
            _artportalenVerbatimRepository.Mode = mode;

            if (mode != JobRunModes.Full)
            {
                // If mode = IncrementalInactiveInstance, make sure we get all deleted since full harvest started. 24 hours should be more than enought.
                // Else 1 hour shold do it since we run incremental harvest every 5 min
                var from = DateTime.Now.AddHours(mode == JobRunModes.IncrementalInactiveInstance ? -24 : -1);
                var deletedIds = await _sightingRepository.GetDeletedIdsAsync(from);
                var rejectedIds = await _sightingRepository.GetRejectedIdsAsync(from);
                var idsToDelete = deletedIds?.Union(rejectedIds ?? Array.Empty<int>()) ?? rejectedIds;
                
                if (idsToDelete?.Any() ?? false)
                {
                    Logger.LogDebug($"Start deleting {idsToDelete.Count():N0} Artportalen sightings ({mode})");
                    var occurrenceIds = idsToDelete.Select(id => ArtportalenObservationFactory.GetOccurenceId(id));

                    await Task.WhenAll(new[]
                    {
                        ProcessedObservationRepository.DeleteByOccurrenceIdAsync(occurrenceIds, false),
                        ProcessedObservationRepository.DeleteByOccurrenceIdAsync(occurrenceIds, true)
                    });
                    Logger.LogDebug($"Finish deleting {idsToDelete.Count():N0} Artportalen sightings ({mode})");
                }


            }
            
            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _artportalenVerbatimRepository,
                cancellationToken);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <inheritdoc />
        public async Task<bool> ProcessObservationsAsync(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider,
                    taxa,
                    _processedVocabularyRepository,
                    true,
                    _artPortalenUrl,
                    TimeManager);

            var result = await base.ProcessBatchAsync(
                dataProvider,
                verbatimObservations,
                $"1-{verbatimObservations.Count()}",
                JobRunModes.IncrementalActiveInstance,
                observationFactory);

            return result.publicCount + result.protectedCount > 0;
        }
    }
}