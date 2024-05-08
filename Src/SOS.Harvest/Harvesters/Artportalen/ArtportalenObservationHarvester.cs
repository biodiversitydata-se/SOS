﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Artportalen
{
    /// <summary>
    ///     Artportalen observation harvester
    /// </summary>
    public class ArtportalenObservationHarvester : ObservationHarvesterBase<ArtportalenObservationVerbatim, int>, IArtportalenObservationHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IArtportalenMetadataContainer _artportalenMetadataContainer;
        private readonly IAreaHelper _areaHelper;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Get harvest factory
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ArtportalenHarvestFactory> PrepareHarvestAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            SetRunMode(mode);

            // Populate data on full harvest or if it's not initialized
            if (mode == JobRunModes.Full || !_artportalenMetadataContainer.IsInitialized)
            {
                await _artportalenMetadataContainer.InitializeAsync();
                cancellationToken?.ThrowIfCancellationRequested();
            }

            Logger.LogDebug("Start creating factory");
            var harvestFactory = new ArtportalenHarvestFactory(
                _mediaRepository,
                _projectRepository,
                _sightingRepository,
                _siteRepository,
                _sightingRelationRepository,
                _speciesCollectionRepository,
                _artportalenMetadataContainer,
                _areaHelper,
                _artportalenConfiguration.NoOfThreads,
                Logger
            );
            Logger.LogDebug("Finish creating factory");

            if (mode.Equals(JobRunModes.Full))
            {
                Logger.LogDebug("Start caching sites");
                await harvestFactory.CacheFreqventlyUsedSitesAsync();
                Logger.LogDebug("Finish caching sites");
            }

            return harvestFactory;
        }

        #region Full
        /// <summary>
        /// Harvest all sightings
        /// </summary>
        /// <param name="harvestFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> HarvestAllAsync(ArtportalenHarvestFactory harvestFactory, IJobCancellationToken cancellationToken)
        {
            var backupDate = await _metadataRepository.GetLastBackupDateAsync();
            if (_artportalenConfiguration.ValidateDataBaseBackup && backupDate < DateTime.Now.AddDays(-2))
            {
                throw new Exception($"Artportalen backup to old ({backupDate}). Stopping harvest");
            }

            Logger.LogInformation($"Start Artportalen HarvestAllAsync()");
            if (_artportalenConfiguration.AddTestSightings && (_artportalenConfiguration.AddTestSightingIds?.Any() ?? false))
            {
                Logger.LogDebug("Start adding test sightings");
                await _semaphore.WaitAsync();
                await HarvestBatchAsync(harvestFactory,
                       _sightingRepository.GetChunkAsync(_artportalenConfiguration.AddTestSightingIds),
                        0);

                Logger.LogDebug("Finish adding test sightings");
            }

            // Get source min and max id
            var (minId, maxId) = await _sightingRepository.GetIdSpanAsync();

            // MaxNumberOfSightingsHarvested is a debug feature. If it's set calculate minid to get last sightings.
            // This make it easier to test incremental harvest since it has a max limit from last modified
            if (_artportalenConfiguration.MaxNumberOfSightingsHarvested.HasValue && _artportalenConfiguration.MaxNumberOfSightingsHarvested > 0)
            {
                minId = maxId - _artportalenConfiguration.MaxNumberOfSightingsHarvested.Value;
            }

            // If maxid is greater than min id
            if (maxId > minId)
            {
                var currentId = minId;
                var harvestBatchTasks = new List<Task<int>>();

                Logger.LogDebug($"Start getting Artportalen sightings");

                var batchIndex = 0;
                // Loop until all sightings are fetched
                while (currentId <= maxId)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    await _semaphore.WaitAsync();
                    batchIndex++;

                    // Add batch task to list
                    harvestBatchTasks.Add(HarvestBatchAsync(harvestFactory,
                        _sightingRepository.GetChunkAsync(currentId, _artportalenConfiguration.ChunkSize),
                        batchIndex));

                    // Calculate start of next chunk
                    currentId += _artportalenConfiguration.ChunkSize;
                }

                // Execute harvest tasks, no of parallel threads running is handled by semaphore
                await Task.WhenAll(harvestBatchTasks);

                // Sum each batch harvested
                var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

                Logger.LogDebug($"Finish getting Artportalen sightings ({nrSightingsHarvested})");
                Logger.LogInformation($"Finish Artportalen HarvestAllAsync(). NrSightingsHarvested={nrSightingsHarvested:N0}");
                return nrSightingsHarvested;
            }

            return -1;
        }
        #endregion Full

        #region Incremental
        /// <summary>
        /// Harvest incremental
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="fromDate"></param>
        /// <param name="harvestFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> HarvestIncrementalAsync(JobRunModes mode, DateTime? fromDate, ArtportalenHarvestFactory harvestFactory,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Artportalen HarvestIncrementalAsync()");
            Logger.LogDebug($"Start getting Artportalen sightings ({mode})");

            // If no from date is passed, we start from last harvested sighting 
            var harvestFromDate = fromDate ?? (await _processedObservationRepository.GetLatestModifiedDateForProviderAsync(1));
            
            // Don't harvest too many days
            if (_artportalenConfiguration.MaxNumberOfDaysHarvested.HasValue)
            {
                int daysToHarvest = (DateTime.Now - harvestFromDate).Days;
                if (daysToHarvest > _artportalenConfiguration.MaxNumberOfDaysHarvested.Value)
                {
                    Logger.LogWarning($"Artportalen incremental harvest cancelled. harvestFromDate is too old: {harvestFromDate.ToLongDateString()}. Limit is {_artportalenConfiguration.MaxNumberOfDaysHarvested.Value} days");
                    return 0;
                }
            }

            // Get list of id's to Make sure we don't harvest more than #limit 
            var idBatch = (await _sightingRepository.GetModifiedIdsAsync(harvestFromDate, _artportalenConfiguration.IncrementalChunkSize))?.ToArray();
            var batchCount = 0;
            var nrSightingsHarvested = 0;
            while ((idBatch?.Length ?? 0) != 0)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                batchCount++;
                var idsToHarvest = idBatch!.Select(m => m.Id);
                var getObservationsTask = _sightingRepository.GetChunkAsync(idsToHarvest);
                await _semaphore.WaitAsync();
                var observationCount = await HarvestBatchAsync(harvestFactory, getObservationsTask, batchCount);
                nrSightingsHarvested += observationCount;

                // Delete observations we can't find
                var deletedIds = idsToHarvest!.Where(i => !getObservationsTask?.Result?.Select(o => o.Id).Contains(i) ?? true).Select(i => i);
                if (deletedIds?.Any() ?? false)
                {
                    await _processedObservationRepository.DeleteByOccurrenceIdAsync(deletedIds.Select(i => $"urn:lsid:artportalen.se:sighting:{i}"), false);
                    await _processedObservationRepository.DeleteByOccurrenceIdAsync(deletedIds.Select(i => $"urn:lsid:artportalen.se:sighting:{i}"), true);
                }

                if (observationCount == 0 || nrSightingsHarvested >= _artportalenConfiguration.CatchUpLimit)
                {
                    break;
                }

                Logger.LogInformation($"HarvestIncrementalAsync(). getObservationsTask!.Result!.LastOrDefault()!.EditDate={getObservationsTask!.Result!.LastOrDefault()!.EditDate}");
                Logger.LogInformation($"HarvestIncrementalAsync(). idBatch.Last.EditDate={idBatch!.Last().EditDate}");
                var harvestDate = idBatch!.Last().EditDate;
                var harvestDate1 = harvestDate.ToLocalTime();                
                var harvestDate2 = DateTime.SpecifyKind(harvestDate, DateTimeKind.Local);
                Logger.LogInformation($"HarvestDate1={harvestDate1}");
                Logger.LogInformation($"HarvestDate2={harvestDate2}");

                idBatch = (await _sightingRepository.GetModifiedIdsAsync(harvestDate2, _artportalenConfiguration.IncrementalChunkSize))?.ToArray();
            }

            Logger.LogDebug($"Finish getting Artportalen sightings ({mode}) (NrSightingsHarvested={nrSightingsHarvested:N0})");
            Logger.LogInformation($"Finish Artportalen HarvestIncrementalAsync(). NrSightingsHarvested={nrSightingsHarvested:N0}");
            return nrSightingsHarvested;
        }
        #endregion Incremental

        /// <summary>
        /// Harvest a batch of sightings
        /// </summary>
        /// <param name="harvestFactory"></param>
        /// <param name="getChunkTask"></param>
        /// <param name="batchIndex"></param>
        /// <param name="incrementalMode"></param>
        /// <param name="storeVerbatim"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IEnumerable<ArtportalenObservationVerbatim>?> GetVerbatimBatchAsync(
            ArtportalenHarvestFactory harvestFactory,
            Task<IEnumerable<SightingEntity>?> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                Logger.LogDebug(
                    $"Start getting Artportalen sightings (BatchIndex={batchIndex})");
                // Get chunk of sightings
                var sightings = (await getChunkTask)?.ToArray();
                Logger.LogDebug(
                    $"Finish getting Artportalen sightings (BatchIndex={batchIndex})");

                if (!sightings?.Any() ?? true)
                {
                    Logger.LogDebug(
                    $"No sightings found (BatchIndex: {batchIndex})");
                    return null;
                }

                Logger.LogDebug($"Start casting entities batch to verbatim ({batchIndex})");

                // Cast sightings to verbatim observations
                var verbatimObservations = await harvestFactory.CastEntitiesToVerbatimsAsync(sightings!);
                Logger.LogDebug($"Finish casting entities batch to verbatim ({batchIndex})");

                return verbatimObservations;
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"Harvest Artportalen sightings batch ({batchIndex}) failed");
                throw;
            }
        }

        private async Task<int> HarvestBatchAsync(
            ArtportalenHarvestFactory harvestFactory,
            Task<IEnumerable<SightingEntity>?> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                var verbatimObservations = await GetVerbatimBatchAsync(harvestFactory, getChunkTask, batchIndex);

                if (!verbatimObservations?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Start storing batch ({batchIndex})");
                if (!await VerbatimRepository.AddManyAsync(verbatimObservations))
                {
                    throw new Exception($"Failed to store verbatims batch: {batchIndex}.");
                }

                // If sleep is required to free resources to other systems
                if (_artportalenConfiguration.SleepAfterBatch > 0)
                {
                    Thread.Sleep(_artportalenConfiguration.SleepAfterBatch);
                }

                Logger.LogDebug($"Finish storing batch ({batchIndex})");

                return verbatimObservations!.Count();
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"Harvest Artportalen sightings batch ({batchIndex}) failed");
                throw;
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }
        }

        private void SetRunMode(JobRunModes mode)
        {
            // Use active index if it's a incremental active instance harvest 
            _processedObservationRepository.LiveMode = mode == JobRunModes.IncrementalActiveInstance;
            VerbatimRepository.Mode = mode;

            // Incremental harvest always use live AP data
            var live = mode != JobRunModes.Full;
            _artportalenMetadataContainer.Live = live;
            _mediaRepository.Live = live;
            _metadataRepository.Live = live;
            _projectRepository.Live = live;
            _sightingRelationRepository.Live = live;
            _sightingRepository.Live = live;
            _siteRepository.Live = live;
            _speciesCollectionRepository.Live = live;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenConfiguration"></param>
        /// <param name="mediaRepository"></param>
        /// <param name="metadataRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="sightingRelationRepository"></param>
        /// <param name="speciesCollectionItemRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="artportalenMetadataContainer"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationHarvester(
            ArtportalenConfiguration artportalenConfiguration,
            IMediaRepository mediaRepository,
            IMetadataRepository metadataRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            IArtportalenVerbatimRepository artportalenVerbatimRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionItemRepository,
            IProcessedObservationCoreRepository processedObservationRepository,
            IArtportalenMetadataContainer artportalenMetadataContainer,
            IAreaHelper areaHelper,
            ILogger<ArtportalenObservationHarvester> logger) : base("Artportalen", artportalenVerbatimRepository, logger)
        {
            _artportalenConfiguration = artportalenConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenConfiguration));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _sightingRelationRepository = sightingRelationRepository ??
                                          throw new ArgumentNullException(nameof(sightingRelationRepository));
            _speciesCollectionRepository = speciesCollectionItemRepository ??
                                           throw new ArgumentNullException(nameof(speciesCollectionItemRepository));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _artportalenMetadataContainer = artportalenMetadataContainer ?? throw new ArgumentNullException(nameof(artportalenMetadataContainer));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads, artportalenConfiguration.NoOfThreads);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            var dataLastModified = (DateTime?)null;
            var notes = (string?)null;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                using var harvestFactory = await PrepareHarvestAsync(mode, cancellationToken);
                initValues.preHarvestCount = await InitializeHarvestAsync(false);

                harvestCount = mode == JobRunModes.Full ?
                    await HarvestAllAsync(harvestFactory, cancellationToken)
                    :
                    await HarvestIncrementalAsync(mode, fromDate, harvestFactory, cancellationToken);
                harvestFactory.Dispose();

                // Update harvest info
                runStatus = harvestCount >= 0 ? RunStatus.Success : RunStatus.Failed;

                if (mode == JobRunModes.Full)
                {
                    dataLastModified = await _sightingRepository.GetLastModifiedDateAsyc();

                    var lastBackupDate = await _metadataRepository.GetLastBackupDateAsync();
                    notes = lastBackupDate.HasValue ? $"Database backup restore: {lastBackupDate}" : null;
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Artportalen harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Harvest Artportalen observations failed");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount, dataLastModified, notes);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException("Not implemented for this provider");
            });
            return null!;
        }

        /// inheritdoc />
        public async Task<IEnumerable<ArtportalenObservationVerbatim>?> HarvestObservationsAsync(IEnumerable<int> ids,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var mode = JobRunModes.IncrementalActiveInstance;
                using var harvestFactory = await PrepareHarvestAsync(mode, cancellationToken);

                return await GetVerbatimBatchAsync(harvestFactory,
                    _sightingRepository.GetChunkAsync(ids),
                    1);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Harvest Artportalen observations by id was cancelled.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Harvest Artportalen observations by id failed");
            }

            return null;
        }
    }
}