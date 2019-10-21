using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SpeciesPortalSightingFactory : Interfaces.ISpeciesPortalSightingFactory
    {
        private readonly IAreaRepository _areaRepository;

        private readonly IMetadataRepository _metadataRepository;

        private readonly IProjectRepository _projectRepository;

        private readonly ISightingRepository _sightingRepository;

        private readonly ISiteRepository _siteRepository;

        private readonly IAreaVerbatimRepository _areaVerbatimRepository;

        private readonly ISightingVerbatimRepository _sightingVerbatimRepository;

        private readonly ILogger<SpeciesPortalSightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="metadataRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="sightingVerbatimRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalSightingFactory(
            IAreaRepository areaRepository,
            IMetadataRepository metadataRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            IAreaVerbatimRepository areaVerbatimRepository,
            ISightingVerbatimRepository sightingVerbatimRepository,
            ILogger<SpeciesPortalSightingFactory> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _areaVerbatimRepository = areaVerbatimRepository ??
                                           throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _sightingVerbatimRepository = sightingVerbatimRepository ??
                                            throw new ArgumentNullException(nameof(sightingVerbatimRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> AggregateAsync()
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
                const int chunkSize = 3000000;

                _logger.LogDebug("Start getting sightings");

                // Loop until all sightings are fetched
                while (minId <= maxId)
                {
                    _logger.LogDebug($"Getting sightings from { minId } to { minId + chunkSize - 1 }");
                    // Get chunk of sightings
                    var sightings = await _sightingRepository.GetChunkAsync(minId, chunkSize);

                    // Cast sightings to aggregates
                    var aggregates = sightings.ToAggregates(activities, genders, stages, units, sites, sightingProjects);

                    // Add sightings to mongodb
                    await _sightingVerbatimRepository.AddManyAsync(aggregates);

                    // Calculate start of next chunk
                    minId += chunkSize;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed aggregation of sightings");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> AggregateAreasAsync()
        {
            try
            {
                _logger.LogDebug("Start getting areas");

                var areas = (await _areaRepository.GetAsync()).ToAggregates();

                _logger.LogDebug("Empty collection");
                // Make sure we have an empty collection
                if (await _areaVerbatimRepository.DeleteCollectionAsync())
                {
                    if (await _areaVerbatimRepository.AddCollectionAsync())
                    {
                        if (await _areaVerbatimRepository.AddManyAsync(areas))
                        {
                            await _areaVerbatimRepository.CreateIndexAsync();
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed aggregation of areas");
                return false;
            }
        }
    }
}
