using Microsoft.Extensions.Logging;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Factories;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Harvesters.Artportalen
{
    internal class ArtportalenHarvestFactory : ArtportalenHarvestFactoryBase, IHarvestFactory<SightingEntity[], ArtportalenObservationVerbatim>
    {
        private readonly IArtportalenMetadataContainer _artportalenMetadataContainer;
        private readonly IMediaRepository _mediaRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRepository _sightingRepository;        
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;        
        private readonly ILogger<ArtportalenObservationHarvester> _logger;

        /// <summary>
        /// Cast sighting itemEntity to model
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="speciesCollectionItems"></param>
        /// <param name="personSighting"></param>
        /// <param name="projects"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        private ArtportalenObservationVerbatim? CastEntityToVerbatim(SightingEntity? entity,
            IEnumerable<SpeciesCollectionItemEntity>? speciesCollectionItems,
            PersonSighting? personSighting,
            IEnumerable<Project>? projects,
            IEnumerable<Media>? media)
        {
            var sightingId = -1;

            try
            {
                if (entity == null)
                {
                    return null;
                }

                Sites.TryGetValue(entity.SiteId ?? 0, out var site);

                sightingId = entity.Id;
               
                _artportalenMetadataContainer.TaxonSpeciesGroups.TryGetValue(entity.TaxonId ?? 0,
                    out var speciesGroupId);

                var observation = new ArtportalenObservationVerbatim();
                observation.Activity = entity.ActivityId.HasValue && _artportalenMetadataContainer.Activities.ContainsKey(entity.ActivityId.Value)
                    ? _artportalenMetadataContainer.Activities[entity.ActivityId.Value]
                    : null;
                observation.Biotope = entity.BiotopeId.HasValue && _artportalenMetadataContainer.Biotopes.ContainsKey(entity.BiotopeId.Value)
                    ? _artportalenMetadataContainer.Biotopes[entity.BiotopeId.Value]
                    : null;
                observation.BiotopeDescription = entity.BiotopeDescription;
                observation.Comment = entity.Comment;
                observation.DatasourceId = entity.DatasourceId;
                observation.DiscoveryMethod = entity.DiscoveryMethodId.HasValue && _artportalenMetadataContainer.DiscoveryMethods.ContainsKey(entity.DiscoveryMethodId.Value)
                    ? _artportalenMetadataContainer.DiscoveryMethods[entity.DiscoveryMethodId.Value]
                    : null;
                observation.DeterminationMethod = entity.DeterminationMethodId.HasValue && _artportalenMetadataContainer.DeterminationMethods.ContainsKey(entity.DeterminationMethodId.Value)
                    ? _artportalenMetadataContainer.DeterminationMethods[entity.DeterminationMethodId.Value]
                    : null;
                observation.EditDate = entity.EditDate;
                observation.EndDate = entity.EndDate;
                observation.EndTime = entity.EndTime;
                observation.FrequencyId = entity.FrequencyId;
                observation.Gender = entity.GenderId.HasValue && _artportalenMetadataContainer.Genders.ContainsKey(entity.GenderId.Value)
                    ? _artportalenMetadataContainer.Genders[entity.GenderId.Value]
                    : null;
                observation.HasTriggeredValidationRules = entity.HasTriggeredValidationRules;
                observation.HasAnyTriggeredValidationRuleWithWarning = entity.HasAnyTriggeredValidationRuleWithWarning;
                observation.HiddenByProvider = entity.HiddenByProvider;
                observation.Id = NextId;
                observation.SightingId = entity.Id;
                observation.OwnerOrganization = entity.OwnerOrganizationId.HasValue &&
                                                _artportalenMetadataContainer.Organizations.ContainsKey(entity.OwnerOrganizationId.Value)
                    ? _artportalenMetadataContainer.Organizations[entity.OwnerOrganizationId.Value]
                    : null;
                observation.Length = entity.Length;
                observation.MaxDepth = entity.MaxDepth;
                observation.MaxHeight = entity.MaxHeight;
                observation.MigrateSightingObsId = entity.MigrateSightingObsId;
                observation.MigrateSightingPortalId = entity.MigrateSightingPortalId;
                observation.MinDepth = entity.MinDepth;
                observation.MinHeight = entity.MinHeight;
                observation.NoteOfInterest = entity.NoteOfInterest;
                observation.HasUserComments = entity.HasUserComments;
                observation.NotPresent = entity.NotPresent;
                observation.NotRecovered = entity.NotRecovered;
                observation.ProtectedBySystem = entity.ProtectedBySystem;
                observation.Quantity = entity.Quantity;
                observation.QuantityOfSubstrate = entity.QuantityOfSubstrate;
                observation.ReportedDate = entity.RegisterDate;
                observation.ReproductionId = entity.ReproductionId;
                observation.RightsHolder = entity.RightsHolder;
                observation.Site = site;
                observation.SpeciesGroupId = speciesGroupId;
                observation.Stage = entity.StageId.HasValue && _artportalenMetadataContainer.Stages.ContainsKey(entity.StageId.Value)
                    ? _artportalenMetadataContainer.Stages[entity.StageId.Value]
                    : null;
                observation.StartDate = entity.StartDate;
                observation.StartTime = entity.StartTime;
                observation.Substrate = entity.SubstrateId.HasValue && _artportalenMetadataContainer.Substrates.ContainsKey(entity.SubstrateId.Value)
                    ? _artportalenMetadataContainer.Substrates[entity.SubstrateId.Value]
                    : null;
                observation.SubstrateDescription = entity.SubstrateDescription;
                observation.SubstrateSpeciesDescription = entity.SubstrateSpeciesDescription;
                observation.SubstrateSpeciesId = entity.SubstrateSpeciesId;
                observation.TaxonId = entity.TaxonId;
                observation.Unit = entity.UnitId.HasValue && _artportalenMetadataContainer.Units.ContainsKey(entity.UnitId.Value)
                    ? _artportalenMetadataContainer.Units[entity.UnitId.Value]
                    : null;
                observation.Unspontaneous = entity.Unspontaneous;
                observation.UnsureDetermination = entity.UnsureDetermination;
                observation.SightingBarcodeURL = entity.SightingBarcodeURL;
                observation.ValidationStatus = _artportalenMetadataContainer.ValidationStatus.ContainsKey(entity.ValidationStatusId)
                    ? _artportalenMetadataContainer.ValidationStatus[entity.ValidationStatusId]
                    : null;
                observation.Weight = entity.Weight;
                observation.Projects = projects;
                observation.SightingTypeId = entity.SightingTypeId;
                observation.SightingTypeSearchGroupId = entity.SightingTypeSearchGroupId;
                observation.DeterminedBy = entity.DeterminerUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.DeterminerUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.DeterminerUserId.Value].FullName : null;
                observation.DeterminationYear = entity.DeterminationYear;
                observation.ConfirmedBy = entity.ConfirmatorUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.ConfirmatorUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.ConfirmatorUserId.Value].FullName : null;
                observation.ConfirmationYear = entity.ConfirmationYear;

                observation.RegionalSightingStateId = entity.RegionalSightingStateId;
                observation.SightingPublishTypeIds = ConvertCsvStringToListOfIntegers(entity.SightingPublishTypeIds);
                observation.SpeciesFactsIds = ConvertCsvStringToListOfIntegers(entity.SpeciesFactsIds);

                if (speciesCollectionItems?.Any() ?? false)
                {
                    var speciesCollectionItemEntity = speciesCollectionItems.OrderByDescending(sci => sci.Id).First();

                    observation.CollectionID = speciesCollectionItemEntity.Label;
                    observation.Label = speciesCollectionItemEntity.Label;
                    observation.SightingSpeciesCollectionItemId = speciesCollectionItemEntity.Id;
                    observation.PublicCollection = speciesCollectionItemEntity.OrganizationId.HasValue && _artportalenMetadataContainer.Organizations.ContainsKey(speciesCollectionItemEntity.OrganizationId.Value)
                        ? _artportalenMetadataContainer.Organizations[speciesCollectionItemEntity.OrganizationId.Value]
                        : null;
                    observation.PrivateCollection = speciesCollectionItemEntity.CollectorId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(speciesCollectionItemEntity.CollectorId.Value)
                        ? _artportalenMetadataContainer.PersonByUserId[speciesCollectionItemEntity.CollectorId.Value].FullName
                        : null;
                }
                
                if (personSighting != null)
                {
                    observation.VerifiedBy = personSighting.VerifiedBy;
                    observation.VerifiedByInternal = personSighting.VerifiedByInternal;
                    observation.Observers = personSighting.Observers;
                    observation.ObserversInternal = personSighting.ObserversInternal;
                    observation.ReportedBy = personSighting.ReportedBy;
                    observation.SpeciesCollection = personSighting.SpeciesCollection;
                    observation.ReportedByUserId = personSighting.ReportedByUserId;
                    observation.ReportedByUserServiceUserId = personSighting.ReportedByUserServiceUserId;
                    observation.ReportedByUserAlias = personSighting.ReportedByUserAlias;
                }

                observation.HasImages = entity.HasImages;
                if (media?.Any() ?? false)
                {
                    observation.Media = media;
                    observation.FirstImageId = media?.FirstOrDefault(m => m.FileType?.Equals("image", StringComparison.CurrentCultureIgnoreCase) ?? false)?.Id ?? 0;
                }

                return observation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to cast Artportalen entity with SightingId={sightingId}");
                throw;
            }
        }

        private static IEnumerable<int> ConvertCsvStringToListOfIntegers(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var stringIds = s.Split(",");
            var ids = new HashSet<int>();

            foreach (var stringId in stringIds)
            {
                if (int.TryParse(stringId, out var id))
                {
                    ids.Add(id);
                }
            }

            return ids.Any() ? ids : null;
        }

        #region Media

        private async Task<Media> CastMediaEntityToVerbatimAsync(MediaEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Media
            {
                CopyrightText = entity.CopyrightText,
                FileType = entity.FileType,
                FileUri = entity.FileUri,
                Id = entity.Id,
                RightsHolder = entity.RightsHolder,
                UploadDateTime = entity.UploadDateTime
            };
        }

        private async Task<IDictionary<int, ICollection<Media>>> GetSightingMediaAsync(IEnumerable<int> sightingIds, bool live)
        {
            var sightingsMedias = new Dictionary<int, ICollection<Media>>();

            if (!sightingIds?.Any() ?? true)
            {
                return sightingsMedias;
            }

            var sightingMediaEntities = await _mediaRepository.GetAsync(sightingIds, live);

            if (sightingMediaEntities == null)
            {
                return sightingsMedias;
            }

            foreach (var sightingMediaEntity in sightingMediaEntities)
            {
                var media = await CastMediaEntityToVerbatimAsync(sightingMediaEntity);

                if (!sightingsMedias.TryGetValue(sightingMediaEntity.SightingId, out var sightingMedia))
                {
                    sightingMedia = new List<Media>();
                    sightingsMedias.Add(sightingMediaEntity.SightingId, sightingMedia);
                }

                sightingMedia.Add(media);
            }

            return sightingsMedias;
        }
        #endregion Media

        #region Project

        /// <summary>
        ///     Cast project parameter itemEntity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ProjectParameter CastProjectParameterEntityToVerbatim(ProjectParameterEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new ProjectParameter
            {
                Id = entity.ProjectParameterId,
                DataType = entity.DataType,
                Description = entity.Description,
                Name = entity.Name,
                Unit = entity.Unit,
                Value = entity.Value
            };
        }

        private async Task<IDictionary<int, Project[]>> GetSightingsProjects(IEnumerable<int> sightingIds, bool live)
        {
            if (!_artportalenMetadataContainer?.Projects?.Any() ?? true)
            {
                return null;
            }

            var sightingProjectIds = (await _sightingRepository.GetSightingProjectIdsAsync(sightingIds))?.ToArray();

            if (!sightingProjectIds?.Any() ?? true)
            {
                return null;
            }

            // Cast a projects to verbatim
            var sightingsProjects = new Dictionary<int, IDictionary<int, Project>>();

            for (var i = 0; i < sightingProjectIds.Length; i++)
            {
                var (sightingId, projectId) = sightingProjectIds[i];

                if (!sightingsProjects.TryGetValue(sightingId, out var sightingProjects))
                {
                    sightingProjects = new Dictionary<int, Project>();
                    sightingsProjects.TryAdd(sightingId, sightingProjects);
                }

                if (!sightingProjects.ContainsKey(projectId))
                {
                    if (!_artportalenMetadataContainer.Projects.TryGetValue(projectId, out var project))
                    {
                        continue;
                    }

                    // Make a copy of project so we can add params to it later
                    sightingProjects.TryAdd(project.Id, project.Clone());
                }
            }

            var projectParameterEntities = (await _projectRepository.GetSightingProjectParametersAsync(sightingIds, IncrementalMode))?.ToArray();

            if (projectParameterEntities?.Any() ?? false)
            {
                for (var i = 0; i < projectParameterEntities.Length; i++)
                {
                    var projectParameterEntity = projectParameterEntities[i];
                    
                    // Try to get projects by sighting id
                    if (!sightingsProjects.TryGetValue(projectParameterEntity.SightingId, out var sightingProjects))
                    {
                        sightingProjects = new Dictionary<int, Project>();
                        sightingsProjects.TryAdd(projectParameterEntity.SightingId, sightingProjects);
                    }

                    // Try to get sighting project 
                    if (!sightingProjects.TryGetValue(projectParameterEntity.ProjectId, out var project))
                    {
                        if (!_artportalenMetadataContainer.Projects.TryGetValue(projectParameterEntity.ProjectId, out project))
                        {
                            continue;
                        }

                        project = project.Clone();
                        sightingProjects.TryAdd(project.Id, project);
                    }
                    project.ProjectParameters ??= new List<ProjectParameter>();
                    project.ProjectParameters.Add(CastProjectParameterEntityToVerbatim(projectParameterEntity));
                }
            }

            return sightingsProjects.Any() ? sightingsProjects.ToDictionary(sp => sp.Key, sp => sp.Value.Values.ToArray()) : null;
        }
        #endregion Project        

        #region SightingRelations
        /// <summary>
        /// Cast sighting relations to verbatim
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static IEnumerable<SightingRelation> CastSightingRelationsToVerbatim(IEnumerable<SightingRelationEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            return from e in entities
                   select new SightingRelation
                   {
                       DeterminationYear = e.DeterminationYear,
                       Discover = e.Discover,
                       EditDate = e.EditDate,
                       Id = e.Id,
                       IsPublic = e.IsPublic,
                       RegisterDate = e.RegisterDate,
                       SightingId = e.SightingId,
                       SightingRelationTypeId = e.SightingRelationTypeId,
                       Sort = e.Sort,
                       UserId = e.UserId
                   }; 
        }
        #endregion SightingRelations

        #region SpeciesCollections
       

        /// <summary>
        /// Initialize species collections
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        private async Task<IDictionary<int, ICollection<SpeciesCollectionItemEntity>>> GetSpeciesCollections(IEnumerable<int> sightingIds)
        {
            var speciesCollectionsBySightingId = new Dictionary<int, ICollection<SpeciesCollectionItemEntity>>();

            var speciesCollections =
                (await _speciesCollectionRepository.GetBySightingAsync(sightingIds, IncrementalMode))?.ToArray();

            if (!speciesCollections?.Any() ?? true)
            {
                return speciesCollectionsBySightingId;
            }

            foreach (var speciesCollection in speciesCollections)
            {
                if (!speciesCollectionsBySightingId.TryGetValue(speciesCollection.SightingId, out var sightingSpeciesCollections))
                {
                    sightingSpeciesCollections = new HashSet<SpeciesCollectionItemEntity>();
                    speciesCollectionsBySightingId.Add(speciesCollection.SightingId, sightingSpeciesCollections);
                }

                sightingSpeciesCollections.Add(speciesCollection);
            }

            return speciesCollectionsBySightingId;
        }
        #endregion SpeciesCollections


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mediaRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="sightingRelationRepository"></param>
        /// <param name="speciesCollectionRepository"></param>
        /// <param name="artportalenMetadataContainer"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenHarvestFactory(
            IMediaRepository mediaRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionRepository,
            IArtportalenMetadataContainer artportalenMetadataContainer,
            IAreaHelper areaHelper,
            ILogger<ArtportalenObservationHarvester> logger) : base(siteRepository, areaHelper)
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _projectRepository = projectRepository;
            _sightingRepository = sightingRepository;
            _sightingRelationRepository = sightingRelationRepository;
            _speciesCollectionRepository = speciesCollectionRepository;
            _artportalenMetadataContainer = artportalenMetadataContainer;
            _logger = logger;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<ArtportalenObservationVerbatim>> CastEntitiesToVerbatimsAsync(SightingEntity[] entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            var sightingIds = new HashSet<int>();
            var newSiteIds = new HashSet<int>();

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                sightingIds.Add(entity.Id);
                var siteId = entity.SiteId ?? 0;

                // Check for new sites since we already lopping the array 
                if (siteId == 0 || newSiteIds.Contains(siteId) || Sites.ContainsKey(siteId))
                {
                    continue;
                }
           
                newSiteIds.Add(siteId);
            }
            
            await AddMissingSitesAsync(newSiteIds);
            var sightingsProjects = await GetSightingsProjects(sightingIds, IncrementalMode);

            // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
            var sightingRelations =
                CastSightingRelationsToVerbatim(await _sightingRelationRepository.GetAsync(sightingIds, IncrementalMode))?.ToArray();

            var speciesCollectionsBySightingId = await GetSpeciesCollections(sightingIds);

            var personSightings = PersonSightingFactory.CreatePersonSightingDictionary(
                sightingIds,
                _artportalenMetadataContainer.PersonByUserId,
                _artportalenMetadataContainer.Organizations,
                speciesCollectionsBySightingId,
                sightingRelations);

            var sightingsMedias = await GetSightingMediaAsync(sightingIds, IncrementalMode);

            var verbatims = new HashSet<ArtportalenObservationVerbatim>();

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];

                speciesCollectionsBySightingId.TryGetValue(entity.Id, out var speciesCollections);
                PersonSighting? personSighting = null;
                personSightings?.TryGetValue(entity.Id, out personSighting);
                Project[]? projects = null;
                sightingsProjects?.TryGetValue(entity.Id, out projects);
                ICollection<Media>? media = null;
                sightingsMedias?.TryGetValue(entity.Id, out media);

                var verbatim = CastEntityToVerbatim(entity, speciesCollections, personSighting, projects, media);

                if (verbatim == null)
                {
                    continue;
                }

                verbatims.Add(verbatim);
            }

            return verbatims;
        }

    }
}
