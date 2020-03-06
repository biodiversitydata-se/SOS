using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SpeciesPortalSightingFactory : Interfaces.ISpeciesPortalSightingFactory
    {
        private readonly SpeciesPortalConfiguration _speciesPortalConfiguration;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISightingVerbatimRepository _sightingVerbatimRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<SpeciesPortalSightingFactory> _logger;
        private readonly SemaphoreSlim _semaphore;
        /// <summary>
        /// Constructor
        /// </summary>///
        /// <param name="speciesPortalConfiguration"></param>
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
        public SpeciesPortalSightingFactory(
            SpeciesPortalConfiguration speciesPortalConfiguration,
            IMetadataRepository metadataRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            ISightingVerbatimRepository sightingVerbatimRepository,
            IPersonRepository personRepository,
            IOrganizationRepository organizationRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionItemRepository,
            ILogger<SpeciesPortalSightingFactory> logger)
        {
            _speciesPortalConfiguration = speciesPortalConfiguration ?? throw new ArgumentNullException(nameof(speciesPortalConfiguration));
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

            _semaphore = new SemaphoreSlim(_speciesPortalConfiguration.NoOfThreads);
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestSightingsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(APSightingVerbatim), DataProvider.ClamPortal, DateTime.Now);

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
                var siteEntities = await _siteRepository.GetAsync();
                _logger.LogDebug("Finish getting sites");

                _logger.LogDebug("Start casting site entities to verbatims");
                var siteVerbatims = siteEntities.ToVerbatims();
                var sites = siteVerbatims.ToDictionary(s => s.Id, s => s);
                _logger.LogDebug("Finish casting site entities to verbatims");

                // Make sure we have an empty collection
                _logger.LogDebug("Empty collection");
                await _sightingVerbatimRepository.DeleteCollectionAsync();
                await _sightingVerbatimRepository.AddCollectionAsync();

                var (minId, maxId) = await _sightingRepository.GetIdSpanAsync();
                var currentId = minId;
                var harvestBatchTasks = new List<Task<int>>();

                _logger.LogDebug("Start harvest species portal sightings");

                // Loop until all sightings are fetched
                while (currentId <= maxId)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_speciesPortalConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        currentId - minId >= _speciesPortalConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    await _semaphore.WaitAsync();

                    harvestBatchTasks.Add(Task.Run(async () => {
                        try
                        {
                            _logger.LogDebug($"Start getting species portal sightings from id: { currentId } to id: { currentId + _speciesPortalConfiguration.ChunkSize - 1 }");
                            // Get chunk of sightings
                            var sightings = (await _sightingRepository.GetChunkAsync(currentId, _speciesPortalConfiguration.ChunkSize)).ToArray();
                            _logger.LogDebug($"Finish getting species portal sightings from id: { currentId } to id: { currentId + _speciesPortalConfiguration.ChunkSize - 1 }");

                            /* if (_speciesPortalConfiguration.AddTestSightings && !hasAddedTestSightings)
                             {
                                 _logger.LogDebug("Start adding test sightings");
                                 AddTestSightings(_sightingRepository, ref sightings, _speciesPortalConfiguration.AddTestSightingIds);
                                 hasAddedTestSightings = true;
                                 _logger.LogDebug("Finish adding test sightings");
                             }*/

                            var sightingIds = new HashSet<int>(sightings.Select(x => x.Id));

                            _logger.LogDebug("Start calculating person sighting directory");
                            // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
                            var sightingRelations = (await _sightingRelationRepository.GetAsync(sightingIds)).ToVerbatims().ToArray();
                            var personSightingBySightingId = PersonSightingFactory.CalculatePersonSightingDictionary(
                                sightingIds,
                                personByUserId,
                                organizationById,
                                speciesCollections,
                                sightingRelations);
                            _logger.LogDebug("Finsih calculating person sighting directory");

                            _logger.LogDebug("Start getting projects and parameters");
                            // Get projects & project parameters
                            var projectEntityDictionaries = GetProjectEntityDictionaries(sightingIds, sightingProjectIds, projectEntityById, projectParameterEntities);
                            _logger.LogDebug("Finsish getting projects and parameters");

                            _logger.LogDebug("Start casting entities to verbatim");
                            // Cast sightings to aggregates
                            IEnumerable<APSightingVerbatim> aggregates = sightings.ToVerbatims(
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
                            await _sightingVerbatimRepository.AddManyAsync(aggregates);
                            _logger.LogDebug("Finish storing batch");

                            return sightings.Length;
                        }
                        finally
                        {
                            // Release semaphore in order to let next thread start getting data from source db 
                            _semaphore.Release();
                        }
                    }));

                    // Calculate start of next chunk
                    currentId += _speciesPortalConfiguration.ChunkSize;
                }

                await Task.WhenAll(harvestBatchTasks);
     
                var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

                _logger.LogDebug("Finish harvest species portal sightings");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Species Portal harvest was cancelled.");
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
                .ToDictionary(g => g.Key, g => g.Select(p => projectEntityById[p.ProjectId]));

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
