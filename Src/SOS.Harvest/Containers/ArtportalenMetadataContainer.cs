using System.Collections.Concurrent;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Entities.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Containers
{
    public class ArtportalenMetadataContainer: IArtportalenMetadataContainer
    {
        #region Metadata
        /// <summary>
        ///     Cast multiple sightings entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Metadata> CastMetdataEntitiesToVerbatims(IEnumerable<MetadataEntity> entities)
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
        ///     Cast multiple organizations to verbatims
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Organization> CastOrganizationEntitiesToVerbatims(IEnumerable<OrganizationEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

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
        private IEnumerable<Person> CastPersonEntitiesToVerbatims(IEnumerable<PersonEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

            return from e in entities
                   select new Person
                   {
                       Id = e.Id,
                       UserId = e.UserId,
                       FirstName = e.FirstName,
                       LastName = e.LastName,
                       Alias = e.Alias,
                       UserServiceUserId = e.UserServiceUserId
                   };
        }
        #endregion Person

        #region Project
        /// <summary>
        /// Add a project
        /// </summary>
        /// <param name="entities"></param>
        public void AddProject(ProjectEntity entity)
        {
            var project = CastProjectEntityToVerbatim(entity);

            if (project == null)
            {
                return;
            }
            Projects.TryAdd(project.Id, project);
        }
        /// <summary>
        ///     Cast multiple projects entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Project> CastProjectEntitiesToVerbatim(IEnumerable<ProjectEntity> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null;
            }

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
            if (entity == null)
            {
                return null;
            }

            return new Project
            {
                Category = entity.Category,
                CategorySwedish = entity.CategorySwedish,
                Description = entity.Description?.Clean(),
                EndDate = entity.EndDate,
                Id = entity.Id,
                IsPublic = entity.IsPublic,
                Name = entity.Name,
                Owner = entity.Owner,
                StartDate = entity.StartDate,
                ProjectURL = entity.ProjectURL,
                SurveyMethod = entity.SurveyMethod,
                SurveyMethodUrl = entity.SurveyMethodUrl
            };
        }
        #endregion Project

        /// <summary>
        /// Constructor
        /// </summary>
        public ArtportalenMetadataContainer()
        {
            IsInitialized = false;
        }

        public bool IsInitialized { get; private set; }

        public ConcurrentDictionary<int, MetadataWithCategory> Activities { get; private set; }
        public ConcurrentDictionary<int, Metadata> Biotopes { get; private set; }
        public ConcurrentDictionary<int, Metadata> DeterminationMethods { get; private set; }
        public ConcurrentDictionary<int, Metadata> DiscoveryMethods { get; private set; }
        public ConcurrentDictionary<int, Metadata> Genders { get; private set; }
        public ConcurrentDictionary<int, Metadata> Organizations { get; private set; }
        public ConcurrentDictionary<int, Person> PersonsByUserId { get; private set; }
        public ConcurrentDictionary<int, Project> Projects { get; private set; }        
        public ConcurrentDictionary<int, Metadata> Stages { get; private set; }
        public ConcurrentDictionary<int, Metadata> Substrates { get; private set; }
        public ConcurrentDictionary<int, int?> TaxonSpeciesGroups { get; private set; }
        public ConcurrentDictionary<int, Metadata> Units { get; private set; }
        public ConcurrentDictionary<int, Metadata> ValidationStatus { get; private set; }

        /// <inheritdoc />
        public void InitializeStatic(
            IEnumerable<MetadataWithCategoryEntity> activities,
            IEnumerable<MetadataEntity> biotopes,
            IEnumerable<MetadataEntity> determinationMethods,
            IEnumerable<MetadataEntity> discoveryMethods,
            IEnumerable<MetadataEntity> genders,
            IEnumerable<MetadataEntity> organizations,
            IEnumerable<MetadataEntity> stages,
            IEnumerable<MetadataEntity> substrates,
            IEnumerable<TaxonEntity> taxa,
            IEnumerable<MetadataEntity> units,
            IEnumerable<MetadataEntity> validationStatus
        )
        {
            Activities = CastMetdataWithCategoryEntityToVerbatim(activities)?.ToConcurrentDictionary(a => a.Id, a => a) ?? new ConcurrentDictionary<int, MetadataWithCategory>();
            Biotopes = CastMetdataEntitiesToVerbatims(biotopes)?.ToConcurrentDictionary(b => b.Id, b => b) ?? new ConcurrentDictionary<int, Metadata>();
            DeterminationMethods = CastMetdataEntitiesToVerbatims(determinationMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata>();
            DiscoveryMethods = CastMetdataEntitiesToVerbatims(discoveryMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata>();
            Genders = CastMetdataEntitiesToVerbatims(genders)?.ToConcurrentDictionary(g => g.Id, g => g) ?? new ConcurrentDictionary<int, Metadata>();
            Organizations = CastMetdataEntitiesToVerbatims(organizations)?.ToConcurrentDictionary(o => o.Id, o => o) ?? new ConcurrentDictionary<int, Metadata>();
            Stages = CastMetdataEntitiesToVerbatims(stages)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata>();
            Substrates = CastMetdataEntitiesToVerbatims(substrates)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata>();
            TaxonSpeciesGroups = taxa.ToConcurrentDictionary(t => t.Id, t => t.SpeciesGroupId) ?? new ConcurrentDictionary<int, int?>();
            Units = CastMetdataEntitiesToVerbatims(units)?.ToConcurrentDictionary(u => u.Id, u => u) ?? new ConcurrentDictionary<int, Metadata>();
            ValidationStatus = CastMetdataEntitiesToVerbatims(validationStatus)?.ToConcurrentDictionary(vs => vs.Id, vs => vs) ?? new ConcurrentDictionary<int, Metadata>();

            IsInitialized = true;
        }

        /// <inheritdoc />
        public void InitializeDynamic(
            IEnumerable<PersonEntity> persons,
            IEnumerable<ProjectEntity> projectEntities
        )
        {
            PersonsByUserId = CastPersonEntitiesToVerbatims(persons)?.ToConcurrentDictionary(p => p.UserId, p => p) ?? new ConcurrentDictionary<int, Person>();
            Projects = CastProjectEntitiesToVerbatim(projectEntities).ToConcurrentDictionary(p => p.Id, p => p) ?? new ConcurrentDictionary<int, Project>();
        }
    }
}