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
        private readonly ILogger<IObservationHarvester> _logger;

        /// <summary>
        /// Cast sighting itemEntity to model
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="site"></param>
        /// <param name="speciesCollectionItems"></param>
        /// <param name="personSighting"></param>
        /// <param name="projects"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        private async Task<ArtportalenObservationVerbatim?> CastEntityToVerbatimAsync(SightingEntity? entity,
            Site site,
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

                sightingId = entity.Id;

                var speciesGroupId = _artportalenMetadataContainer.TryGetTaxonSpeciesGroupId(entity.TaxonId ?? 0);

                var observation = new ArtportalenObservationVerbatim();
                observation.Activity = _artportalenMetadataContainer.TryGetActivity(entity.ActivityId);

                observation.Biotope = _artportalenMetadataContainer.TryGetBiotope(entity.BiotopeId);
                observation.BiotopeDescription = entity.BiotopeDescription;
                observation.ChecklistId = entity.ChecklistId;
                observation.Comment = entity.Comment;
                observation.DatasourceId = entity.DatasourceId;
                observation.DiscoveryMethod = _artportalenMetadataContainer.TryGetDiscoveryMethod(entity.DiscoveryMethodId);
                observation.DeterminationMethod = _artportalenMetadataContainer.TryGetDeterminationMethod(entity.DeterminationMethodId);
                observation.EditDate = entity.EditDate;
                observation.EndDate = entity.EndDate;
                observation.EndTime = entity.EndTime;
                observation.FieldDiaryGroupId = entity.FieldDiaryGroupId;
                observation.Gender = _artportalenMetadataContainer.TryGetGender(entity.GenderId);
                observation.HasTriggeredValidationRules = entity.HasTriggeredValidationRules;
                observation.HasAnyTriggeredValidationRuleWithWarning = entity.HasAnyTriggeredValidationRuleWithWarning;
                observation.HiddenByProvider = entity.HiddenByProvider;
                observation.Id = NextId;
                observation.SightingId = entity.Id;
                observation.OwnerOrganization = _artportalenMetadataContainer.TryGetOrganization(entity.OwnerOrganizationId);
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
                observation.RightsHolder = entity.RightsHolder;
                observation.Site = site;
                observation.SpeciesGroupId = speciesGroupId;
                observation.Stage = _artportalenMetadataContainer.TryGetStage(entity.StageId);
                observation.StartDate = entity.StartDate;
                observation.StartTime = entity.StartTime;
                observation.Substrate = _artportalenMetadataContainer.TryGetSubstrate(entity.SubstrateId);
                observation.SubstrateDescription = entity.SubstrateDescription;
                observation.SubstrateSpeciesDescription = entity.SubstrateSpeciesDescription;
                observation.SubstrateSpeciesId = entity.SubstrateSpeciesId;
                observation.Summary = _artportalenMetadataContainer.TryGetSummary(entity.Summary, entity.IsFreeTextSummary);
                observation.TaxonId = entity.TaxonId;
                observation.TriggeredObservationRuleFrequencyId = entity.TriggeredObservationRuleFrequencyId;
                observation.TriggeredObservationRuleReproductionId = entity.TriggeredObservationRuleReproductionId;
                observation.TriggeredObservationRuleUnspontaneous = entity.TriggeredObservationRuleUnspontaneous;
                observation.Unit = _artportalenMetadataContainer.TryGetUnit(entity.UnitId);
                observation.Unspontaneous = entity.Unspontaneous;
                observation.UnsureDetermination = entity.UnsureDetermination;
                observation.SightingBarcodeURL = entity.SightingBarcodeURL;
                observation.ValidationStatus = _artportalenMetadataContainer.TryGetValidationStatus(entity.ValidationStatusId);
                observation.Weight = entity.Weight;
                observation.Projects = projects;
                observation.SightingTypeId = entity.SightingTypeId;
                observation.SightingTypeSearchGroupId = entity.SightingTypeSearchGroupId;
                observation.DeterminedBy = (await _artportalenMetadataContainer.TryGetPersonByUserIdAsync(entity.DeterminerUserId))?.FullName;
                observation.DeterminationYear = entity.DeterminationYear;
                observation.ConfirmedBy = (await _artportalenMetadataContainer.TryGetPersonByUserIdAsync(entity.ConfirmatorUserId))?.FullName;
                observation.ConfirmationYear = entity.ConfirmationYear;

                //observation.RegionalSightingStateId = entity.RegionalSightingStateId;
                observation.SightingPublishTypeIds = ConvertCsvStringToListOfIntegers(entity.SightingPublishTypeIds);
                observation.SpeciesFactsIds = ConvertCsvStringToListOfIntegers(entity.SpeciesFactsIds);

                if (speciesCollectionItems?.Any() ?? false)
                {
                    var speciesCollectionItemEntity = speciesCollectionItems.OrderByDescending(sci => sci.Id).First();

                    observation.CollectionID = speciesCollectionItemEntity.Label;
                    observation.Label = speciesCollectionItemEntity.Label;
                    observation.SightingSpeciesCollectionItemId = speciesCollectionItemEntity.Id;
                    observation.PublicCollection = _artportalenMetadataContainer.TryGetOrganization(speciesCollectionItemEntity.OrganizationId);
                    observation.PrivateCollection = (await _artportalenMetadataContainer.TryGetPersonByUserIdAsync(speciesCollectionItemEntity.CollectorId))?.FullName;
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
                    observation.FirstImageId = media?.FirstOrDefault()?.Id ?? 0;
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
                return null!;
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

            return ids.Any() ? ids : null!;
        }

        #region Media

        private async Task<IDictionary<int, Media[]>> GetSightingMediaAsync(IEnumerable<int> sightingIds)
        {
            var sightingsMedias = new Dictionary<int, IDictionary<int, Media>>();

            if (!sightingIds?.Any() ?? true)
            {
                return null!;
            }

            var sightingMediaEntities = await _mediaRepository.GetAsync(sightingIds!);
            if (!sightingMediaEntities?.Any() ?? true)
            {
                return null!;
            }

            foreach (var sightingMediaEntity in sightingMediaEntities!)
            {
                // Check if a directory of sighting media exists. If not create it
                if (!sightingsMedias.TryGetValue(sightingMediaEntity.SightingId, out var sightingMedia))
                {
                    sightingMedia = new Dictionary<int, Media>();
                    sightingsMedias.Add(sightingMediaEntity.SightingId, sightingMedia);
                }

                // Check if the media file allready is added to the sighting collection, if not add it
                if (!sightingMedia.TryGetValue(sightingMediaEntity.Id, out var media))
                {
                    media = new Media
                    {
                        Comments = new List<MediaComment>(),
                        CopyrightText = sightingMediaEntity.CopyrightText,
                        FileType = sightingMediaEntity.FileType,
                        FileUri = sightingMediaEntity.FileUri,
                        Id = sightingMediaEntity.Id,
                        RightsHolder = sightingMediaEntity.RightsHolder,
                        UploadDateTime = sightingMediaEntity.UploadDateTime
                    };
                    sightingMedia.Add(media.Id, media);
                }

                // If we have a media comment, add it to media
                if (!string.IsNullOrEmpty(sightingMediaEntity.Comment))
                {
                    media.Comments.Add(new MediaComment
                    {
                        Comment = sightingMediaEntity.Comment,
                        CommentBy = sightingMediaEntity.CommentBy,
                        CommentCreated = sightingMediaEntity.CommentCreated
                    });
                }
            }

            return sightingsMedias.ToDictionary(i => i.Key, v => v.Value.Select(m => m.Value).ToArray());
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
                return null!;
            }

            return new ProjectParameter
            {
                Id = entity.ProjectParameterId,
                DataType = entity.DataType,
                Description = entity.Description,
                Name = entity.Name.Clean(),
                Unit = entity.Unit,
                Value = entity.Value.Clean()
            };
        }

        private async Task<IDictionary<int, Project[]>> GetSightingsProjects(IEnumerable<int> sightingIds)
        {
            var sightingProjectIds = (await _sightingRepository.GetSightingProjectIdsAsync(sightingIds))?.ToArray();

            if (!sightingProjectIds?.Any() ?? true)
            {
                return null!;
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
                    var project = await _artportalenMetadataContainer.TryGetProjectAsync(projectId);
                    if (project == null)
                    {
                        continue;
                    }

                    // Make a copy of project so we can add params to it later
                    sightingProjects.TryAdd(project.Id, project.Clone());
                }
            }

            var projectParameterEntities = (await _projectRepository.GetSightingProjectParametersAsync(sightingIds))?.ToArray();
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
                        project = await _artportalenMetadataContainer.TryGetProjectAsync(projectParameterEntity.ProjectId);
                        if (project == null)
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

            return sightingsProjects.Any() ? sightingsProjects.ToDictionary(sp => sp.Key, sp => sp.Value.Values.ToArray()) : null!;
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
                return null!;
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
                (await _speciesCollectionRepository.GetBySightingAsync(sightingIds))?.ToArray();

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
        /// <param name="live"></param>
        /// <param name="noOfThreads"></param>
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
            int noOfThreads,
            ILogger<IObservationHarvester> logger) : base(siteRepository, areaHelper, noOfThreads)
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _sightingRepository = sightingRepository ?? throw new ArgumentNullException(nameof(sightingRepository));
            _sightingRelationRepository = sightingRelationRepository ?? throw new ArgumentNullException(nameof(sightingRelationRepository));
            _speciesCollectionRepository = speciesCollectionRepository ?? throw new ArgumentNullException(nameof(speciesCollectionRepository));
            _artportalenMetadataContainer = artportalenMetadataContainer ?? throw new ArgumentNullException(nameof(artportalenMetadataContainer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<ArtportalenObservationVerbatim>> CastEntitiesToVerbatimsAsync(SightingEntity[] entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null!;
            }

            var sightingIds = new HashSet<int>();
            var siteIds = new HashSet<int>();

            for (var i = 0; i < entities!.Length; i++)
            {
                var entity = entities[i];
                sightingIds.Add(entity.Id);
                siteIds.Add(entity.SiteId);
            }
            var sites = await GetBatchSitesAsync(siteIds);

            var sightingsProjects = await GetSightingsProjects(sightingIds);

            // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
            var sightingRelations =
                CastSightingRelationsToVerbatim(await _sightingRelationRepository.GetAsync(sightingIds))?.ToArray();

            var speciesCollectionsBySightingId = await GetSpeciesCollections(sightingIds);

            var personSightings = PersonSightingFactory.CreatePersonSightingDictionary(
                sightingIds,
                _artportalenMetadataContainer.PersonsByUserId,
                _artportalenMetadataContainer.Organizations,
                speciesCollectionsBySightingId,
                sightingRelations);

            var sightingsMedias = await GetSightingMediaAsync(sightingIds);

            var verbatims = new HashSet<ArtportalenObservationVerbatim>();

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                sites.TryGetValue(entity.SiteId, out var site);
                speciesCollectionsBySightingId.TryGetValue(entity.Id, out var speciesCollections);
                PersonSighting? personSighting = null;
                personSightings?.TryGetValue(entity.Id, out personSighting);
                Project[]? projects = null;
                sightingsProjects?.TryGetValue(entity.Id, out projects);
                Media[]? media = null;
                sightingsMedias?.TryGetValue(entity.Id, out media);

                var verbatim = await CastEntityToVerbatimAsync(entity, site!, speciesCollections, personSighting, projects, media);

                if (verbatim == null)
                {
                    continue;
                }

                verbatims.Add(verbatim);
            }
            // Clean up
            sites.Clear();
           
            return verbatims;
        }
    }
}
