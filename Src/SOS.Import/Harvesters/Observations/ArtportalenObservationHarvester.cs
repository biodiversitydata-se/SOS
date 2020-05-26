using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Extensions;
using SOS.Import.Factories;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    /// Artportalen observation harvester
    /// </summary>
    public class ArtportalenObservationHarvester : Interfaces.IArtportalenObservationHarvester
    {
        private readonly ArtportalenConfiguration _artportalenConfiguration;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISightingVerbatimRepository _sightingVerbatimRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<ArtportalenObservationHarvester> _logger;
        private readonly SemaphoreSlim _semaphore;
        private bool _hasAddedTestSightings;

        /// <summary>
        /// Constructor
        /// </summary>///
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
            ILogger<ArtportalenObservationHarvester> logger)
        {
            _artportalenConfiguration = artportalenConfiguration ?? throw new ArgumentNullException(nameof(artportalenConfiguration));
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _sightingVerbatimRepository = sightingVerbatimRepository ?? throw new ArgumentNullException(nameof(sightingVerbatimRepository));
            _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
            _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            _sightingRelationRepository = sightingRelationRepository ?? throw new ArgumentNullException(nameof(sightingRelationRepository));
            _speciesCollectionRepository = speciesCollectionItemRepository ?? throw new ArgumentNullException(nameof(speciesCollectionItemRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads);
        }

        private async Task<int> HarvestBatchAsync(
            int currentId,
            IDictionary<int, MetadataWithCategory> activities,
            IDictionary<int, Metadata> biotopes,
            IDictionary<int, Metadata> genders,
            IDictionary<int, Metadata> organizations,
            IDictionary<int, Site> sites,
            IDictionary<int, Metadata> stages,
            IDictionary<int, Metadata> substrates,
            IDictionary<int, Metadata> validationStatus,
            IDictionary<int, Metadata> units,
            IDictionary<int, Person> personByUserId,
            IDictionary<int, Organization> organizationById,
            IList<SpeciesCollectionItem> speciesCollections,
            IEnumerable<(int SightingId, int ProjectId)> sightingProjectIds,
            IDictionary<int, ProjectEntity> projectEntityById,
            IEnumerable<ProjectParameterEntity> projectParameterEntities
        )
        {
            try
            {
                _logger.LogDebug(
                    $"Start getting Artportalen sightings from id: {currentId} to id: {currentId + _artportalenConfiguration.ChunkSize - 1}");
                // Get chunk of sightings
                var sightings =
                    (await _sightingRepository.GetChunkAsync(currentId, _artportalenConfiguration.ChunkSize))
                    .ToArray();
                _logger.LogDebug(
                    $"Finish getting Artportalen sightings from id: {currentId} to id: {currentId + _artportalenConfiguration.ChunkSize - 1}");

                 if (_artportalenConfiguration.AddTestSightings && !_hasAddedTestSightings)
                 {
                     _logger.LogDebug("Start adding test sightings");
                     AddTestSightings(_sightingRepository, ref sightings, _artportalenConfiguration.AddTestSightingIds);
                     _hasAddedTestSightings = true;
                     _logger.LogDebug("Finish adding test sightings");
                 }

                var sightingIds = new HashSet<int>(sightings.Select(x => x.Id));

                _logger.LogDebug("Start calculating person sighting directory");
                // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
                var sightingRelations =
                    (await _sightingRelationRepository.GetAsync(sightingIds)).ToVerbatims().ToArray();
                var personSightingBySightingId = PersonSightingFactory.CreatePersonSightingDictionary(
                    sightingIds,
                    personByUserId,
                    organizationById,
                    speciesCollections,
                    sightingRelations);
                _logger.LogDebug("Finish calculating person sighting directory");

                _logger.LogDebug("Start getting projects and parameters");
                // Get projects & project parameters
                var projectEntityDictionaries = GetProjectEntityDictionaries(sightingIds, sightingProjectIds,
                    projectEntityById, projectParameterEntities);
                _logger.LogDebug("Finish getting projects and parameters");

                _logger.LogDebug("Start casting entities to verbatim");
                // Cast sightings to verbatim observations
                IEnumerable<ArtportalenVerbatimObservation> verbatimObservations = sightings.ToVerbatims(
                    activities,
                    biotopes,
                    genders,
                    organizations,
                    personSightingBySightingId,
                    sites,
                    stages,
                    substrates,
                    validationStatus,
                    units,
                    projectEntityDictionaries);
                _logger.LogDebug("Finsih casting entities to verbatim");

                _logger.LogDebug("Start storing batch");
                // Add sightings to mongodb
                await _sightingVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug("Finish storing batch");

                return sightings.Length;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Harvest Artportalen sightings from id: {currentId} to id: {currentId + _artportalenConfiguration.ChunkSize - 1} failed.");
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }

            throw new Exception("Harvest Artportalen batch failed");
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestSightingsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(ArtportalenVerbatimObservation), DataProviderType.ArtportalenObservations, DateTime.Now);

            try
            {
                var activities = (await _metadataRepository.GetActivitiesAsync()).ToVerbatims().ToDictionary(a => a.Id, a => a);

                var metaDataTasks = new[]
                {
                    _metadataRepository.GetBiotopesAsync(),
                    _metadataRepository.GetGendersAsync(),
                    _metadataRepository.GetOrganizationsAsync(),
                    _metadataRepository.GetStagesAsync(),
                    _metadataRepository.GetSubstratesAsync(),
                    _metadataRepository.GetUnitsAsync(),
                    _metadataRepository.GetValidationStatusAsync()
                };

                _logger.LogDebug("Start getting meta data");
                await Task.WhenAll(metaDataTasks);
                cancellationToken?.ThrowIfCancellationRequested();
                var biotopes = metaDataTasks[0].Result.ToVerbatims().ToDictionary(b => b.Id, b => b);
                var genders = metaDataTasks[1].Result.ToVerbatims().ToDictionary(g => g.Id, g => g);
                var organizations = metaDataTasks[2].Result.ToVerbatims().ToDictionary(o => o.Id, o => o);
                var stages = metaDataTasks[3].Result.ToVerbatims().ToDictionary(s => s.Id, s => s);
                var substrates = metaDataTasks[4].Result.ToVerbatims().ToDictionary(s => s.Id, s => s);
                var units = metaDataTasks[5].Result.ToVerbatims().ToDictionary(u => u.Id, u => u);
                var validationStatus = metaDataTasks[6].Result.ToVerbatims().ToDictionary(v => v.Id, v => v);
                _logger.LogDebug("Finish getting meta data");

                _logger.LogDebug("Start getting persons & organizations data");
                var personByUserId = (await _personRepository.GetAsync()).ToVerbatims().ToDictionary(p => p.UserId, p => p);
                var organizationById = (await _organizationRepository.GetAsync()).ToVerbatims().ToDictionary(o => o.Id, o => o);
                _logger.LogDebug("Finish getting persons & organizations data");

                _logger.LogDebug("Start getting species collection data");
                var speciesCollections = (await _speciesCollectionRepository.GetAsync()).ToVerbatims().ToList();
                _logger.LogDebug("Finish getting species collection data");

                _logger.LogDebug("Start getting projects & project parameters");
                var projectEntityById = (await _projectRepository.GetProjectsAsync()).ToDictionary(p => p.Id, p => p);
                var projectParameterEntities = await _projectRepository.GetProjectParametersAsync();
                var sightingProjectIds = await _sightingRepository.GetProjectIdsAsync();
                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogDebug("Finish getting projects & project parameters");

                _logger.LogDebug("Start getting sites");
                var siteEntities = (await _siteRepository.GetAsync()).ToList();
                _logger.LogDebug("Finish getting sites");

                _logger.LogDebug("Start casting site entities to verbatims");
                var siteVerbatims = siteEntities.ToVerbatimsUsingBatch();
                var sites = siteVerbatims.ToDictionary(s => s.Id, s => s);
                _logger.LogDebug("Finish casting site entities to verbatims");

                // Make sure we have an empty collection
                _logger.LogDebug("Empty collection");
                await _sightingVerbatimRepository.DeleteCollectionAsync();
                await _sightingVerbatimRepository.AddCollectionAsync();

                // Get source min and max id
                var (minId, maxId) = await _sightingRepository.GetIdSpanAsync();
                var currentId = minId;
                var harvestBatchTasks = new List<Task<int>>();
                _hasAddedTestSightings = false;

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
                    harvestBatchTasks.Add(HarvestBatchAsync(currentId, activities, biotopes, genders, organizations, sites, stages, substrates,
                        validationStatus, units,
                        personByUserId, organizationById, speciesCollections, sightingProjectIds, projectEntityById,
                        projectParameterEntities));

                    // Calculate start of next chunk
                    currentId += _artportalenConfiguration.ChunkSize;
                }

                // Execute harvest tasks, no of parallel threads running is handled by semaphore
                await Task.WhenAll(harvestBatchTasks);

                // Sum each batch harvested
                var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

                _logger.LogDebug("Finish getting Artportalen sightings");

                // Update harvest info
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

        /// <summary>
        /// Add test sightings for testing purpose.
        /// </summary>
        private void AddTestSightings(
            ISightingRepository sightingRepository,
            ref SightingEntity[] sightings,
            IEnumerable<int> sightingIds)
        {
            var extraSightings = sightingRepository.GetChunkAsync(sightingIds).Result;
            sightings = extraSightings.Union(sightings.Where(s => extraSightings.All(e => e.Id != s.Id))).ToArray();
        }

        private static ProjectEntityDictionaries GetProjectEntityDictionaries(
            HashSet<int> sightingIds,
            IEnumerable<(int SightingId, int ProjectId)> sightingProjectIds,
            IDictionary<int, ProjectEntity> projectEntityById,
            IEnumerable<ProjectParameterEntity> projectParameterEntities)
        {
            Dictionary<int, IEnumerable<ProjectEntity>> projectEntitiesBySightingId = sightingProjectIds
                .Where(p => sightingIds.Contains(p.SightingId))
                .GroupBy(p => p.SightingId)
                .ToDictionary(g => g.Key, g => g
                    .Where(p => projectEntityById.ContainsKey(p.ProjectId))
                    .Select(p => projectEntityById[p.ProjectId]));

            Dictionary<int, IEnumerable<ProjectParameterEntity>> projectParameterEntitiesBySightingId = projectParameterEntities
                .Where(p => sightingIds.Contains(p.SightingId))
                .GroupBy(p => p.SightingId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return new ProjectEntityDictionaries
            {
                ProjectEntityById = projectEntityById,
                ProjectEntitiesBySightingId = projectEntitiesBySightingId,
                ProjectParameterEntitiesBySightingId = projectParameterEntitiesBySightingId
            };
        }
    }
}
