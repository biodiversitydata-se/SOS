using System.Collections.Concurrent;
using System.Text.RegularExpressions;
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
    public class ArtportalenMetadataContainer : IArtportalenMetadataContainer
    {
        private readonly IDiaryEntryRepository _diaryEntryRepository;
        private readonly IMetadataRepository _metadataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaxonRepository _taxonRepository;
        private readonly ILogger<ArtportalenMetadataContainer> _logger;

        private bool _initilazionInProgress;

        private ConcurrentDictionary<int, MetadataWithCategory<int>> _activities;
        private ConcurrentDictionary<int, Metadata<int>> _biotopes;
        private ConcurrentDictionary<int, Metadata<int>> _determinationMethods;
        private ConcurrentDictionary<(DateTime, int, int), ICollection<DiaryEntry>> _diaryEntries;
        private ConcurrentDictionary<int, Metadata<int>> _discoveryMethods;
        private ConcurrentDictionary<int, Metadata<int>> _genders;
        private ConcurrentDictionary<int, Metadata<int>> _organizations;
        private ConcurrentDictionary<int, Person> _personsByUserId;
        private ConcurrentDictionary<int, Project> _projects;
        private ConcurrentDictionary<string, Metadata<string>> _sharedLabels;
        private ConcurrentDictionary<int, Metadata<int>> _stages;
        private ConcurrentDictionary<int, Metadata<int>> _substrates;
        private ConcurrentDictionary<int, int?> _taxonSpeciesGroups;
        private ConcurrentDictionary<int, Metadata<int>> _units;
        private ConcurrentDictionary<int, Metadata<int>> _validationStatus;

        #region Diary Entries
        private async Task PopulateWeatherAsync() 
        {
            _diaryEntries = new ConcurrentDictionary<(DateTime, int, int), ICollection<DiaryEntry>>();
            var diaryEntries = await _diaryEntryRepository.GetAsync();

            foreach(var diaryEntry in diaryEntries)
            {
                if (!_diaryEntries.TryGetValue((diaryEntry.IssueDate, diaryEntry.ProjectId, diaryEntry.UserId), out var dateDiaryEntries))
                {
                    dateDiaryEntries = new List<DiaryEntry>();
                    _diaryEntries.TryAdd((diaryEntry.IssueDate.Date, diaryEntry.ProjectId, diaryEntry.UserId), dateDiaryEntries);
                }

                dateDiaryEntries.Add(new DiaryEntry
                {
                    CloudinessId = diaryEntry.CloudinessId,
                    ControlingOrganisationId = diaryEntry.ControlingOrganisationId,
                    EndTime = diaryEntry.EndTime,
                    IssueDate = diaryEntry.IssueDate,
                    OrganizationId = diaryEntry.OrganizationId,
                    PrecipitationId = diaryEntry.PrecipitationId,
                    ProjectId = diaryEntry.ProjectId,
                    SiteId = diaryEntry.SiteId,
                    SnowcoverId = diaryEntry.SnowcoverId,
                    StartTime = diaryEntry.StartTime,
                    Temperature = diaryEntry.Temperature,
                    UserId = diaryEntry.UserId,
                    VisibilityId = diaryEntry.VisibilityId,
                    WindId = diaryEntry.WindId,
                    WindStrengthId = diaryEntry.WindStrengthId
                });
            }
        }
        #endregion Diary Entries

        #region Metadata
        /// <summary>
        ///     Cast multiple sightings entities to models
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IEnumerable<Metadata<T>> CastMetdataEntitiesToVerbatims<T>(IEnumerable<MetadataEntity<T>> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null!;
            }

            var metadataItems = new Dictionary<T, Metadata<T>>();
            foreach (var entity in entities)
            {
                if (!metadataItems.ContainsKey(entity.Id))
                {
                    metadataItems.Add(entity.Id, new Metadata<T>(entity.Id));
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
        private IEnumerable<MetadataWithCategory<T>> CastMetdataWithCategoryEntityToVerbatim<T>(
            IEnumerable<MetadataWithCategoryEntity<T>> entities)
        {
            if (!entities?.Any() ?? true)
            {
                return null!;
            }

            var metadataItems = new Dictionary<T, MetadataWithCategory<T>>();
            foreach (var entity in entities)
            {
                if (!metadataItems.ContainsKey(entity.Id))
                {
                    metadataItems.Add(entity.Id, new MetadataWithCategory<T>(entity.Id, entity.CategoryId));
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
        /// <param name="diaryEntryRepository"></param>
        /// <param name="metadataRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="projectRepository"></param>
        /// <param name="taxonRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenMetadataContainer(
            IDiaryEntryRepository diaryEntryRepository,
            IMetadataRepository metadataRepository,
            IPersonRepository personRepository,
            IProjectRepository projectRepository,
            ITaxonRepository taxonRepository,
            ILogger<ArtportalenMetadataContainer> logger)
        {
            _diaryEntryRepository = diaryEntryRepository ?? throw new ArgumentNullException(nameof(diaryEntryRepository));
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
                var sharedLabelsTask = _metadataRepository.GetResourcesAsync("Shared_");

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

                var activities = await activitiesTask;
                var persons = await personsTask;
                var projects = await projectsTask;
                var taxa = await taxaTask;
                var sharedLabels = await sharedLabelsTask;

                _activities = CastMetdataWithCategoryEntityToVerbatim(activities)?.ToConcurrentDictionary(a => a.Id, a => a) ?? new ConcurrentDictionary<int, MetadataWithCategory<int>>();
                _biotopes = CastMetdataEntitiesToVerbatims(biotopes)?.ToConcurrentDictionary(b => b.Id, b => b) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _determinationMethods = CastMetdataEntitiesToVerbatims(determinationMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _discoveryMethods = CastMetdataEntitiesToVerbatims(discoveryMethods)?.ToConcurrentDictionary(dm => dm.Id, dm => dm) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _genders = CastMetdataEntitiesToVerbatims(genders)?.ToConcurrentDictionary(g => g.Id, g => g) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _organizations = CastMetdataEntitiesToVerbatims(organizations)?.ToConcurrentDictionary(o => o.Id, o => o) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _personsByUserId = CastPersonEntitiesToVerbatims(persons)?.ToConcurrentDictionary(p => p.UserId, p => p) ?? new ConcurrentDictionary<int, Person>();
                _projects = CastProjectEntitiesToVerbatim(projects).ToConcurrentDictionary(p => p.Id, p => p) ?? new ConcurrentDictionary<int, Project>();
                _sharedLabels = CastMetdataEntitiesToVerbatims(sharedLabels)?.ToConcurrentDictionary(g => g.Id, g => g) ?? new ConcurrentDictionary<string, Metadata<string>>();
                _stages = CastMetdataEntitiesToVerbatims(stages)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _substrates = CastMetdataEntitiesToVerbatims(substrates)?.ToConcurrentDictionary(s => s.Id, s => s) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _taxonSpeciesGroups = taxa.ToConcurrentDictionary(t => t.Id, t => t.SpeciesGroupId) ?? new ConcurrentDictionary<int, int?>();
                _units = CastMetdataEntitiesToVerbatims(units)?.ToConcurrentDictionary(u => u.Id, u => u) ?? new ConcurrentDictionary<int, Metadata<int>>();
                _validationStatus = CastMetdataEntitiesToVerbatims<int>(validationStatus)?.ToConcurrentDictionary(vs => vs.Id, vs => vs) ?? new ConcurrentDictionary<int, Metadata<int>>();

                await PopulateWeatherAsync();

                IsInitialized = true;

                _logger.LogDebug("Finish getting meta data");
            }
            finally
            {
                _initilazionInProgress = false;
            }
        }

        public bool Live
        {
            set
            {
                _metadataRepository.Live = value;
                _personRepository.Live = value;
                _projectRepository.Live = value;
                _taxonRepository.Live = value;
            }
        }

        /// <inheritdoc />
        public IDictionary<int, Metadata<int>> Organizations => _organizations;

        /// <inheritdoc />
        public IDictionary<int, Person> PersonsByUserId => _personsByUserId;

        /// <inheritdoc />
        public MetadataWithCategory<int> TryGetActivity(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _activities.TryGetValue(id ?? 0, out var activity);

            return activity!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetBiotope(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _biotopes.TryGetValue(id ?? 0, out var biotope);

            return biotope!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetDeterminationMethod(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _determinationMethods.TryGetValue(id ?? 0, out var determinationMethod);

            return determinationMethod!;
        }

        /// <inheritdoc />
        public DiaryEntry TryGetDiaryEntry(IEnumerable<int> projectIds, DateTime? startDate, TimeSpan? startTime, int userId, int? siteId, int? controlingOrganisationId)
        {
            if (!projectIds?.Any() ?? true || !startDate.HasValue)
            {
                return null!;
            }

            DiaryEntry diaryEntry = null!;
            foreach (var projectId in projectIds!)
            {
                if (_diaryEntries.TryGetValue((startDate!.Value, projectId, userId), out var dateDiaryEntries))
                {
                    var matchLevel = -1;
                    foreach (var dateDiaryEntry in dateDiaryEntries)
                    {
                        // We have found weather data for project/user the requested date. Set weather to make sure we have something to return if we not find any better option
                        if (matchLevel.Equals(-1))
                        {
                            diaryEntry = dateDiaryEntry;
                            matchLevel = 0;
                        }

                        var matchCount = (startTime >= dateDiaryEntry.StartTime ? 1 : 0) +
                            (startTime <= dateDiaryEntry.EndTime ? 1 : 0) +
                            (siteId == dateDiaryEntry.SiteId ? 1 : 0) +
                            (controlingOrganisationId == dateDiaryEntry.ControlingOrganisationId ? 1 : 0);

                        if (matchCount.Equals(4) || dateDiaryEntries.Count().Equals(1))
                        {
                            return dateDiaryEntry; // Perfect match no reason to go on
                        }

                        if (matchCount > matchLevel)
                        {
                            diaryEntry = dateDiaryEntry; // We found a "better" match, use it 
                            matchLevel = matchCount;
                        }
                    }
                }
            }

            return diaryEntry!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetDiscoveryMethod(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _discoveryMethods.TryGetValue(id ?? 0, out var discoveryMethod);

            return discoveryMethod!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetGender(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _genders.TryGetValue(id ?? 0, out var gender);

            return gender!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetOrganization(int? id)
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
            if (!_personsByUserId.TryGetValue(id ?? 0, out var person))
            {
                var personEntity = await _personRepository.GetByUserIdAsync(id ?? 0);
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
        public Metadata<int> TryGetStage(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _stages.TryGetValue(id ?? 0, out var stage);

            return stage!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetSubstrate(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _substrates.TryGetValue(id ?? 0, out var substrate);

            return substrate!;
        }

        public string TryGetSummary(string source, bool FreeTextSummary)
        {
            if (FreeTextSummary || string.IsNullOrEmpty(source))
            {
                return source!;
            }

            var regex = new Regex("\\[[^\\]]*\\]\\]");
            var matches = regex.Matches(source);

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Value;

                if (_sharedLabels.TryGetValue(match.Replace("[", "").Replace("]", ""), out var label))
                {
                    var value = label.Translate("sv-SE");
                    source = source.Replace(match, value);
                }
            }

            return source!;
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
        public Metadata<int> TryGetUnit(int? id)
        {
            if (id == null)
            {
                return null!;
            }
            _units.TryGetValue(id ?? 0, out var unit);

            return unit!;
        }

        /// <inheritdoc />
        public Metadata<int> TryGetValidationStatus(int? id)
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