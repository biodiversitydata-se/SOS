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
    public class ArtportalenChecklistHarvester : IArtportalenChecklistHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly IAreaHelper _areaHelper;
        private readonly IProjectRepository _projectRepository;
        private readonly SemaphoreSlim _semaphore;
        private readonly IChecklistRepository _checklistRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> _artportalenChecklistVerbatimRepository;
        private readonly ILogger<ArtportalenChecklistHarvester> _logger;

        /// <summary>
        /// Harvest all sightings
        /// </summary>
        /// <param name="harvestFactory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> HarvestAllAsync(ArtportalenChecklistHarvestFactory harvestFactory, IJobCancellationToken cancellationToken)
        {
            _checklistRepository.Live = false;

            // Get source min and max id
            var (minId, maxId) = await _checklistRepository.GetIdSpanAsync();

            // If minid is greater or equal to maxid
            if (minId >= maxId)
            {
                return -1;
            }

            var currentId = minId;
            var harvestBatchTasks = new List<Task<int>>();

            _logger.LogDebug($"Start getting Artportalen checklists");

            var batchIndex = 0;
            // Loop until all sightings are fetched
            while (currentId <= maxId)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                if (_artportalenConfiguration.MaxNumberOfChecklistsHarvested.HasValue &&
                    currentId - minId >= _artportalenConfiguration.MaxNumberOfChecklistsHarvested)
                {
                    break;
                }

                await _semaphore.WaitAsync();
                batchIndex++;

                // Add batch task to list
                harvestBatchTasks.Add(HarvestBatchAsync(harvestFactory,
                    _checklistRepository.GetChunkAsync(currentId, _artportalenConfiguration.ChunkSizeChecklists),
                    batchIndex));

                // Calculate start of next chunk
                currentId += _artportalenConfiguration.ChunkSizeChecklists;
            }

            // Execute harvest tasks, no of parallel threads running is handled by semaphore
            await Task.WhenAll(harvestBatchTasks);

            // Sum each batch harvested
            var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

            _logger.LogDebug($"Finish getting Artportalen checklists ({ nrSightingsHarvested })");

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
            ArtportalenChecklistHarvestFactory harvestFactory,
            Task<IEnumerable<ChecklistEntity>> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                _logger.LogDebug(
                    $"Start getting Artportalen checklists ({batchIndex})");
                // Get chunk of sightings
                var chekLists = (await getChunkTask)?.ToArray();
                _logger.LogDebug(
                    $"Finish getting Artportalen checklists ({batchIndex})");

                if (!chekLists?.Any() ?? true)
                {
                    _logger.LogDebug(
                    $"No checklists found ({batchIndex})");
                    return 0;
                }

                _logger.LogDebug($"Start casting checklist entities to verbatim ({batchIndex})");

                // Cast sightings to verbatim observations
                var verbatimObservations = await harvestFactory.CastEntitiesToVerbatimsAsync(chekLists!);

                _logger.LogDebug($"Finish casting checklist entities to verbatim ({batchIndex})");

                _logger.LogDebug($"Start storing checklist batch ({batchIndex})");
                // Add sightings to mongodb

                await _artportalenChecklistVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug($"Finish storing checklist batch ({batchIndex})");

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
                    $"Harvest Artportalen checklists ({batchIndex})");
                throw new Exception("Harvest Artportalen checklists failed");
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
        /// <param name="checklistRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="artportalenChecklistVerbatimRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenChecklistHarvester(
            ArtportalenConfiguration artportalenConfiguration,
            IAreaHelper areaHelper,
            IProjectRepository projectRepository,
            IChecklistRepository checklistRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenChecklistVerbatimRepository,
            ILogger<ArtportalenChecklistHarvester> logger)
        {
            _artportalenConfiguration = artportalenConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenConfiguration));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _checklistRepository = checklistRepository ?? throw new ArgumentNullException(nameof(checklistRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _artportalenChecklistVerbatimRepository = artportalenChecklistVerbatimRepository ??
                                                      throw new ArgumentNullException(
                                                          nameof(artportalenChecklistVerbatimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads, artportalenConfiguration.NoOfThreads);
        }


        /// inheritdoc />
        public async Task<HarvestInfo> HarvestChecklistsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("Artportalen-Checklist", DateTime.Now);

            try
            {
                // Populate data on full harvest or if it's not initialized
                _logger.LogDebug("Start getting checklist metadata");
                var projectEntities = await _projectRepository.GetProjectsAsync();
                _logger.LogDebug("Finish getting checklist metadata");

                _logger.LogDebug("Start creating factory");
                using var harvestFactory = new ArtportalenChecklistHarvestFactory(
                    _areaHelper,
                    _checklistRepository,
                    _siteRepository,
                    _sightingRepository,
                    projectEntities,
                    _artportalenConfiguration.NoOfThreads,
                    _logger
                );

                _logger.LogDebug("Finish creating factory");

                _artportalenChecklistVerbatimRepository.Mode = JobRunModes.Full;

                // Make sure we have an empty public collection
                _logger.LogDebug("Start empty artportalen checklist verbatim collection");
                await _artportalenChecklistVerbatimRepository.DeleteCollectionAsync();
                await _artportalenChecklistVerbatimRepository.AddCollectionAsync();
                _logger.LogDebug("Finish empty artportalen checklist verbatim collection");

                var nrChecklistsHarvested = await HarvestAllAsync(harvestFactory, cancellationToken);
               
                // Update harvest info
                harvestInfo.Status = nrChecklistsHarvested >= 0 ? RunStatus.Success : RunStatus.Failed;

                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrChecklistsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Artportalen checklist harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest artportalen checklists");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestChecklistsAsync(DataProvider provider,
            IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => {
                throw new NotImplementedException("Not implemented for this provider");
            });
            return null!;
        }
    }
}