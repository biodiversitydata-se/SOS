using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;

namespace SOS.Process.Processors.Artportalen
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ArtportalenObservationProcessor : ObservationProcessorBase<ArtportalenObservationProcessor>,
        IArtportalenObservationProcessor
    {
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly IDiffusionManager _diffusionManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedProtectedObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IProcessedProtectedObservationRepository processedProtectedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IDiffusionManager diffusionManager,
            IValidationManager validationManager,
            ILogger<ArtportalenObservationProcessor> logger) : 
                base(processedPublicObservationRepository, processedProtectedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processConfiguration, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));

            _diffusionManager = diffusionManager ?? throw new ArgumentNullException(nameof(diffusionManager));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,            
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, _processedVocabularyRepository, mode != JobRunModes.Full);
            _artportalenVerbatimRepository.Mode = mode;

            var minId = 1;
            var maxId = await _artportalenVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<(int publicCount, int protectedCount)>>();

            while (minId <= maxId)
            {
                await SemaphoreBatch.WaitAsync();

                var batchEndId = minId + WriteBatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, minId, batchEndId, mode, observationFactory,
                    taxa, cancellationToken));
                minId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return (processBatchTasks.Sum(t => t.Result.publicCount), processBatchTasks.Sum(t => t.Result.protectedCount));
        }

        /// <summary>
        /// Process a batch of data
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="mode"></param>
        /// <param name="observationFactory"></param>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<(int publicCount, int protectedCount)> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            JobRunModes mode,
            ArtportalenObservationFactory observationFactory,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Artportalen batch ({startId}-{endId})");
                var verbatimObservationsBatch = await _artportalenVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Artportalen batch ({startId}-{endId})");

                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return (0,0);
                }

                Logger.LogDebug($"Start processing Artportalen batch ({startId}-{endId})");

                var publicObservations = new List<Observation>();
                var protectedObservations = new List<Observation>();

                foreach (var verbatimObservation in verbatimObservationsBatch)
                {
                    var taxonId = verbatimObservation.TaxonId ?? -1;
                    taxa.TryGetValue(taxonId, out var taxon);
                    var observation = observationFactory.CreateProcessedObservation(verbatimObservation, taxon);

                    if (observation == null)
                    {
                        continue;
                    }
                   
                    if (observation.Occurrence.ProtectionLevel > 2)
                    {
                        observation.Protected = true;
                        protectedObservations.Add(observation);

                        //If it is a protected sighting, public users should not be possible to find it in the current month 
                        if (!EnableDiffusion || (verbatimObservation?.StartDate.Value.Year == DateTime.Now.Year || verbatimObservation?.EndDate.Value.Year == DateTime.Now.Year) &&
                            (verbatimObservation?.StartDate.Value.Month == DateTime.Now.Month || verbatimObservation?.EndDate.Value.Month == DateTime.Now.Month))
                        {
                            continue;
                        }

                        // Recreate observation to make a new object
                        observation = observationFactory.CreateProcessedObservation(verbatimObservation, taxon);
                        // Diffuse protected observation before adding it to public index. Clone it to not affect protected obs
                        _diffusionManager.DiffuseObservation(observation);
                    }

                    // Add public observation
                    publicObservations.Add(observation);
                }

                Logger.LogDebug($"Finish processing Artportalen batch ({startId}-{endId})");

                var validateAndStoreTasks = new []
                {
                    ValidateAndStoreObservations(dataProvider, mode, false, publicObservations, $"{startId}-{endId}"),
                    ValidateAndStoreObservations(dataProvider, mode, true, protectedObservations,$"{startId}-{endId}")
                };
                await Task.WhenAll(validateAndStoreTasks);

                var publicCount = validateAndStoreTasks[0].Result;
                var protectedCount = validateAndStoreTasks[1].Result;

                return (publicCount, protectedCount); 
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw e;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process Artportalen sightings from id: {startId} to id: {endId} failed");
            }
            finally
            {
                SemaphoreBatch.Release();
            }

            return (0, 0);
        }
    }
}