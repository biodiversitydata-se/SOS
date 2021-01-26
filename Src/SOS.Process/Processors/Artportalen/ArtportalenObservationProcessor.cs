﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
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
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Delete batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="verbatimIds"></param>
        /// <returns></returns>
        private async Task<bool> DeleteBatchAsync(
            ICollection<int> sightingIds)
        {
            try
            {
                return await ProcessRepository.DeleteArtportalenBatchAsync(sightingIds);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to delete AP batch");
                return false;
            }

        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationProcessor(IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IDiffusionManager diffusionManager,
            IValidationManager validationManager,
            ILogger<ArtportalenObservationProcessor> logger) : 
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
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

            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <inheritdoc />
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,            
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory =
                await ArtportalenObservationFactory.CreateAsync(dataProvider, _processedVocabularyRepository, mode != JobRunModes.Full);
            _artportalenVerbatimRepository.IncrementalMode = mode != JobRunModes.Full;


            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + ProcessRepository.WriteBatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, batchStartId, batchEndId, mode, observationFactory,
                    taxa, cancellationToken));
                batchStartId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
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
        private async Task<int> ProcessBatchAsync(
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
                    return 0;
                }

                Logger.LogDebug($"Start processing Artportalen batch ({startId}-{endId})");

                var publicObservations = new List<Observation>();
                var protectedObservations = new List<Observation>();

                foreach (var verbatimObservation in verbatimObservationsBatch)
                {
                    var taxonId = verbatimObservation.TaxonId ?? -1;
                    if (taxa.TryGetValue(taxonId, out var taxon))
                    {
                        if (!string.IsNullOrEmpty(verbatimObservation.URL))
                        {
                            // If we gone change properties for referenced taxon, we need to make a new object
                            taxon = taxon.Clone();
                            taxon.IndividualId = verbatimObservation.URL;
                        }
                    }

                    var observation = observationFactory.CreateProcessedObservation(verbatimObservation, taxon);

                    if (observation == null)
                    {
                        continue;
                    }
                   
                    if (observation.ProtectionLevel > 2)
                    {
                        observation.Protected = true;
                        protectedObservations.Add(observation);

                        //If it is a protected sighting, public users should not be possible to find it in the current month 
                        if ((verbatimObservation?.StartDate.Value.Year == DateTime.Now.Year || verbatimObservation?.EndDate.Value.Year == DateTime.Now.Year) &&
                            (verbatimObservation?.StartDate.Value.Month == DateTime.Now.Month || verbatimObservation?.EndDate.Value.Month == DateTime.Now.Month))
                        {
                            continue;
                        }

                        // Diffuse protected observation before adding it to public index. Must recreate observation since cloned observation fails to store in ElasticSearch
                        observation = observationFactory.CreateProcessedObservation(verbatimObservation, taxon);
                        _diffusionManager.DiffuseObservation(observation);
                    }

                    // Add public observation
                    publicObservations.Add(observation);
                }

                Logger.LogDebug($"Finish processing Artportalen batch ({startId}-{endId})");
               
                var publicCount = await ValidateAndStoreObservations(dataProvider, publicObservations, mode, false, $"{startId}-{endId}", cancellationToken);
                var protectedCount = await ValidateAndStoreObservations(dataProvider, protectedObservations, mode, true,$"{startId}-{endId}", cancellationToken);

                return publicCount; // Since public contains protected (diffused) observations, we return public count             // + protectedCount;
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
                _semaphore.Release();
            }

            return 0;
        }

        private async Task<int> ValidateAndStoreObservations(
            DataProvider dataProvider,
            ICollection<Observation> observations,
            JobRunModes mode, 
            bool protectedObservations,
            string batchId,
            IJobCancellationToken cancellationToken)
        {
            cancellationToken?.ThrowIfCancellationRequested();

            if (!observations?.Any() ?? true)
            {
                return 0;
            }

            observations = await ValidateAndRemoveInvalidObservations(dataProvider, observations, batchId);

            Protected = protectedObservations;

            if (mode != JobRunModes.Full)
            {
                Logger.LogDebug($"Start deleteing live data ({batchId})");
                var success = await DeleteBatchAsync(observations.Select(v => v.ArtportalenInternal.SightingId).ToArray());
                Logger.LogDebug($"Finish deleteing live data ({batchId}) {success}");
            }

            var verbatimCount = await CommitBatchAsync(dataProvider, observations, batchId);

            if (mode == JobRunModes.Full && !protectedObservations)
            {
                await WriteObservationsToDwcaCsvFiles(observations, dataProvider, batchId);
            }

            observations.Clear();
            Logger.LogDebug($"Artportalen sightings processed: {verbatimCount}");

            return verbatimCount;
        }
    }
}