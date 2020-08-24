using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Factories.Harvest
{
    public class ArtportalenHarvestFactory : IHarvestFactory<SightingEntity[], ArtportalenObservationVerbatim>
    {
        private readonly ISiteRepository _siteRepository;
        private ISightingRelationRepository _sightingRelationRepository;
        private readonly IDictionary<int, MetadataWithCategory> _activities;
        private readonly IDictionary<int, Metadata> _biotopes;
        private readonly IDictionary<int, Metadata> _genders;
        private readonly IDictionary<int, Metadata> _organizations;
        private IDictionary<int, Organization> _organizationById;
        private readonly ConcurrentDictionary<int, Site> _sites;
        private IList<SpeciesCollectionItem> _speciesCollections;
        private readonly IDictionary<int, Metadata> _stages;
        private readonly IDictionary<int, Metadata> _substrates;
        private readonly IDictionary<int, Metadata> _validationStatus;
        private readonly IDictionary<int, Metadata> _units;
        private readonly IDictionary<int, Metadata> _discoveryMethods;
        private readonly IDictionary<int, Metadata> _determinationMethods;
        private IDictionary<int, Person> _personByUserId;
        private IDictionary<int, Project[]> _sightingsProjects;

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="personSightings"></param>
        /// <param name="projectEntityDictionaries"></param>
        /// <returns></returns>
        private ArtportalenObservationVerbatim CastEntityToVerbatim(SightingEntity entity,
            IDictionary<int, PersonSighting> personSightings)
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
                Activity = entity.ActivityId.HasValue && _activities.ContainsKey(entity.ActivityId.Value)
                    ? _activities[entity.ActivityId.Value]
                    : null,
                Biotope = entity.BiotopeId.HasValue && _biotopes.ContainsKey(entity.BiotopeId.Value)
                    ? _biotopes[entity.BiotopeId.Value]
                    : null,
                BiotopeDescription = entity.BiotopeDescription,
                CollectionID = entity.CollectionID,
                Comment = entity.Comment,
                DiscoveryMethod = entity.DiscoveryMethodId.HasValue && _discoveryMethods.ContainsKey(entity.DiscoveryMethodId.Value)
                    ? _discoveryMethods[entity.DiscoveryMethodId.Value]
                    : null,
                DeterminationMethod = entity.DeterminationMethodId.HasValue && _determinationMethods.ContainsKey(entity.DeterminationMethodId.Value)
                    ? _determinationMethods[entity.DeterminationMethodId.Value]
                    : null,
                EndDate = entity.EndDate,
                EndTime = entity.EndTime,
                Gender = entity.GenderId.HasValue && _genders.ContainsKey(entity.GenderId.Value)
                    ? _genders[entity.GenderId.Value]
                    : null,
                HasImages = entity.HasImages,
                HasTriggeredValidationRules = entity.HasTriggeredValidationRules,
                HasAnyTriggeredValidationRuleWithWarning = entity.HasAnyTriggeredValidationRuleWithWarning,
                HiddenByProvider = entity.HiddenByProvider,
                Id = entity.Id,
                OwnerOrganization =
                    entity.OwnerOrganizationId.HasValue &&
                    _organizations.ContainsKey(entity.OwnerOrganizationId.Value)
                        ? _organizations[entity.OwnerOrganizationId.Value]
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
                Stage = entity.StageId.HasValue && _stages.ContainsKey(entity.StageId.Value)
                    ? _stages[entity.StageId.Value]
                    : null,
                StartDate = entity.StartDate,
                StartTime = entity.StartTime,
                Substrate = entity.SubstrateId.HasValue && _substrates.ContainsKey(entity.SubstrateId.Value)
                    ? _substrates[entity.SubstrateId.Value]
                    : null,
                SubstrateDescription = entity.SubstrateDescription,
                SubstrateSpeciesDescription = entity.SubstrateSpeciesDescription,
                SubstrateSpeciesId = entity.SubstrateSpeciesId,
                TaxonId = entity.TaxonId,
                Unit = entity.UnitId.HasValue && _units.ContainsKey(entity.UnitId.Value)
                    ? _units[entity.UnitId.Value]
                    : null,
                Unspontaneous = entity.Unspontaneous,
                UnsureDetermination = entity.UnsureDetermination,
                URL = entity.URL,
                ValidationStatus = _validationStatus.ContainsKey(entity.ValidationStatusId)
                    ? _validationStatus[entity.ValidationStatusId]
                    : null,
                Weight = entity.Weight,
                Projects = _sightingsProjects.ContainsKey(entity.Id) ? _sightingsProjects[entity.Id] : null,
                SightingTypeId = entity.SightingTypeId,
                SightingTypeSearchGroupId = entity.SightingTypeSearchGroupId,
                PublicCollection = entity.OrganizationCollectorId.HasValue && _organizations.ContainsKey(entity.OrganizationCollectorId.Value)
                    ? _organizations[entity.OrganizationCollectorId.Value]
                    : null,
                PrivateCollection = entity.UserCollectorId.HasValue && _personByUserId.ContainsKey(entity.UserCollectorId.Value)
                    ? _personByUserId[entity.UserCollectorId.Value].FullName
                    : null,
                DeterminedBy = entity.DeterminerUserId.HasValue && _personByUserId.ContainsKey(entity.DeterminerUserId.Value) ? _personByUserId[entity.DeterminerUserId.Value].FullName : null,
                DeterminationYear = entity.DeterminationYear,
                ConfirmedBy = entity.ConfirmatorUserId.HasValue && _personByUserId.ContainsKey(entity.ConfirmatorUserId.Value) ? _personByUserId[entity.ConfirmatorUserId.Value].FullName : null,
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

        #region Metadata
        /// <summary>
        ///     Cast multiple sightings entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Metadata> CastMetdataEntityToVerbatim(IEnumerable<MetadataEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            var metadataItems = new Dictionary<int, Metadata>();
            foreach (var entity in entities)
            {
                if (!metadataItems.ContainsKey(entity.Id))
                {
                    metadataItems.Add(entity.Id, new Metadata(entity.Id));
                }

                metadataItems[entity.Id].Translations.Add(new MetadataTranslation
                { Culture = entity.CultureCode, Value = entity.Translation });
            }

            return metadataItems.Values;
        }

        /// <summary>
        ///     Cast multiple sightings entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<MetadataWithCategory> CastMetdataWithCategoryEntityToVerbatim(
            IEnumerable<MetadataWithCategoryEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            var metadataItems = new Dictionary<int, MetadataWithCategory>();
            foreach (var entity in entities)
            {
                if (!metadataItems.ContainsKey(entity.Id))
                {
                    metadataItems.Add(entity.Id, new MetadataWithCategory(entity.Id, entity.CategoryId));
                }

                var metadata = metadataItems[entity.Id];
                metadata.Translations.Add(new MetadataTranslation
                {
                    Culture = entity.CultureCode,
                    Value = entity.Translation
                });

                metadata.Category.Translations.Add(new MetadataTranslation
                {
                    Culture = entity.CultureCode,
                    Value = entity.CategoryName
                });
            }

            return metadataItems.Values;
        }
        #endregion Metadata

        #region Organization
        /// <summary>
        ///     Cast multiple projects to aggregates
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Organization> CastOrganizationEntityToVerbatim(IEnumerable<OrganizationEntity> entities)
        {
            return from o in entities
                   select new Organization
                   {
                       Id = o.Id,
                       Name = o.Name,
                       OrganizationId = o.OrganizationId
                   };
        }
        #endregion Organization

        #region Person
        private IEnumerable<Person> CastPersonEntityToVerbatim(IEnumerable<PersonEntity> entities)
        {
            return from e in entities
                   select new Person
                   {
                       Id = e.Id,
                       UserId = e.UserId,
                       FirstName = e.FirstName,
                       LastName = e.LastName,
                       Alias = e.Alias
                   };
        }
        #endregion Person

        #region Project
        /// <summary>
        ///     Cast multiple projects entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Project> CastProjectEntitiesToVerbatim(IEnumerable<ProjectEntity> entities)
        {
            return from p in entities
                   select CastProjectEntityToVerbatim(p);
        }

        /// <summary>
        ///     Cast single project entity to verbatim
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Project CastProjectEntityToVerbatim(ProjectEntity entity)
        {
            return new Project
            {
                Category = entity.Category,
                Description = entity.Description,
                EndDate = entity.EndDate,
                Id = entity.Id,
                IsPublic = entity.IsPublic,
                Name = entity.Name,
                Owner = entity.Owner,
                StartDate = entity.StartDate,
                SurveyMethod = entity.SurveyMethod,
                SurveyMethodUrl = entity.SurveyMethodUrl
            };
        }

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

        /// <summary>
        /// Create sighting projects dictionary
        /// </summary>
        /// <param name="projectEntities"></param>
        /// <param name="sightingProjectIds"></param>
        /// <param name="projectParameterEntities"></param>
        /// <returns></returns>
        private IDictionary<int, Project[]> GetSightingProjects(IEnumerable<ProjectEntity> projectEntities, IReadOnlyList<(int SightingId, int ProjectId)> sightingProjectIds, IEnumerable<ProjectParameterEntity> projectParameterEntities)
        {
            if ((!projectEntities?.Any() ?? true) || (!sightingProjectIds?.Any() ?? true))
            {
                return null;
            }
            
              // Cast a projects to verbatim
            var projects = CastProjectEntitiesToVerbatim(projectEntities).ToDictionary(p => p.Id, p => p);
            var sightingsProjects = new Dictionary<int, IDictionary<int, Project>>();

            for (var i = 0; i < sightingProjectIds.Count; i++)
            {
                var (sightingId,  projectId) = sightingProjectIds[i];
                
                if (!projects.TryGetValue(projectId, out var project))
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
                    if (!projects.ContainsKey(projectParameterEntity.ProjectId))
                    {
                        continue;
                    }

                    // Get project from all projects
                    project = projects[projectParameterEntity.ProjectId].Clone();
                    sightingProjects.Add(project.Id, project);
                }

                if (project.ProjectParameters == null)
                {
                    project.ProjectParameters = new List<ProjectParameter>();
                }

                project.ProjectParameters.Add(CastProjectParameterEntityToVerbatim(projectParameterEntity));
            }

            return sightingsProjects.ToDictionary(sp => sp.Key, sp => sp.Value.Values.ToArray());
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

            while (idBatch.Any())
            {
                var sites = CastSiteEntitiesToVerbatim((await _siteRepository.GetByIdsAsync(idBatch, IncrementalMode))?.ToList());

                if (sites?.Any() ?? false)
                {
                    foreach (var site in sites)
                    {
                        _sites.TryAdd(site.Id, site);
                    }

                    startIndex += batchSize;
                    idBatch = newSiteIds.Skip(startIndex).Take(batchSize).ToArray();
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
            if (!siteEntities?.Any() ?? true)
            {
                return new List<Site>();
            }

            var sites = new List<Site>();
      
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
                County = entity.CountyId.HasValue
                    ? new GeographicalArea { Id = entity.CountyId.Value, Name = entity.CountyName }
                    : null,
                CountryPart = entity.CountryPartId.HasValue
                    ? new GeographicalArea { Id = entity.CountryPartId.Value, Name = entity.CountryPartName }
                    : null,
                ExternalId = entity.ExternalId,
                Id = entity.Id,
                Municipality = entity.MunicipalityId.HasValue
                    ? new GeographicalArea { Id = entity.MunicipalityId.Value, Name = entity.MunicipalityName }
                    : null,
                Province = entity.ProvinceId.HasValue
                    ? new GeographicalArea { Id = entity.ProvinceId.Value, Name = entity.ProvinceName }
                    : null,
                Parish = entity.ParishId.HasValue
                    ? new GeographicalArea { Id = entity.ParishId.Value, Name = entity.ParishName }
                    : null,
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
        #endregion SpeciesCollections

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteRepository"></param>
        /// <param name="sightingRelationRepository"></param>
        /// <param name="activities"></param>
        /// <param name="biotopes"></param>
        /// <param name="determinationMethods"></param>
        /// <param name="discoveryMethods"></param>
        /// <param name="genders"></param>
        /// <param name="organizations"></param>
        /// <param name="stages"></param>
        /// <param name="substrates"></param>
        /// <param name="validationStatus"></param>
        /// <param name="units"></param>
        /// <param name="organizationById"></param>
        /// <param name="personByUserId"></param>
        /// <param name="projectEntities"></param>
        /// <param name="projectParameterEntities"></param>
        /// <param name="sightingProjectIds"></param>
        /// <param name="speciesCollections"></param>
        public ArtportalenHarvestFactory(
            ISiteRepository siteRepository,
            ISightingRelationRepository sightingRelationRepository,
            IEnumerable<MetadataWithCategoryEntity> activities,
            IEnumerable<MetadataEntity> biotopes,
            IEnumerable<MetadataEntity> determinationMethods,
            IEnumerable<MetadataEntity> discoveryMethods,
            IEnumerable<MetadataEntity> genders,
            IEnumerable<MetadataEntity> organizations,
            IEnumerable<MetadataEntity> stages,
            IEnumerable<MetadataEntity> substrates,
            IEnumerable<MetadataEntity> validationStatus,
            IEnumerable<MetadataEntity> units,
            IEnumerable<OrganizationEntity> organizationById,
            IEnumerable<PersonEntity> personByUserId,
            IEnumerable<ProjectEntity> projectEntities,
            IEnumerable<ProjectParameterEntity> projectParameterEntities,
            IEnumerable<(int SightingId, int ProjectId)> sightingProjectIds,
            IEnumerable<SpeciesCollectionItemEntity> speciesCollections)
        {
            _siteRepository = siteRepository;
            _sightingRelationRepository = sightingRelationRepository;
            _activities = CastMetdataWithCategoryEntityToVerbatim(activities)?.ToDictionary(a => a.Id, a => a);
            _biotopes = CastMetdataEntityToVerbatim(biotopes)?.ToDictionary(b => b.Id, b => b);
            _determinationMethods = CastMetdataEntityToVerbatim(determinationMethods)?.ToDictionary(dm => dm.Id, dm => dm);
            _discoveryMethods = CastMetdataEntityToVerbatim(discoveryMethods)?.ToDictionary(dm => dm.Id, dm => dm);
            _genders = CastMetdataEntityToVerbatim(genders)?.ToDictionary(g => g.Id, g => g);
            _organizations = CastMetdataEntityToVerbatim(organizations)?.ToDictionary(o => o.Id, o => o);
            _stages = CastMetdataEntityToVerbatim(stages)?.ToDictionary(s => s.Id, s => s);
            _substrates = CastMetdataEntityToVerbatim(substrates)?.ToDictionary(s => s.Id, s => s);
            _validationStatus = CastMetdataEntityToVerbatim(validationStatus)?.ToDictionary(vs => vs.Id, vs => vs);
            _units = CastMetdataEntityToVerbatim(units)?.ToDictionary(u => u.Id, u => u);

            _organizationById = CastOrganizationEntityToVerbatim(organizationById)?.ToDictionary(o => o.Id, o => o);
            _personByUserId = CastPersonEntityToVerbatim(personByUserId)?.ToDictionary(p => p.Id, p => p);
            _sightingsProjects = GetSightingProjects(projectEntities, sightingProjectIds.ToList(), projectParameterEntities) ?? new Dictionary<int, Project[]>();
            _speciesCollections = CastSpeciesCollectionsToVerbatim(speciesCollections).ToList();

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

            // Get Observers, ReportedBy, SpeciesCollection & VerifiedBy
            var sightingRelations =
                CastSightingRelationsToVerbatim(await _sightingRelationRepository.GetAsync(sightingIds)).ToArray();

            var personSightings = PersonSightingFactory.CreatePersonSightingDictionary(
                sightingIds,
                _personByUserId,
                _organizationById,
                _speciesCollections,
                sightingRelations);

            return from e in entities
                select CastEntityToVerbatim(e, personSightings);
        }
    }
}
