using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Artportalen
{
    /// <summary>
    ///     Artportalen observation harvester
    /// </summary>
    public class ArtportalenObservationHarvester : IArtportalenObservationHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IArtportalenMetadataContainer _artportalenMetadataContainer;
        private readonly IAreaHelper _areaHelper;
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger<ArtportalenObservationHarvester> _logger;

        /// <summary>
        /// Get harvest factory
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ArtportalenHarvestFactory> GetHarvestFactoryAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            SetRunMode(mode);
 
            // Populate data on full harvest or if it's not initialized
            if (mode == JobRunModes.Full || !_artportalenMetadataContainer.IsInitialized)
            {
                await _artportalenMetadataContainer.InitializeAsync();
                cancellationToken?.ThrowIfCancellationRequested();
            }

            _logger.LogDebug("Start creating factory");
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
                _logger
            );
            _logger.LogDebug("Finish creating factory");

            if (mode.Equals(JobRunModes.Full))
            {
                _logger.LogDebug("Start caching sites");
                await harvestFactory.CacheFreqventlyUsedSitesAsync();
                _logger.LogDebug("Finish caching sites");
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
            if (_artportalenConfiguration.AddTestSightings && (_artportalenConfiguration.AddTestSightingIds?.Any() ?? false))
            { 
                _logger.LogDebug("Start adding test sightings");
                await _semaphore.WaitAsync();
                await HarvestBatchAsync(harvestFactory,
                       _sightingRepository.GetChunkAsync(_artportalenConfiguration.AddTestSightingIds),
                        0);

                _logger.LogDebug("Finish adding test sightings");
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

                _logger.LogDebug($"Start getting Artportalen sightings");
                
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

                _logger.LogDebug($"Finish getting Artportalen sightings ({ nrSightingsHarvested })");

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
        /// <param name="harvestFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> HarvestIncrementalAsync(JobRunModes mode, ArtportalenHarvestFactory harvestFactory, 
            IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug($"Start getting Artportalen sightings ({mode})");

            // We start from last harvested sighting 
            var lastModified = await _processedObservationRepository.GetLatestModifiedDateForProviderAsync(1);
            lastModified = lastModified.AddMinutes(-1); // When we force fetching data from AP (get one observation), we mess up the logic. Back track 1 min to make sure we get all modified obseervations

            // Get list of id's to Make sure we don't harvest more than #limit 
            var idsToHarvest = (await _sightingRepository.GetModifiedIdsAsync(lastModified, _artportalenConfiguration.CatchUpLimit))?.ToArray();
            _logger.LogDebug($"Number of Artportalen Ids to harvest: {idsToHarvest?.Length ?? 0}, lastModifiedQuery={lastModified} ({mode})");

            if (!idsToHarvest?.Any() ?? true)
            {
                return 0;
            }

            // Decrease chunk size for incremental harvest since the SQL query is slower 
            var harvestBatchTasks = new List<Task<int>>();

            var idBatch = idsToHarvest.Skip(0).Take(_artportalenConfiguration.IncrementalChunkSize);
            var batchCount = 0;

            // Loop until all sightings are fetched
            while (idBatch.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();

                await _semaphore.WaitAsync();

                batchCount++;

                // Add batch task to list
                harvestBatchTasks.Add(HarvestBatchAsync(harvestFactory,
                    _sightingRepository.GetChunkAsync(idBatch),
                    batchCount));

                idBatch = idsToHarvest.Skip(batchCount * _artportalenConfiguration.IncrementalChunkSize).Take(_artportalenConfiguration.IncrementalChunkSize);
            }

            // Execute harvest tasks, no of parallel threads running is handled by semaphore
            await Task.WhenAll(harvestBatchTasks);

            // Sum each batch harvested
            var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

            _logger.LogDebug($"Finish getting Artportalen sightings ({mode}) (NrSightingsHarvested={nrSightingsHarvested:N0})");

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
            Task<IEnumerable<SightingEntity>> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                _logger.LogDebug(
                    $"Start getting Artportalen sightings (BatchIndex={batchIndex})");
                // Get chunk of sightings
                var sightings = (await getChunkTask)?.ToArray();
                _logger.LogDebug(
                    $"Finish getting Artportalen sightings (BatchIndex={batchIndex})");

                if (!sightings?.Any() ?? true)
                {
                    _logger.LogDebug(
                    $"No sightings found (BatchIndex={batchIndex})");
                    return null;
                }

                _logger.LogDebug($"Start casting entities to verbatim ({batchIndex})");

                // Cast sightings to verbatim observations
                var verbatimObservations = await harvestFactory.CastEntitiesToVerbatimsAsync(sightings!);
                _logger.LogDebug($"Finish casting entities to verbatim ({batchIndex})");

                return verbatimObservations;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Harvest Artportalen sightings ({batchIndex}) failed");
                throw new Exception("Harvest Artportalen batch failed");
            }
        }

        private async Task<int> HarvestBatchAsync(
            ArtportalenHarvestFactory harvestFactory,
            Task<IEnumerable<SightingEntity>> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                var verbatimObservations = await GetVerbatimBatchAsync(harvestFactory, getChunkTask, batchIndex);

                _logger.LogDebug($"Start storing batch ({batchIndex})");
                await _artportalenVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug($"Finish storing batch ({batchIndex})");

                // If sleep is required to free resources to other systems
                if (_artportalenConfiguration.SleepAfterBatch > 0)
                {
                    Thread.Sleep(_artportalenConfiguration.SleepAfterBatch);
                }

                return verbatimObservations?.Count() ?? 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Harvest Artportalen sightings ({batchIndex}) failed");
                throw new Exception("Harvest Artportalen batch failed");
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
            var live = mode == JobRunModes.IncrementalActiveInstance;

            _processedObservationRepository.LiveMode = live;
            _artportalenVerbatimRepository.Mode = mode;
            _mediaRepository.Live = live;
            _projectRepository.Live = live;
            _sightingRelationRepository.Live = live;
            _sightingRepository.Live = live;
            _siteRepository.Live = live;
            _speciesCollectionRepository.Live = live;

            _artportalenMetadataContainer.Live = live;
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
            ILogger<ArtportalenObservationHarvester> logger)
        {
            _artportalenConfiguration = artportalenConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenConfiguration));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _sightingRelationRepository = sightingRelationRepository ??
                                          throw new ArgumentNullException(nameof(sightingRelationRepository));
            _speciesCollectionRepository = speciesCollectionItemRepository ??
                                           throw new ArgumentNullException(nameof(speciesCollectionItemRepository));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _artportalenMetadataContainer = artportalenMetadataContainer ?? throw new ArgumentNullException(nameof(artportalenMetadataContainer));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads, artportalenConfiguration.NoOfThreads);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("Artportalen", DateTime.Now);
            _logger.LogInformation($"Start harvesting sightings for Artportalen data provider. Mode={mode}");

            try
            {
                var harvestFactory = await GetHarvestFactoryAsync(mode, cancellationToken);

                // Make sure we have an empty public collection
                _logger.LogDebug("Start empty artportalen verbatim collection");
                await _artportalenVerbatimRepository.DeleteCollectionAsync();
                await _artportalenVerbatimRepository.AddCollectionAsync();
                _logger.LogDebug("Finish empty artportalen verbatim collection");

                var nrSightingsHarvested = mode == JobRunModes.Full ?
                    await HarvestAllAsync(harvestFactory, cancellationToken)
                    :
                    await HarvestIncrementalAsync(mode, harvestFactory, cancellationToken);
                harvestFactory.Dispose();

                // Update harvest info
                harvestInfo.Status = nrSightingsHarvested >= 0 ? RunStatus.Success : RunStatus.Failed;
                
                if (mode == JobRunModes.Full)
                {
                    harvestInfo.DataLastModified = await _sightingRepository.GetLastModifiedDateAsyc();

                    var lastBackupDate = await _metadataRepository.GetLastBackupDateAsync();
                    harvestInfo.Notes = lastBackupDate.HasValue ? $"Database backup restore: {lastBackupDate}" : null;
                }
                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Artportalen harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed aggregation of sightings");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation($"Finish harvesting sightings for Artportalen data provider. Mode={mode}, Status={harvestInfo.Status}");
            return harvestInfo;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<IEnumerable<ArtportalenObservationVerbatim>?> HarvestObservationsAsync(IEnumerable<int> ids,
            IJobCancellationToken cancellationToken)
        {
            try
            {
               var mode = JobRunModes.IncrementalActiveInstance;
               using var harvestFactory = await GetHarvestFactoryAsync(mode, cancellationToken);

                return await GetVerbatimBatchAsync(harvestFactory,
                    _sightingRepository.GetChunkAsync(ids),
                    1);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Harvest Artportalen observations by id was cancelled.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Harvest Artportalen observations by id failed");
            }

            return null;
        }
    }
}