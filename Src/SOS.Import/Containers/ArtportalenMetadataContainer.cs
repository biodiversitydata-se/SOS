using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SOS.Import.Containers.Interfaces;
using SOS.Import.Entities.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Containers
{
    public class ArtportalenMetadataContainer: IArtportalenMetadataContainer
    {
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
            Projects.TryAdd(project.Id, project);
        }
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
                CategorySwedish = entity.CategorySwedish,
                Description = entity.Description,
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
        public ConcurrentDictionary<int, Organization> OrganizationById { get; private set; }
        public ConcurrentDictionary<int, Metadata> Organizations { get; private set; }
        public ConcurrentDictionary<int, Person> PersonByUserId { get; private set; }
        public ConcurrentDictionary<int, Project> Projects { get; private set; }        
        public ConcurrentDictionary<int, Metadata> Stages { get; private set; }
        public ConcurrentDictionary<int, Metadata> Substrates { get; private set; }
        public ConcurrentDictionary<int, int?> TaxonSpeciesGroups { get; private set; }
        public ConcurrentDictionary<int, Metadata> Units { get; private set; }
        public ConcurrentDictionary<int, Metadata> ValidationStatus { get; private set; }

        /// <inheritdoc />
        public void Initialize(
            IEnumerable<MetadataWithCategoryEntity> activities,
            IEnumerable<MetadataEntity> biotopes,
            IEnumerable<MetadataEntity> determinationMethods,
            IEnumerable<MetadataEntity> discoveryMethods,
            IEnumerable<MetadataEntity> genders,
            IEnumerable<OrganizationEntity> organizationById,
            IEnumerable<MetadataEntity> organizations,
            IEnumerable<PersonEntity> personByUserId,
            IEnumerable<ProjectEntity> projectEntities,
            IEnumerable<MetadataEntity> stages,
            IEnumerable<MetadataEntity> substrates,
            IEnumerable<TaxonEntity> taxa,
            IEnumerable<MetadataEntity> units,
            IEnumerable<MetadataEntity> validationStatus
        )
        {
            Activities = CastMetdataWithCategoryEntityToVerbatim(activities)?.ToConcurrentDictionary(a => a.Id, a => a);
            Biotopes = CastMetdataEntityToVerbatim(biotopes)?.ToConcurrentDictionary(b => b.Id, b => b);
            DeterminationMethods = CastMetdataEntityToVerbatim(determinationMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm);
            DiscoveryMethods = CastMetdataEntityToVerbatim(discoveryMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm);
            Genders = CastMetdataEntityToVerbatim(genders)?.ToConcurrentDictionary(g => g.Id, g => g);
            OrganizationById = CastOrganizationEntityToVerbatim(organizationById)?.ToConcurrentDictionary(o => o.Id, o => o);
            Organizations = CastMetdataEntityToVerbatim(organizations)?.ToConcurrentDictionary(o => o.Id, o => o);
            PersonByUserId = CastPersonEntityToVerbatim(personByUserId)?.ToConcurrentDictionary(p => p.UserId, p => p);
            Projects = CastProjectEntitiesToVerbatim(projectEntities).ToConcurrentDictionary(p => p.Id, p => p);
            Stages = CastMetdataEntityToVerbatim(stages)?.ToConcurrentDictionary(s => s.Id, s => s);
            Substrates = CastMetdataEntityToVerbatim(substrates)?.ToConcurrentDictionary(s => s.Id, s => s);
            TaxonSpeciesGroups = taxa.ToConcurrentDictionary(t => t.Id, t => t.SpeciesGroupId);
            Units = CastMetdataEntityToVerbatim(units)?.ToConcurrentDictionary(u => u.Id, u => u);
            ValidationStatus = CastMetdataEntityToVerbatim(validationStatus)?.ToConcurrentDictionary(vs => vs.Id, vs => vs);

            IsInitialized = true;
        }
    }
}