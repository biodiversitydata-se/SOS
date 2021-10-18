using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using SOS.Import.Containers.Interfaces;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Factories.Harvest
{
    internal class ArtportalenHarvestFactory : HarvestBaseFactory, IHarvestFactory<SightingEntity[], ArtportalenObservationVerbatim>
    {
        private readonly IArtportalenMetadataContainer _artportalenMetadataContainer;
        private readonly IMediaRepository _mediaRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ConcurrentDictionary<int, Site> _sites;
        private readonly ILogger<ArtportalenObservationHarvester> _logger;

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="personSightings"></param>
        /// <param name="sightingsProjects"></param>
        /// <param name="sightingMedia"></param>
        /// <returns></returns>
        private ArtportalenObservationVerbatim CastEntityToVerbatim(SightingEntity entity,
            IDictionary<int, PersonSighting> personSightings,
            IDictionary<int, Project[]> sightingsProjects,
            IDictionary<int, ICollection<Media>> sightingMedias)
        {
            var sightingId = -1;

            try
            {
                if (entity == null)
                {
                    return null;
                }

                sightingId = entity.Id;
                if (_sites.TryGetValue(entity.SiteId.HasValue ? entity.SiteId.Value : -1, out var site))
                {
                    // Try to set parent site name if empty
                    if (site?.ParentSiteId != null && string.IsNullOrEmpty(site.ParentSiteName))
                    {
                        if (_sites.TryGetValue(site.ParentSiteId.Value, out var parentSite))
                        {
                            site.ParentSiteName = parentSite.Name;
                        }
                    }
                }

                var observation = new ArtportalenObservationVerbatim();
                observation.Activity = entity.ActivityId.HasValue && _artportalenMetadataContainer.Activities.ContainsKey(entity.ActivityId.Value)
                    ? _artportalenMetadataContainer.Activities[entity.ActivityId.Value]
                    : null;
                observation.Biotope = entity.BiotopeId.HasValue && _artportalenMetadataContainer.Biotopes.ContainsKey(entity.BiotopeId.Value)
                    ? _artportalenMetadataContainer.Biotopes[entity.BiotopeId.Value]
                    : null;
                observation.BiotopeDescription = entity.BiotopeDescription;
                observation.CollectionID = entity.CollectionID;
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
                observation.Gender = entity.GenderId.HasValue && _artportalenMetadataContainer.Genders.ContainsKey(entity.GenderId.Value)
                    ? _artportalenMetadataContainer.Genders[entity.GenderId.Value]
                    : null;
                observation.HasImages = entity.HasImages;
                observation.FirstImageId = entity.FirstImageId;
                observation.HasTriggeredValidationRules = entity.HasTriggeredValidationRules;
                observation.HasAnyTriggeredValidationRuleWithWarning = entity.HasAnyTriggeredValidationRuleWithWarning;
                observation.HiddenByProvider = entity.HiddenByProvider;
                observation.Id = NextId;
                observation.SightingId = entity.Id;
                observation.OwnerOrganization = entity.OwnerOrganizationId.HasValue &&
                                                _artportalenMetadataContainer.Organizations.ContainsKey(entity.OwnerOrganizationId.Value)
                    ? _artportalenMetadataContainer.Organizations[entity.OwnerOrganizationId.Value]
                    : null;
                observation.Label = entity.Label;
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
                observation.SightingSpeciesCollectionItemId = entity.SightingSpeciesCollectionItemId;
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
                observation.URL = entity.URL;
                observation.ValidationStatus = _artportalenMetadataContainer.ValidationStatus.ContainsKey(entity.ValidationStatusId)
                    ? _artportalenMetadataContainer.ValidationStatus[entity.ValidationStatusId]
                    : null;
                observation.Weight = entity.Weight;
                observation.Projects = sightingsProjects?.ContainsKey(entity.Id) ?? false ? sightingsProjects[entity.Id] : null;
                observation.SightingTypeId = entity.SightingTypeId;
                observation.SightingTypeSearchGroupId = entity.SightingTypeSearchGroupId;
                observation.PublicCollection = entity.OrganizationCollectorId.HasValue && _artportalenMetadataContainer.Organizations.ContainsKey(entity.OrganizationCollectorId.Value)
                    ? _artportalenMetadataContainer.Organizations[entity.OrganizationCollectorId.Value]
                    : null;
                observation.PrivateCollection = entity.UserCollectorId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.UserCollectorId.Value)
                    ? _artportalenMetadataContainer.PersonByUserId[entity.UserCollectorId.Value].FullName
                    : null;
                observation.DeterminedBy = entity.DeterminerUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.DeterminerUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.DeterminerUserId.Value].FullName : null;
                observation.DeterminationYear = entity.DeterminationYear;
                observation.ConfirmedBy = entity.ConfirmatorUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.ConfirmatorUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.ConfirmatorUserId.Value].FullName : null;
                observation.ConfirmationYear = entity.ConfirmationYear;

                observation.RegionalSightingStateId = entity.RegionalSightingStateId;
                observation.SightingPublishTypeIds = ConvertCsvStringToListOfIntegers(entity.SightingPublishTypeIds);
                observation.SpeciesFactsIds = ConvertCsvStringToListOfIntegers(entity.SpeciesFactsIds);

                if (personSightings.TryGetValue(entity.Id, out var personSighting))
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

                if (sightingMedias.TryGetValue(entity.Id, out var media))
                {
                    observation.Media = media;
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
                        var projectEntity = await _projectRepository.GetProjectAsync(projectId, live);

                        if (projectEntity == null)
                        {
                            continue;
                        }
                        _artportalenMetadataContainer.AddProject(projectEntity);
                        if (!_artportalenMetadataContainer.Projects.TryGetValue(projectId, out project))
                        {
                            continue;
                        }
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
                            var projectEntity = await _projectRepository.GetProjectAsync(projectParameterEntity.ProjectId, live);

                            if (projectEntity == null)
                            {
                                continue;
                            }
                            _artportalenMetadataContainer.AddProject(projectEntity);
                            if(!_artportalenMetadataContainer.Projects.TryGetValue(projectParameterEntity.ProjectId, out project)) 
                            { 
                                continue;
                            }
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

        #region Site
        /// <summary>
        /// Try to add missing sites from live data
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        private async Task AddMissingSitesAsync(IEnumerable<int> siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return;
            }

            var siteEntities = await _siteRepository.GetByIdsAsync(siteIds, IncrementalMode);
            var siteAreas = await _siteRepository.GetSitesAreas(siteIds, IncrementalMode);
            var sitesGeometry = await _siteRepository.GetSitesGeometry(siteIds, IncrementalMode); // It's faster to get geometries in separate query than join it in site query

            var sites = await CastSiteEntitiesToVerbatimAsync(siteEntities?.ToArray(), siteAreas, sitesGeometry);

            if (sites?.Any() ?? false)
            {
                foreach (var site in sites)
                {
                    _sites.TryAdd(site.Id, site);
                }
            }
        }

        /// <summary>
        /// Cast multiple sites entities to models by continuously decreasing the siteEntities input list.
        ///     This saves about 500MB RAM when casting Artportalen sites (3 millions).
        /// </summary>
        /// <param name="siteEntities"></param>
        /// <param name="sitesAreas"></param>
        /// <param name="sitesGeometry"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Site>> CastSiteEntitiesToVerbatimAsync(ICollection<SiteEntity> siteEntities, IDictionary<int, ICollection<AreaEntityBase>> sitesAreas, IDictionary<int, string> sitesGeometry)
        {
            var sites = new List<Site>();

            if (!siteEntities?.Any() ?? true)
            {
                return sites;
            }

            // Make sure metadata are initialized
            sitesAreas ??= new Dictionary<int, ICollection<AreaEntityBase>>();
            sitesGeometry ??= new Dictionary<int, string>();

            foreach (var siteEntity in siteEntities)
            {
                sitesAreas.TryGetValue(siteEntity.Id, out var siteAreas);
                sitesGeometry.TryGetValue(siteEntity.Id, out var geometryWkt);

                var site = await CastSiteEntityToVerbatimAsync(siteEntity, siteAreas, geometryWkt);

                if (site != null)
                {
                    sites.Add(site);
                }
            }
          
            return sites;
        }

        /// <summary>
        /// Cast site itemEntity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="areas"></param>
        /// <param name="geometryWkt"></param>
        /// <returns></returns>
        private async Task<Site> CastSiteEntityToVerbatimAsync(SiteEntity entity, ICollection<AreaEntityBase> areas, string geometryWkt)
        {
            if (entity == null)
            {
                return null;
            }

            Point wgs84Point = null;
            const int defaultAccuracy = 100;

            if (entity.XCoord > 0 && entity.YCoord > 0)
            {
                // We process point here since site is added to observation verbatim. One site can have multiple observations and by 
                // doing it here we only have to convert the point once
                var webMercatorPoint = new Point(entity.XCoord, entity.YCoord);
                wgs84Point = (Point)webMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
            }

            Geometry siteGeometry = null;
            if (!string.IsNullOrEmpty(geometryWkt))
            {
                siteGeometry = geometryWkt.ToGeometry()
                    .Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84).TryMakeValid();
            }

            var accuracy = entity.Accuracy > 0 ? entity.Accuracy : defaultAccuracy; // If Artportalen site accuracy is <= 0, this is due to an old import. Set the accuracy to 100.
            var site = new Site
            {
                Accuracy = accuracy,
                ExternalId = entity.ExternalId,
                Id = entity.Id,
                PresentationNameParishRegion = entity.PresentationNameParishRegion,
                Point = wgs84Point?.ToGeoJson(),
                PointWithBuffer = (siteGeometry?.IsValid() ?? false ? siteGeometry : wgs84Point.ToCircle(accuracy))?.ToGeoJson(),
                Name = entity.Name,
                XCoord = entity.XCoord,
                YCoord = entity.YCoord,
                VerbatimCoordinateSystem = CoordinateSys.WebMercator,
                ParentSiteId = entity.ParentSiteId
            };

            if (!areas?.Any() ?? true)
            {
                return site;
            }

            foreach (var area in areas)
            {
                switch ((AreaType)area.AreaDatasetId)
                {
                    case AreaType.BirdValidationArea:
                        (site.BirdValidationAreaIds ??= new List<string>()).Add(area.FeatureId);
                        break;
                    case AreaType.County:
                        site.County = new GeographicalArea{FeatureId = area.FeatureId, Name = area.Name};
                        break;
                    case AreaType.Municipality:
                        site.Municipality = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                    case AreaType.Parish:
                        site.Parish = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                    case AreaType.Province:
                        site.Province = new GeographicalArea { FeatureId = area.FeatureId, Name = area.Name };
                        break;
                }
            }

            _areaHelper.AddAreaDataToSite(site);

            return site;
        }
        #endregion Site

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
        private IEnumerable<SpeciesCollectionItem> CastSpeciesCollectionsToVerbatim(
            IEnumerable<SpeciesCollectionItemEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            return from s in entities
                select new SpeciesCollectionItem
                {
                    SightingId = s.SightingId,
                    CollectorId = s.CollectorId,
                    OrganizationId = s.OrganizationId,
                    DeterminerText = s.DeterminerText,
                    DeterminerYear = s.DeterminerYear,
                    Description = s.Description,
                    ConfirmatorText = s.ConfirmatorText,
                    ConfirmatorYear = s.ConfirmatorYear
                };
        }

        /// <summary>
        /// Initialize species collections
        /// </summary>
        /// <param name="sightingIds"></param>
        /// <returns></returns>
        private async Task<IList<SpeciesCollectionItem>> GetSpeciesCollections(IEnumerable<int> sightingIds)
        {
            return CastSpeciesCollectionsToVerbatim(await _speciesCollectionRepository.GetBySightingAsync(sightingIds, IncrementalMode))?.ToList();
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
        public ArtportalenHarvestFactory(
            IMediaRepository mediaRepository,
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionRepository,
            IArtportalenMetadataContainer artportalenMetadataContainer,
            IAreaHelper areaHelper,
            ILogger<ArtportalenObservationHarvester> logger) : base()
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _projectRepository = projectRepository;
            _sightingRepository = sightingRepository;
            _siteRepository = siteRepository;
            _sightingRelationRepository = sightingRelationRepository;
            _speciesCollectionRepository = speciesCollectionRepository;
            _artportalenMetadataContainer = artportalenMetadataContainer;
            _areaHelper = areaHelper;
            _logger = logger;
            _sites = new ConcurrentDictionary<int, Site>();
        }

        public bool IncrementalMode { get; set; }
        
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
                if (siteId == 0 || newSiteIds.Contains(siteId) || _sites.ContainsKey(siteId))
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

            var speciesCollections = await GetSpeciesCollections(sightingIds);

            var personSightings = PersonSightingFactory.CreatePersonSightingDictionary(
                sightingIds,
                _artportalenMetadataContainer.PersonByUserId,
                _artportalenMetadataContainer.OrganizationById,
                speciesCollections,
                sightingRelations);

            var sightingsMedias = await GetSightingMediaAsync(sightingIds, IncrementalMode);

            var verbatims = new HashSet<ArtportalenObservationVerbatim>();
            for (var i = 0; i < entities.Length; i++)
            {
                verbatims.Add(CastEntityToVerbatim(entities[i], personSightings, sightingsProjects, sightingsMedias));
            }

            return verbatims;
        }

    }
}
