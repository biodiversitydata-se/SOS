using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Artportalen
{
    /// <summary>
    ///     Artportalen observation harvester
    /// </summary>
    public class ArtportalenCheckListHarvester : IArtportalenCheckListHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly IAreaHelper _areaHelper;
        private readonly IProjectRepository _projectRepository;
        private readonly SemaphoreSlim _semaphore;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IVerbatimRepositoryBase<ArtportalenCheckListVerbatim, int> _artportalenCheckListVerbatimRepository;
        private readonly ILogger<ArtportalenCheckListHarvester> _logger;

        /// <summary>
        /// Harvest all sightings
        /// </summary>
        /// <param name="harvestFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> HarvestAllAsync(ArtportalenCheckListHarvestFactory harvestFactory, IJobCancellationToken cancellationToken)
        {
            _checkListRepository.Live = false;

            // Get source min and max id
            var (minId, maxId) = await _checkListRepository.GetIdSpanAsync();

            // If minid is greater or equal to maxid
            if (minId >= maxId)
            {
                return -1;
            }

            var currentId = minId;
            var harvestBatchTasks = new List<Task<int>>();

            _logger.LogDebug($"Start getting Artportalen check lists");

            var batchIndex = 0;
            // Loop until all sightings are fetched
            while (currentId <= maxId)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                if (_artportalenConfiguration.MaxNumberOfCheckListsHarvested.HasValue &&
                    currentId - minId >= _artportalenConfiguration.MaxNumberOfCheckListsHarvested)
                {
                    break;
                }

                await _semaphore.WaitAsync();
                batchIndex++;

                // Add batch task to list
                harvestBatchTasks.Add(HarvestBatchAsync(harvestFactory,
                    _checkListRepository.GetChunkAsync(currentId, _artportalenConfiguration.ChunkSizeCheckLists),
                    batchIndex));

                // Calculate start of next chunk
                currentId += _artportalenConfiguration.ChunkSizeCheckLists;
            }

            // Execute harvest tasks, no of parallel threads running is handled by semaphore
            await Task.WhenAll(harvestBatchTasks);

            // Sum each batch harvested
            var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

            _logger.LogDebug($"Finish getting Artportalen check lists ({ nrSightingsHarvested })");

            return nrSightingsHarvested;

        }

        /// <summary>
        ///  Harvest a batch of sightings
        /// </summary>
        /// <param name="harvestFactory"></param>
        /// <param name="getChunkTask"></param>
        /// <param name="batchIndex"></param>
        /// <param name="incremenatlMode"></param>
        /// <returns></returns>
        private async Task<int> HarvestBatchAsync(
            ArtportalenCheckListHarvestFactory harvestFactory,
            Task<IEnumerable<CheckListEntity>> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                _logger.LogDebug(
                    $"Start getting Artportalen check lists ({batchIndex})");
                // Get chunk of sightings
                var chekLists = (await getChunkTask)?.ToArray();
                _logger.LogDebug(
                    $"Finish getting Artportalen check lists ({batchIndex})");

                if (!chekLists?.Any() ?? true)
                {
                    _logger.LogDebug(
                    $"No check lists found ({batchIndex})");
                    return 0;
                }

                _logger.LogDebug($"Start casting check list entities to verbatim ({batchIndex})");

                // Cast sightings to verbatim observations
                var verbatimObservations = await harvestFactory.CastEntitiesToVerbatimsAsync(chekLists);

                _logger.LogDebug($"Finish casting check list entities to verbatim ({batchIndex})");

                _logger.LogDebug($"Start storing check list batch ({batchIndex})");
                // Add sightings to mongodb

                await _artportalenCheckListVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug($"Finish storing check list batch ({batchIndex})");

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
                    $"Harvest Artportalen check lists ({batchIndex})");
                throw new Exception("Harvest Artportalen check lists failed");
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenConfiguration"></param>
        /// <param name="areaHelper"></param>
        /// <param name="projectRepository"></param>
        /// <param name="checkListRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="artportalenCheckListVerbatimRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenCheckListHarvester(
            ArtportalenConfiguration artportalenConfiguration,
            IAreaHelper areaHelper,
            IProjectRepository projectRepository,
            ICheckListRepository checkListRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            IVerbatimRepositoryBase<ArtportalenCheckListVerbatim, int> artportalenCheckListVerbatimRepository,
            ILogger<ArtportalenCheckListHarvester> logger)
        {
            _artportalenConfiguration = artportalenConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenConfiguration));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _checkListRepository = checkListRepository ?? throw new ArgumentNullException(nameof(checkListRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _artportalenCheckListVerbatimRepository = artportalenCheckListVerbatimRepository ??
                                                      throw new ArgumentNullException(
                                                          nameof(artportalenCheckListVerbatimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads);
        }


        /// inheritdoc />
        public async Task<HarvestInfo> HarvestCheckListsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

            try
            {
                // Populate data on full harvest or if it's not initialized
                _logger.LogDebug("Start getting check list metadata");
                var projectEntities = await _projectRepository.GetProjectsAsync();
                _logger.LogDebug("Finish getting check list metadata");

                _logger.LogDebug("Start creating factory");
                var harvestFactory = new ArtportalenCheckListHarvestFactory(
                    _areaHelper,
                    _checkListRepository,
                    _siteRepository,
                    _sightingRepository,
                    projectEntities,
                    _logger
                )
                {
                    IncrementalMode = false
                };
                _logger.LogDebug("Finish creating factory");

                _artportalenCheckListVerbatimRepository.Mode = JobRunModes.Full;

                // Make sure we have an empty public collection
                _logger.LogDebug("Start empty artportalen check list verbatim collection");
                await _artportalenCheckListVerbatimRepository.DeleteCollectionAsync();
                await _artportalenCheckListVerbatimRepository.AddCollectionAsync();
                _logger.LogDebug("Finish empty artportalen check list verbatim collection");

                var nrCheckListsHarvested = await HarvestAllAsync(harvestFactory, cancellationToken);

                // Update harvest info
                harvestInfo.Status = nrCheckListsHarvested >= 0 ? RunStatus.Success : RunStatus.Failed;

                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrCheckListsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Artportalen check list harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest artportalen check lists");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestCheckListsAsync(DataProvider provider,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }
    }
}