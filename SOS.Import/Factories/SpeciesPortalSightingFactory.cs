using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Extensions;
using SOS.Import.Models;
using SOS.Import.Models.Aggregates;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SpeciesPortalSightingFactory : Interfaces.ISpeciesPortalSightingFactory
    {
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

        /// <summary>
        /// Constructor
        /// </summary>
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
        }

        public async Task<bool> AggregateAsync()
        {
            return await AggregateAsync(new SpeciesPortalAggregationOptions());
        }

        public async Task<bool> AggregateAsync(SpeciesPortalAggregationOptions options)
        {
            try
            {
                var metaDataTasks = new[]
                {
                    _metadataRepository.GetActivitiesAsync(),
                    _metadataRepository.GetGendersAsync(),
                    _metadataRepository.GetStagesAsync(),
                    _metadataRepository.GetUnitsAsync()
                };

                _logger.LogDebug("Start getting meta data");
                await Task.WhenAll(metaDataTasks);

                var activities = metaDataTasks[0].Result.ToAggregates().ToDictionary(a => a.Id, a => a);
                var genders = metaDataTasks[1].Result.ToAggregates().ToDictionary(g => g.Id, g => g);
                var stages = metaDataTasks[2].Result.ToAggregates().ToDictionary(s => s.Id, s => s);
                var units = metaDataTasks[3].Result.ToAggregates().ToDictionary(u => u.Id, u => u);

                _logger.LogDebug("Start getting persons & organizations data");
                var personByUserId = (await _personRepository.GetAsync()).ToAggregates().ToDictionary(p => p.UserId, p => p);
                var organizationById = (await _organizationRepository.GetAsync()).ToDictionary(o => o.Id, o => o);

                _logger.LogDebug("Start getting species collection data");
                var speciesCollections = (await _speciesCollectionRepository.GetAsync()).ToAggregates().ToList();

                _logger.LogDebug("Start getting projects");
                var projects = (await _projectRepository.GetAsync()).ToAggregates().ToDictionary(p => p.Id, p => p);

                _logger.LogDebug("Start getting sighting project id's");
                var sightingProjectIds = await _sightingRepository.GetProjectIdsAsync();

                // Create a dictionary with sightings projects. 
                var sightingProjects = new Dictionary<int, List<Project>>();
                foreach (var (sightingId, projectId) in sightingProjectIds)
                {
                    // If no entry exists for sighting, add it
                    if (!sightingProjects.ContainsKey(sightingId))
                    {
                        sightingProjects.Add(sightingId, new List<Project>());
                    }

                    // Add project to sighting
                    sightingProjects[sightingId].Add(projects[projectId]);
                }

                _logger.LogDebug("Start getting sites");
                var sites = (await _siteRepository.GetAsync()).ToAggregates().ToDictionary(s => s.Id, s => s);

                _logger.LogDebug("Empty collection");
                // Make sure we have an empty collection
                await _sightingVerbatimRepository.DeleteCollectionAsync();
                await _sightingVerbatimRepository.AddCollectionAsync();

                var (minId, maxId) = await _sightingRepository.GetIdSpanAsync();
                _logger.LogDebug("Start getting sightings");
                int nrSightingsHarvested = 0;

                // Loop until all sightings are fetched
                while (minId <= maxId)
                {
                    if (options.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= options.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    _logger.LogDebug($"Getting sightings from { minId } to { minId + options.ChunkSize -1 }");
                    nrSightingsHarvested += options.ChunkSize;

                    // Get chunk of sightings
                    IEnumerable<SightingEntity> sightings = (await _sightingRepository.GetChunkAsync(minId, options.ChunkSize)).ToArray();
                    HashSet<int> sightingIds = new HashSet<int>(sightings.Select(x => x.Id));
                    
                    // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
                    var sightingRelations = (await _sightingRelationRepository.GetAsync(sightings.Select(x => x.Id))).ToList();
                    var personSightingBySightingId = PersonSightingFactory.CalculatePersonSightingDictionary(
                        sightingIds,
                        personByUserId,
                        organizationById,
                        speciesCollections,
                        sightingRelations);

                    // Cast sightings to aggregates
                    IEnumerable <APSightingVerbatim> aggregates = sightings.ToAggregates(
                        activities, 
                        genders, 
                        stages, 
                        units, 
                        sites, 
                        sightingProjects,
                        personSightingBySightingId);
                    
                    // Add sightings to mongodb
                    if (options.AddSightingsToVerbatimDatabase)
                    {
                        await _sightingVerbatimRepository.AddManyAsync(aggregates);
                    }

                    // Calculate start of next chunk
                    minId += options.ChunkSize;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed aggregation of sightings");
                return false;
            }
        }
    }
}
