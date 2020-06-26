using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private bool _hasAddedTestSightings;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// ///
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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(artportalenConfiguration.NoOfThreads);
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestSightingsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(ArtportalenVerbatimObservation),
                DataProviderType.ArtportalenObservations, DateTime.Now);

            try
            {
                var activities = (await _metadataRepository.GetActivitiesAsync());

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

                _logger.LogDebug("Start getting meta data");
                await Task.WhenAll(metaDataTasks);
                cancellationToken?.ThrowIfCancellationRequested();
                var biotopes = metaDataTasks[0].Result;
                var genders = metaDataTasks[1].Result;
                var organizations = metaDataTasks[2].Result;
                var stages = metaDataTasks[3].Result;
                var substrates = metaDataTasks[4].Result;
                var units = metaDataTasks[5].Result;
                var validationStatus = metaDataTasks[6].Result;
                var discoveryMethods = metaDataTasks[7].Result;
                var determinationMethods = metaDataTasks[8].Result;
                _logger.LogDebug("Finish getting meta data");

                _logger.LogDebug("Start getting persons & organizations data");
                var personByUserId = await _personRepository.GetAsync();
                var organizationById = await _organizationRepository.GetAsync();
                _logger.LogDebug("Finish getting persons & organizations data");

                _logger.LogDebug("Start getting species collection data");
                var speciesCollections = await _speciesCollectionRepository.GetAsync();
                _logger.LogDebug("Finish getting species collection data");

                _logger.LogDebug("Start getting projects & project parameters");
                var projectEntityById = (await _projectRepository.GetProjectsAsync()).ToDictionary(p => p.Id, p => p);
                var projectParameterEntities = await _projectRepository.GetProjectParametersAsync();
                var sightingProjectIds = await _sightingRepository.GetProjectIdsAsync();
                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogDebug("Finish getting projects & project parameters");

                _logger.LogDebug("Start getting sites");
                var sites = (await _siteRepository.GetAsync()).ToList();
                _logger.LogDebug("Finish getting sites");

                // Make sure we have an empty collection
                _logger.LogDebug("Empty collection");
                await _sightingVerbatimRepository.DeleteCollectionAsync();
                await _sightingVerbatimRepository.AddCollectionAsync();

                // Get source min and max id
                var (minId, maxId) = await _sightingRepository.GetIdSpanAsync();
                var currentId = minId;
                var harvestBatchTasks = new List<Task<int>>();
                _hasAddedTestSightings = false;

                var harvestFactory = new ArtportalenHarvestFactory(
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
                        currentId, 
                        harvestFactory));

                    // Calculate start of next chunk
                    currentId += _artportalenConfiguration.ChunkSize;
                }

                // Execute harvest tasks, no of parallel threads running is handled by semaphore
                await Task.WhenAll(harvestBatchTasks);

                // Sum each batch harvested
                var nrSightingsHarvested = harvestBatchTasks.Sum(t => t.Result);

                _logger.LogDebug("Finish getting Artportalen sightings");

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

        private async Task<int> HarvestBatchAsync(
            int currentId,
            ArtportalenHarvestFactory harvestFactory
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

                _logger.LogDebug("Start casting entities to verbatim");

                // Cast sightings to verbatim observations
                var verbatimObservations = await harvestFactory.CastEntitiesToVerbatimsAsync(sightings);

                _logger.LogDebug("Finsih casting entities to verbatim");

                _logger.LogDebug("Start storing batch");
                // Add sightings to mongodb
                await _sightingVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug("Finish storing batch");

                return sightings.Length;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Harvest Artportalen sightings from id: {currentId} to id: {currentId + _artportalenConfiguration.ChunkSize - 1} failed.");
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }

            throw new Exception("Harvest Artportalen batch failed");
        }

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
    }
}