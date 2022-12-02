using System.Collections.Concurrent;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Containers
{
    public class ArtportalenMetadataContainer: IArtportalenMetadataContainer
    {
        private readonly IMetadataRepository _metadataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaxonRepository _taxonRepository;
        private readonly ILogger<ArtportalenMetadataContainer> _logger;

        private bool _initilazionInProgress;

        private ConcurrentDictionary<int, MetadataWithCategory> _activities;
        private ConcurrentDictionary<int, Metadata> _biotopes;
        private ConcurrentDictionary<int, Metadata> _determinationMethods;
        private ConcurrentDictionary<int, Metadata> _discoveryMethods;
        private ConcurrentDictionary<int, Metadata> _genders;
        private ConcurrentDictionary<int, Metadata> _organizations;
        private ConcurrentDictionary<int, Person> _personsByUserId;
        private ConcurrentDictionary<int, Project> _projects;
        private ConcurrentDictionary<int, Metadata> _stages;
        private ConcurrentDictionary<int, Metadata> _substrates;
        private ConcurrentDictionary<int, int?> _taxonSpeciesGroups;
        private ConcurrentDictionary<int, Metadata> _units;
        private ConcurrentDictionary<int, Metadata> _validationStatus;

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
                return null!;
            }

            return from e in entities
                   select CastPersonEntityToVerbatim(e);
        }

        private Person CastPersonEntityToVerbatim(PersonEntity entitiy)
        {
            if (entitiy == null)
            {
                return null!;
            }

            return new Person
                   {
                       Id = entitiy.Id,
                       UserId = entitiy.UserId,
                       FirstName = entitiy.FirstName,
                       LastName = entitiy.LastName,
                       Alias = entitiy.Alias,
                       UserServiceUserId = entitiy.UserServiceUserId
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
            if (!entities?.Any() ?? true)
            {
                return null!;
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
                return null!;
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
        /// <param name="metadataRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="taxonRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenMetadataContainer(
            IMetadataRepository metadataRepository,
            IPersonRepository personRepository,
            IProjectRepository projectRepository,
            ITaxonRepository taxonRepository,
            ILogger<ArtportalenMetadataContainer> logger)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _taxonRepository = taxonRepository ?? throw new ArgumentNullException(nameof(taxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            IsInitialized = false;
        }

        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            try
            {
                // If init i s started, wait for it to finish 
                if (_initilazionInProgress)
                {
                    while (_initilazionInProgress)
                    {
                        await Task.Delay(100);
                    }
                    return;
                }
                
                _initilazionInProgress = true;

                _logger.LogDebug("Start getting meta data");

                var activitiesTask = _metadataRepository.GetActivitiesAsync();
                var metaDataTasks = new[]
                {
                _metadataRepository.GetBiotopesAsync(),
                _metadataRepository.GetDeterminationMethodsAsync(),
                _metadataRepository.GetDiscoveryMethodsAsync(),
                _metadataRepository.GetGendersAsync(),
                _metadataRepository.GetOrganizationsAsync(),
                _metadataRepository.GetStagesAsync(),
                _metadataRepository.GetSubstratesAsync(),
                _metadataRepository.GetUnitsAsync(),
                _metadataRepository.GetValidationStatusAsync()
            };
                var personsTask = _personRepository.GetAsync();
                var projectsTask = _projectRepository.GetProjectsAsync();
                var taxaTask = _taxonRepository.GetAsync();

                await activitiesTask;
                var activities = activitiesTask.Result;

                await Task.WhenAll(metaDataTasks);
                var biotopes = metaDataTasks[0].Result;
                var determinationMethods = metaDataTasks[1].Result;
                var discoveryMethods = metaDataTasks[2].Result;
                var genders = metaDataTasks[3].Result;
                var organizations = metaDataTasks[4].Result;
                var stages = metaDataTasks[5].Result;
                var substrates = metaDataTasks[6].Result;
                var units = metaDataTasks[7].Result;
                var validationStatus = metaDataTasks[8].Result;

                await personsTask;
                var persons = personsTask.Result;

                await projectsTask;
                var projects = projectsTask.Result;

                await taxaTask;
                var taxa = taxaTask.Result;

                _activities = CastMetdataWithCategoryEntityToVerbatim(activities)?.ToConcurrentDictionary(a => a.Id, a => a) ?? new ConcurrentDictionary<int, MetadataWithCategory>();
                _biotopes = CastMetdataEntitiesToVerbatims(biotopes)?.ToConcurrentDictionary(b => b.Id, b => b) ?? new ConcurrentDictionary<int, Metadata>();
                _determinationMethods = CastMetdataEntitiesToVerbatims(determinationMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata>();
                _discoveryMethods = CastMetdataEntitiesToVerbatims(discoveryMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata>();
                _genders = CastMetdataEntitiesToVerbatims(genders)?.ToConcurrentDictionary(g => g.Id, g => g) ?? new ConcurrentDictionary<int, Metadata>();
                _organizations = CastMetdataEntitiesToVerbatims(organizations)?.ToConcurrentDictionary(o => o.Id, o => o) ?? new ConcurrentDictionary<int, Metadata>();
                _personsByUserId = CastPersonEntitiesToVerbatims(persons)?.ToConcurrentDictionary(p => p.UserId, p => p) ?? new ConcurrentDictionary<int, Person>();
                _projects = CastProjectEntitiesToVerbatim(projects).ToConcurrentDictionary(p => p.Id, p => p) ?? new ConcurrentDictionary<int, Project>();
                _stages = CastMetdataEntitiesToVerbatims(stages)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata>();
                _substrates = CastMetdataEntitiesToVerbatims(substrates)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata>();
                _taxonSpeciesGroups = taxa.ToConcurrentDictionary(t => t.Id, t => t.SpeciesGroupId) ?? new ConcurrentDictionary<int, int?>();
                _units = CastMetdataEntitiesToVerbatims(units)?.ToConcurrentDictionary(u => u.Id, u => u) ?? new ConcurrentDictionary<int, Metadata>();
                _validationStatus = CastMetdataEntitiesToVerbatims(validationStatus)?.ToConcurrentDictionary(vs => vs.Id, vs => vs) ?? new ConcurrentDictionary<int, Metadata>();

                IsInitialized = true;

                _logger.LogDebug("Finish getting meta data");
            }
            finally
            {
                _initilazionInProgress = false;
            }
        }

        public bool Live {
            set {
                _metadataRepository.Live = value;
                _personRepository.Live = value;
                _projectRepository.Live = value;
                _taxonRepository.Live = value;
            }
        }

        /// <inheritdoc />
        public IDictionary<int, Metadata> Organizations => _organizations;

        /// <inheritdoc />
        public IDictionary<int, Person> PersonsByUserId => _personsByUserId;

        /// <inheritdoc />
        public MetadataWithCategory TryGetActivity(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _activities.TryGetValue(id ?? 0, out var activity);

            return activity!;
        }

        /// <inheritdoc />
        public Metadata TryGetBiotope(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _biotopes.TryGetValue(id ?? 0, out var biotope);

            return biotope!;
        }

    /// <inheritdoc />
        public Metadata TryGetDeterminationMethod(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _determinationMethods.TryGetValue(id ?? 0, out var determinationMethod);

            return determinationMethod!;
        }

        /// <inheritdoc />
        public Metadata TryGetDiscoveryMethod(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _discoveryMethods.TryGetValue(id ?? 0, out var discoveryMethod);

            return discoveryMethod!;
        }

        /// <inheritdoc />
        public Metadata TryGetGender(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _genders.TryGetValue(id ?? 0, out var gender);

            return gender!;
        }

        /// <inheritdoc />
        public Metadata TryGetOrganization(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _organizations.TryGetValue(id ?? 0, out var organization);

            return organization!;
        }

        /// <inheritdoc />
        public async Task<Person> TryGetPersonByUserIdAsync(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            if(!_personsByUserId.TryGetValue(id ?? 0, out var person)) {
                var personEntity = await _personRepository.GetAsync(id ?? 0);
                if (personEntity != null)
                {
                    person = CastPersonEntityToVerbatim(personEntity);

                    _personsByUserId.TryAdd(person.Id, person);
                }
                
            }

            return person!;
        }

        /// <inheritdoc />
        public async Task<Project> TryGetProjectAsync(int? id)
        {
            if (id == null)
            {
                return null!;
            }

            if (!_projects.TryGetValue(id ?? 0, out var project))
            {
                var projectEntity = await _projectRepository.GetProjectAsync(id ?? 0);
                if (projectEntity != null)
                {
                    project = CastProjectEntityToVerbatim(projectEntity);

                    _projects.TryAdd(project.Id, project);
                }
            }

            return project!;
        }

        /// <inheritdoc />
        public Metadata TryGetStage(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _stages.TryGetValue(id ?? 0, out var stage);

            return stage!;
        }

        /// <inheritdoc />
        public Metadata TryGetSubstrate(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _substrates.TryGetValue(id ?? 0, out var substrate);

            return substrate!;
        }

        /// <inheritdoc />
        public int? TryGetTaxonSpeciesGroupId(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _taxonSpeciesGroups.TryGetValue(id ?? 0, out var taxonSpeciesGroupId);

            return taxonSpeciesGroupId!;
        }

        /// <inheritdoc />
        public Metadata TryGetUnit(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _units.TryGetValue(id ?? 0, out var unit);

            return unit!;
        }

        /// <inheritdoc />
        public Metadata TryGetValidationStatus(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _validationStatus.TryGetValue(id ?? 0, out var validationStatus);

            return validationStatus!;
        }
    }
}