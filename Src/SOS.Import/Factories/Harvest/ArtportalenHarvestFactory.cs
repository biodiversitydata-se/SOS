
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SOS.Import.Containers.Interfaces;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Factories.Harvest
{
    internal class ArtportalenHarvestFactory : IHarvestFactory<SightingEntity[], ArtportalenObservationVerbatim>
    {
        private readonly IArtportalenMetadataContainer _artportalenMetadataContainer;

        private readonly IProjectRepository _projectRepository;
        private readonly ISightingRepository _sightingRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISightingRelationRepository _sightingRelationRepository;
        private readonly ISpeciesCollectionItemRepository _speciesCollectionRepository;

        private int _idCounter;
        private int NextId => Interlocked.Increment(ref _idCounter);
        
        private readonly ConcurrentDictionary<int, Site> _sites;

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="personSightings"></param>
        /// <param name="sightingsProjects"></param>
        /// <returns></returns>
        private ArtportalenObservationVerbatim CastEntityToVerbatim(SightingEntity entity,
            IDictionary<int, PersonSighting> personSightings,
            IDictionary<int, Project[]> sightingsProjects)
        {
            if (entity == null)
            {
                return null;
            }

            if (_sites.TryGetValue(entity.SiteId.HasValue ? entity.SiteId.Value : -1, out var site))
            {
                // Try to set parent site name if empty
                if (site.ParentSiteId != null && string.IsNullOrEmpty(site.ParentSiteName))
                {
                    if (_sites.TryGetValue(site.ParentSiteId.Value, out var parentSite))
                    {
                        site.ParentSiteName = parentSite.Name;
                    }
                }
            }

            var observation = new ArtportalenObservationVerbatim
            {
                Activity = entity.ActivityId.HasValue && _artportalenMetadataContainer.Activities.ContainsKey(entity.ActivityId.Value)
                    ? _artportalenMetadataContainer.Activities[entity.ActivityId.Value]
                    : null,
                Biotope = entity.BiotopeId.HasValue && _artportalenMetadataContainer.Biotopes.ContainsKey(entity.BiotopeId.Value)
                    ? _artportalenMetadataContainer.Biotopes[entity.BiotopeId.Value]
                    : null,
                BiotopeDescription = entity.BiotopeDescription,
                CollectionID = entity.CollectionID,
                Comment = entity.Comment,
                DiscoveryMethod = entity.DiscoveryMethodId.HasValue && _artportalenMetadataContainer.DiscoveryMethods.ContainsKey(entity.DiscoveryMethodId.Value)
                    ? _artportalenMetadataContainer.DiscoveryMethods[entity.DiscoveryMethodId.Value]
                    : null,
                DeterminationMethod = entity.DeterminationMethodId.HasValue && _artportalenMetadataContainer.DeterminationMethods.ContainsKey(entity.DeterminationMethodId.Value)
                    ? _artportalenMetadataContainer.DeterminationMethods[entity.DeterminationMethodId.Value]
                    : null,
                EditDate = entity.EditDate,
                EndDate = entity.EndDate,
                EndTime = entity.EndTime,
                Gender = entity.GenderId.HasValue && _artportalenMetadataContainer.Genders.ContainsKey(entity.GenderId.Value)
                    ? _artportalenMetadataContainer.Genders[entity.GenderId.Value]
                    : null,
                HasImages = entity.HasImages,
                HasTriggeredValidationRules = entity.HasTriggeredValidationRules,
                HasAnyTriggeredValidationRuleWithWarning = entity.HasAnyTriggeredValidationRuleWithWarning,
                HiddenByProvider = entity.HiddenByProvider,
                Id = NextId,
                SightingId = entity.Id,
                OwnerOrganization =
                    entity.OwnerOrganizationId.HasValue &&
                    _artportalenMetadataContainer.Organizations.ContainsKey(entity.OwnerOrganizationId.Value)
                        ? _artportalenMetadataContainer.Organizations[entity.OwnerOrganizationId.Value]
                        : null,
                Label = entity.Label,
                Length = entity.Length,
                MaxDepth = entity.MaxDepth,
                MaxHeight = entity.MaxHeight,
                MigrateSightingObsId = entity.MigrateSightingObsId,
                MigrateSightingPortalId = entity.MigrateSightingPortalId,
                MinDepth = entity.MinDepth,
                MinHeight = entity.MinHeight,
                NoteOfInterest = entity.NoteOfInterest,
                NotPresent = entity.NotPresent,
                NotRecovered = entity.NotRecovered,
                ProtectedBySystem = entity.ProtectedBySystem,
                Quantity = entity.Quantity,
                QuantityOfSubstrate = entity.QuantityOfSubstrate,
                ReportedDate = entity.RegisterDate,
                RightsHolder = entity.RightsHolder,
                Site = site,
                SightingSpeciesCollectionItemId = entity.SightingSpeciesCollectionItemId,
                Stage = entity.StageId.HasValue && _artportalenMetadataContainer.Stages.ContainsKey(entity.StageId.Value)
                    ? _artportalenMetadataContainer.Stages[entity.StageId.Value]
                    : null,
                StartDate = entity.StartDate,
                StartTime = entity.StartTime,
                Substrate = entity.SubstrateId.HasValue && _artportalenMetadataContainer.Substrates.ContainsKey(entity.SubstrateId.Value)
                    ? _artportalenMetadataContainer.Substrates[entity.SubstrateId.Value]
                    : null,
                SubstrateDescription = entity.SubstrateDescription,
                SubstrateSpeciesDescription = entity.SubstrateSpeciesDescription,
                SubstrateSpeciesId = entity.SubstrateSpeciesId,
                TaxonId = entity.TaxonId,
                Unit = entity.UnitId.HasValue && _artportalenMetadataContainer.Units.ContainsKey(entity.UnitId.Value)
                    ? _artportalenMetadataContainer.Units[entity.UnitId.Value]
                    : null,
                Unspontaneous = entity.Unspontaneous,
                UnsureDetermination = entity.UnsureDetermination,
                URL = entity.URL,
                ValidationStatus = _artportalenMetadataContainer.ValidationStatus.ContainsKey(entity.ValidationStatusId)
                    ? _artportalenMetadataContainer.ValidationStatus[entity.ValidationStatusId]
                    : null,
                Weight = entity.Weight,
                Projects = sightingsProjects?.ContainsKey(entity.Id) ?? false ? sightingsProjects[entity.Id] : null,
                SightingTypeId = entity.SightingTypeId,
                SightingTypeSearchGroupId = entity.SightingTypeSearchGroupId,
                PublicCollection = entity.OrganizationCollectorId.HasValue && _artportalenMetadataContainer.Organizations.ContainsKey(entity.OrganizationCollectorId.Value)
                    ? _artportalenMetadataContainer.Organizations[entity.OrganizationCollectorId.Value]
                    : null,
                PrivateCollection = entity.UserCollectorId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.UserCollectorId.Value)
                    ? _artportalenMetadataContainer.PersonByUserId[entity.UserCollectorId.Value].FullName
                    : null,
                DeterminedBy = entity.DeterminerUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.DeterminerUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.DeterminerUserId.Value].FullName : null,
                DeterminationYear = entity.DeterminationYear,
                ConfirmedBy = entity.ConfirmatorUserId.HasValue && _artportalenMetadataContainer.PersonByUserId.ContainsKey(entity.ConfirmatorUserId.Value) ? _artportalenMetadataContainer.PersonByUserId[entity.ConfirmatorUserId.Value].FullName : null,
                ConfirmationYear = entity.ConfirmationYear
            };

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
                observation.ReportedByUserAlias = personSighting.ReportedByUserAlias;
            }

            return observation;
        }

        private static List<int> ConvertCsvStringToListOfIntegers(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var stringIds = s.Split(",");
            var ids = new List<int>();

            foreach (var stringId in stringIds)
            {
                if (int.TryParse(stringId, out var id))
                {
                    ids.Add(id);
                }
            }

            return ids.Any() ? ids : null;
        }

        #region Project
        
        /// <summary>
        ///     Cast project parameter itemEntity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ProjectParameter CastProjectParameterEntityToVerbatim(ProjectParameterEntity entity)
        {
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

        private async Task<IDictionary<int, Project[]>> GetSightingsProjects(IEnumerable<int> sightingIds)
        {
            if (!_artportalenMetadataContainer?.Projects?.Any() ?? true)
            {
                return null;
            }

            var projectParameterEntities = await _projectRepository.GetSightingProjectParametersAsync(sightingIds);
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

                if (!_artportalenMetadataContainer.Projects.TryGetValue(projectId, out var project))
                {
                    continue;
                }

                if (!sightingsProjects.TryGetValue(sightingId, out var sightingProjects))
                {
                    sightingProjects = new Dictionary<int, Project>();
                    sightingsProjects.Add(sightingId, sightingProjects);
                }

                if (!sightingProjects.ContainsKey(projectId))
                {
                    sightingProjects.Add(project.Id, project.Clone());
                }
            }

            foreach (var projectParameterEntity in projectParameterEntities)
            {
                Project project = null;

                // Try to get projects by sighting id
                if (sightingsProjects.TryGetValue(projectParameterEntity.SightingId, out var sightingProjects))
                {
                    // Try to get sighting project 
                    sightingProjects.TryGetValue(projectParameterEntity.ProjectId, out project);
                }
                else
                {
                    // Sighting projects is missing, add it
                    sightingProjects = new Dictionary<int, Project>();
                    sightingsProjects.Add(projectParameterEntity.SightingId, sightingProjects);
                }

                if (project == null)
                {
                    if (!_artportalenMetadataContainer.Projects.ContainsKey(projectParameterEntity.ProjectId))
                    {
                        continue;
                    }

                    // Get project from all projects
                    project = _artportalenMetadataContainer.Projects[projectParameterEntity.ProjectId].Clone();
                    sightingProjects.Add(project.Id, project.Clone());
                }

                if (project.ProjectParameters == null)
                {
                    project.ProjectParameters = new List<ProjectParameter>();
                }

                project.ProjectParameters.Add(CastProjectParameterEntityToVerbatim(projectParameterEntity));
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
        private async Task AddMissingSitesAsync(HashSet<int> newSiteIds)
        {
            if (!newSiteIds?.Any() ?? true)
            {
                return;
            }

            var batchSize = 10000;
            var startIndex = 0;
            var idBatch = newSiteIds.Take(batchSize).ToArray();

            while (idBatch?.Any() ?? false)
            {
                var sites = CastSiteEntitiesToVerbatim((await _siteRepository.GetByIdsAsync(idBatch, IncrementalMode))?.ToList());

                if (sites?.Any() ?? false)
                {
                    foreach (var site in sites)
                    {
                        _sites.TryAdd(site.Id, site);
                    }

                    startIndex += batchSize;
                    idBatch = newSiteIds.Skip(startIndex).Take(batchSize)?.ToArray();
                }
            }
        }

        /// <summary>
        ///     Cast multiple sites entities to models by continuously decreasing the siteEntities input list.
        ///     This saves about 500MB RAM when casting Artportalen sites (3 millions).
        /// </summary>
        /// <param name="siteEntities"></param>
        /// <returns></returns>
        private IEnumerable<Site> CastSiteEntitiesToVerbatim(List<SiteEntity> siteEntities)
        {
            var sites = new List<Site>();

            if (!siteEntities?.Any() ?? true)
            {
                return sites;
            }

            var batchSize = 100000;
            while (siteEntities.Count > 0)
            {
                var sitesBatch = siteEntities.Take(batchSize).ToArray();
                sites.AddRange(from s in sitesBatch
                               select CastSiteEntityToVerbatim(s));
                siteEntities.RemoveRange(0, sitesBatch?.Count() ?? 0);
            }
            return sites;
        }

        /// <summary>
        ///     Cast site itemEntity to aggregate
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Site CastSiteEntityToVerbatim(SiteEntity entity)
        {
            Point wgs84Point = null;
            const int defaultAccuracy = 100;

            if (entity.XCoord > 0 && entity.YCoord > 0)
            {
                // We process point here since site is added to observation verbatim. One site can have multiple observations and by 
                // doing it here we only have to convert the point once
                var webMercatorPoint = new Point(entity.XCoord, entity.YCoord);
                wgs84Point = (Point)webMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
            }

            var accuracy = entity.Accuracy > 0 ? entity.Accuracy : defaultAccuracy; // If Artportalen site accuracy is <= 0, this is due to an old import. Set the accuracy to 100.
            return new Site
            {
                Accuracy = accuracy,
                County = string.IsNullOrEmpty(entity.CountyFeatureId)
                    ? null : new GeographicalArea { FeatureId = entity.CountyFeatureId, Name = entity.CountyName },
                CountryPart = string.IsNullOrEmpty(entity.CountryPartFeatureId)
                    ? null : new GeographicalArea { FeatureId = entity.CountryPartFeatureId, Name = entity.CountryPartName },
                ExternalId = entity.ExternalId,
                Id = entity.Id,
                Municipality = string.IsNullOrEmpty(entity.MunicipalityFeatureId)
                    ? null : new GeographicalArea { FeatureId = entity.MunicipalityFeatureId, Name = entity.MunicipalityName },
                Province = string.IsNullOrEmpty(entity.ProvinceFeatureId)
                    ? null : new GeographicalArea { FeatureId = entity.ProvinceFeatureId, Name = entity.ProvinceName },
                Parish = string.IsNullOrEmpty(entity.ParishFeatureId)
                    ? null : new GeographicalArea { FeatureId = entity.ParishFeatureId, Name = entity.ParishName },
                PresentationNameParishRegion = entity.PresentationNameParishRegion,
                Point = wgs84Point?.ToGeoJson(),
                PointWithBuffer = wgs84Point?.ToCircle(accuracy)?.ToGeoJson(),
                Name = entity.Name,
                XCoord = entity.XCoord,
                YCoord = entity.YCoord,
                VerbatimCoordinateSystem = CoordinateSys.WebMercator,
                ParentSiteId = entity.ParentSiteId
            };
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
            return CastSpeciesCollectionsToVerbatim(await _speciesCollectionRepository.GetBySightingAsync(sightingIds))?.ToList();
        }
        #endregion SpeciesCollections


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectRepository"></param>
        /// <param name="sightingRepository"></param>
        /// <param name="siteRepository"></param>
        /// <param name="sightingRelationRepository"></param>
        /// <param name="speciesCollectionRepository"></param>
        /// <param name="artportalenMetadataContainer"></param>
        public ArtportalenHarvestFactory(
            IProjectRepository projectRepository,
            ISightingRepository sightingRepository,
            ISiteRepository siteRepository,
            ISightingRelationRepository sightingRelationRepository,
            ISpeciesCollectionItemRepository speciesCollectionRepository,
            IArtportalenMetadataContainer artportalenMetadataContainer)
        {
            _projectRepository = projectRepository;
            _sightingRepository = sightingRepository;
            _siteRepository = siteRepository;
            _sightingRelationRepository = sightingRelationRepository;
            _speciesCollectionRepository = speciesCollectionRepository;

            _artportalenMetadataContainer = artportalenMetadataContainer;
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
            var sightingsProjects = await GetSightingsProjects(sightingIds);

            // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
            var sightingRelations =
                CastSightingRelationsToVerbatim(await _sightingRelationRepository.GetAsync(sightingIds))?.ToArray();

            var speciesCollections = await GetSpeciesCollections(sightingIds);

            var personSightings = PersonSightingFactory.CreatePersonSightingDictionary(
                sightingIds,
                _artportalenMetadataContainer.PersonByUserId,
                _artportalenMetadataContainer.OrganizationById,
                speciesCollections,
                sightingRelations);

            var verbatims = new List<ArtportalenObservationVerbatim>();
            for (var i = 0; i < entities.Length; i++)
            {
                verbatims.Add(CastEntityToVerbatim(entities[i], personSightings, sightingsProjects));
            }

            return verbatims;
        }

    }
}
