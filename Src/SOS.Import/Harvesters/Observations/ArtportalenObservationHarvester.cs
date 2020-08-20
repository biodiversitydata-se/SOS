using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     Artportalen observation harvester
    /// </summary>
    public class ArtportalenObservationHarvester : IArtportalenObservationHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly ILogger<ArtportalenObservationHarvester> _logger;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly SemaphoreSlim _semaphore;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISightingVerbatimRepository _sightingVerbatimRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private bool _hasAddedTestSightings;
        private ArtportalenHarvestFactory _harvestFactory;

        /// <summary>
        ///     Add test sightings for testing purpose.
        /// </summary>
        private void AddTestSightings(
            ISightingRepository sightingRepository,
            ref SightingEntity[] sightings,
            IEnumerable<int> sightingIds)
        {
            var extraSightings = sightingRepository.GetChunkAsync(sightingIds).Result;
            sightings = extraSightings.Union(sightings.Where(s => extraSightings.All(e => e.Id != s.Id))).ToArray();
        }

        /// <summary>
        /// Harvest a batch of sightings
        /// </summary>
        /// <param name="currentId"></param>
        /// <param name="incrementalHarvest"></param>
        /// <returns></returns>
        private async Task<int> HarvestBatchAsync(
            int currentId,
            bool incrementalHarvest
        )
        {
            var lastId = currentId + _artportalenConfiguration.ChunkSize - 1;
            try
            {
                _logger.LogDebug(
                    $"Start getting Artportalen sightings from id: {currentId} to id: {lastId}");
                // Get chunk of sightings
                var sightings =
                    (await _sightingRepository.GetChunkAsync(currentId, _artportalenConfiguration.ChunkSize, incrementalHarvest))
                    ?.ToArray();
                _logger.LogDebug(
                    $"Finish getting Artportalen sightings from id: {currentId} to id: {lastId}");

                if (!sightings?.Any() ?? true)
                {
                    _logger.LogDebug(
                    $"No sightings found from id: {currentId} to id: {lastId}");
                    return 0;
                }

                if (_artportalenConfiguration.AddTestSightings && !_hasAddedTestSightings)
                {
                    _logger.LogDebug("Start adding test sightings");
                    AddTestSightings(_sightingRepository, ref sightings, _artportalenConfiguration.AddTestSightingIds);
                    _hasAddedTestSightings = true;
                    _logger.LogDebug("Finish adding test sightings");
                }

                _logger.LogDebug($"Start casting entities to verbatim from id: {currentId} to id: {lastId}");

                // Cast sightings to verbatim observations
                var verbatimObservations = await _harvestFactory.CastEntitiesToVerbatimsAsync(sightings);
                
                // We don't need entities in memory any more
                sightings = null;
                
                _logger.LogDebug($"Finsih casting entities to verbatim from id: {currentId} to id: {lastId}");
                
                _logger.LogDebug($"Start storing batch from id: {currentId} to id: {lastId}");
                // Add sightings to mongodb
                await _sightingVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug($"Finish storing batch from id: {currentId} to id: {lastId}");

                return verbatimObservations?.Count() ?? 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Harvest Artportalen sightings from id: {currentId} to id: {lastId} failed.");
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }

            throw new Exception("Harvest Artportalen batch failed");
        }

        /// <summary>
        /// Initialize activities
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<MetadataWithCategoryEntity>> GetActivitiesAsync()
        {
            return await _metadataRepository.GetActivitiesAsync();
        }

        /// <summary>
        /// Initialize meta data
        /// </summary>
        /// <returns></returns>
        private async Task<(IEnumerable<MetadataEntity>, 
            IEnumerable<MetadataEntity>, 
            IEnumerable<MetadataEntity>, 
            IEnumerable<MetadataEntity>, 
            IEnumerable<MetadataEntity>,
            IEnumerable<MetadataEntity>,
            IEnumerable<MetadataEntity>,
            IEnumerable<MetadataEntity>,
            IEnumerable<MetadataEntity>)> GetMetadataAsync()
        {
            _logger.LogDebug("Start getting meta data");

            var metaDataTasks = new[]
            {
                _metadataRepository.GetBiotopesAsync(),
                _metadataRepository.GetGendersAsync(),
                _metadataRepository.GetOrganizationsAsync(),
                _metadataRepository.GetStagesAsync(),
                _metadataRepository.GetSubstratesAsync(),
                _metadataRepository.GetUnitsAsync(),
                _metadataRepository.GetValidationStatusAsync(),
                _metadataRepository.GetDiscoveryMethodsAsync(),
                _metadataRepository.GetDeterminationMethodsAsync()
            };

            await Task.WhenAll(metaDataTasks);
            _logger.LogDebug("Finish getting meta data");

            return (metaDataTasks[0].Result, 
                metaDataTasks[1].Result, 
                metaDataTasks[2].Result, 
                metaDataTasks[3].Result, 
                metaDataTasks[4].Result, 
                metaDataTasks[5].Result, 
                metaDataTasks[6].Result, 
                metaDataTasks[7].Result, 
                metaDataTasks[8].Result);
        }

        /// <summary>
        /// persons and organizations
        /// </summary>
        /// <returns></returns>
        private async Task<(IEnumerable<PersonEntity>, IEnumerable<OrganizationEntity>)> GetPersonsAndOrganizationsAsync()
        {
            _logger.LogDebug("Start getting persons & organizations data");
            var personByUserId = await _personRepository.GetAsync();
            var organizationById = await _organizationRepository.GetAsync();
            _logger.LogDebug("Finish getting persons & organizations data");

            return (personByUserId, organizationById);
        }

        /// <summary>
        /// Initialize project related data
        /// </summary>
        /// <returns></returns>
        private async Task<(IEnumerable<ProjectEntity>, IEnumerable<ProjectParameterEntity>, IEnumerable<(int, int)>)> GetProjectRelatedAsync()
        {
            _logger.LogDebug("Start getting projects & project parameters");
            var projectEntityById = (await _projectRepository.GetProjectsAsync());
            var projectParameterEntities = await _projectRepository.GetProjectParametersAsync();
            var sightingProjectIds = await _sightingRepository.GetProjectIdsAsync();
            _logger.LogDebug("Finish getting projects & project parameters");

            return (projectEntityById, projectParameterEntities, sightingProjectIds);
        }

        /// <summary>
        /// Initialize species collections
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<SpeciesCollectionItemEntity>> GetSpeciesCollections()
        {
            _logger.LogDebug("Start getting species collection data");
            var speciesCollections = await _speciesCollectionRepository.GetAsync();
            _logger.LogDebug("Finish getting species collection data");

            return speciesCollections;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenConfiguration"></param>
        /// <param name="metadataRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="sightingVerbatimRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="organizationRepository"></param>
        /// <param name="sightingRelationRepository"></param>
        /// <param name="speciesCollectionItemRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public ArtportalenObservationHarvester(
            ArtportalenConfiguration artportalenConfiguration,
            IMetadataRepository metadataRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            ISightingVerbatimRepository sightingVerbatimRepository,
            IPersonRepository personRepository,
            IOrganizationRepository organizationRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionItemRepository,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<ArtportalenObservationHarvester> logger)
        {
            _artportalenConfiguration = artportalenConfiguration ??
                                        throw new ArgumentNullException(nameof(artportalenConfiguration));
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _sightingVerbatimRepository = sightingVerbatimRepository ??
                                          throw new ArgumentNullException(nameof(sightingVerbatimRepository));
            _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
            _organizationRepository =
                organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            _sightingRelationRepository = sightingRelationRepository ??
                                          throw new ArgumentNullException(nameof(sightingRelationRepository));
            _speciesCollectionRepository = speciesCollectionItemRepository ??
                                           throw new ArgumentNullException(nameof(speciesCollectionItemRepository));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestSightingsAsync(bool incrementalHarvest, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(ArtportalenObservationVerbatim),
                DataProviderType.ArtportalenObservations, DateTime.Now);

            try
            {
                if (_harvestFactory == null)
                {
                    var activities = await GetActivitiesAsync();
                    var (biotopes,
                        genders,
                        organizations,
                        stages,
                        substrates,
                        units,
                        validationStatus,
                        discoveryMethods,
                        determinationMethods) = await GetMetadataAsync();

                    var (personByUserId, organizationById) = await GetPersonsAndOrganizationsAsync();
                    var (projectEntityById, projectParameterEntities, sightingProjectIds) =
                        await GetProjectRelatedAsync();
                    var speciesCollections = await GetSpeciesCollections();
                    
                    cancellationToken?.ThrowIfCancellationRequested();

                    // If we harvest all sightings from backup, get all sites at once to increase performance
                    _logger.LogDebug("Start getting sites");
                    var sites = (List<SiteEntity>)null;// (await _siteRepository.GetAsync()).ToList();
                    _logger.LogDebug("Finish getting sites");

                    _logger.LogDebug("Start creating factory");
                    _harvestFactory = new ArtportalenHarvestFactory(
                        _siteRepository,
                        _sightingRelationRepository,
                        activities,
                        biotopes,
                        determinationMethods,
                        discoveryMethods,
                        genders,
                        organizations,
                        organizationById,
                        personByUserId,
                        projectEntityById,
                        projectParameterEntities,
                        sightingProjectIds,
                        sites,
                        speciesCollections,
                        stages,
                        substrates,
                        validationStatus,
                        units
                    );

                    // Clean up to release memory
                    activities = null;
                    biotopes = null;
                    determinationMethods = null;
                    discoveryMethods = null;
                    genders = null;
                    organizations = null;
                    organizationById = null;
                    personByUserId = null;
                    projectEntityById = null;
                    projectParameterEntities = null;
                    sightingProjectIds = null;
                    sites = null;
                    speciesCollections = null;
                    stages = null;
                    substrates = null;
                    validationStatus = null;
                    units = null;

                    _logger.LogDebug("Finsih creating factory");
                }

                // Get source min and max id
                int minId, maxId;

                if (incrementalHarvest)
                {
                    // Make sure incremental mode is false to get max id from last full harvest
                    _processedObservationRepository.IncrementalMode = false;

                    // We start from last harvested sighting and end at latest added sighting (live data)
                    minId = await _processedObservationRepository.GetMaxIdForProviderAsync(1) + 1;
                    maxId = await _sightingRepository.GetMaxIdLiveAsync();
            
                    // Check if number of sightings to harvest exceeds live harvest limit
                    if (maxId - minId > _artportalenConfiguration.CatchUpLimit)
                    {
                        _logger.LogInformation("Canceling Artportalen harvest. To many sightings for live harvest.");

                        throw new JobAbortedException();
                    }
                }
                else
                {
                    (minId, maxId) = await _sightingRepository.GetIdSpanAsync();

                    // MaxNumberOfSightingsHarvested is a debug feature. If it's set calculate minid to get last sightings.
                    // This make it easier to test incremental harvest since it has a max limit from last created 
                    if (_artportalenConfiguration.MaxNumberOfSightingsHarvested.HasValue && _artportalenConfiguration.MaxNumberOfSightingsHarvested > 0)
                    {
                        minId = maxId - _artportalenConfiguration.MaxNumberOfSightingsHarvested.Value;
                    }
                }

                // Set observation repository in incremental mode in order to store data in other collection
                _sightingVerbatimRepository.IncrementalMode = incrementalHarvest;
                _harvestFactory.IncrementalMode = incrementalHarvest;

                var nrSightingsHarvested = 0;

                if (maxId > minId)
                {
                    var currentId = minId;
                    var harvestBatchTasks = new List<Task<int>>();
                    // Don't add test sightings when we harvest live data
                    _hasAddedTestSightings = incrementalHarvest;

                    // Make sure we have an empty collection
                    _logger.LogDebug("Empty collection");
                    await _sightingVerbatimRepository.DeleteCollectionAsync();
                    await _sightingVerbatimRepository.AddCollectionAsync();

                    _logger.LogDebug("Start getting Artportalen sightings");
                    // Loop until all sightings are fetched
                    while (currentId <= maxId)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
                        if (_artportalenConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                            currentId - minId >= _artportalenConfiguration.MaxNumberOfSightingsHarvested)
                        {
                            break;
                        }

                        await _semaphore.WaitAsync();

                        // Add batch task to list
                        harvestBatchTasks.Add(HarvestBatchAsync(
                            currentId, incrementalHarvest));

                        // Calculate start of next chunk
                        currentId += _artportalenConfiguration.ChunkSize;
                    }

                    // Execute harvest tasks, no of parallel threads running is handled by semaphore
                    await Task.WhenAll(harvestBatchTasks);

                    // Sum each batch harvested
                    nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

                    _logger.LogDebug($"Finish getting Artportalen sightings ({ nrSightingsHarvested })");
                }
                
                // Update harvest info
                harvestInfo.DataLastModified = await _sightingRepository.GetLastModifiedDateAsyc();
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
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

            return harvestInfo;
        }
    }
}