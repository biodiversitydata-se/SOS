using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson.Serialization.Conventions;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.UserService;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.HealthChecks;
using SOS.Observations.Api.LiveIntegrationTests.Repositories;
using SOS.Observations.Api.LiveIntegrationTests.Repositories.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Api.Services.Interfaces;
using SOS.TestHelpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProviderManager = SOS.Observations.Api.Managers.DataProviderManager;
using SOS.Shared.Api.Validators;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Configuration;
using SOS.Lib.Helpers.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Lib.Models.Cache;
using SOS.Lib.Cache.Interfaces;
using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch.Cluster;

namespace SOS.Observations.Api.LiveIntegrationTests.Fixtures
{
    public class ApiIntegrationTestFixture : FixtureBase, IDisposable
    {
        public InstallationEnvironment InstallationEnvironment { get; private set; }
        public ObservationsController ObservationsController { get; private set; }
        public ObservationManager ObservationManager { get; private set; }
        public LocationsController LocationsController { get; private set; }
        public ExportsController ExportsController { get; private set; }
        public SystemsController SystemsController { get; private set; }
        public VocabulariesController VocabulariesController { get; private set; }
        public UserController UserController { get; private set; }
        public DataProvidersController DataProvidersController { get; private set; }
        public AreasController AreasController { get; private set; }
        public IProcessedObservationRepository ProcessedObservationRepository { get; set; }
        public IEventRepository EventRepository { get; set; }
        public IInvalidObservationRepository InvalidObservationRepository { get; set; }
        public IProcessedObservationRepositoryTest ProcessedObservationRepositoryTest { get; set; }
        public IProcessedObservationRepository CustomProcessedObservationRepository { get; set; }
        public ObservationsController CustomObservationsController { get; private set; }
        public DwcArchiveFileWriter DwcArchiveFileWriter { get; set; }
        public IFilterManager FilterManager;
        private IUserManager _userManager;
        public string UserAuthenticationToken { get; set; }

        public TaxonRepository TaxonRepository { get; private set; }
        public TaxonManager TaxonManager { get; private set; }

        public SearchDataProvidersHealthCheck SearchDataProvidersHealthCheck { get; set; }
        public SearchPerformanceHealthCheck SearchPerformanceHealthCheck { get; set; }
        public AzureSearchHealthCheck AzureSearchHealthCheck { get; set; }
        public SersObservationVerbatimRepository SersObservationVerbatimRepository { get; set; }

        public VocabularyValueResolver VocabularyValueResolver { get; set; }

        public ApiIntegrationTestFixture()
        {
            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
                },
                t => true);

            InstallationEnvironment = GetEnvironmentFromAppSettings();
            Initialize().Wait();
        }

        public void Dispose() { }

        public void InitControllerHttpContext()
        {
            ExportsController.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        protected string GetUserAuthenticationToken()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var userAuthenticationToken = config.GetSection($"{configPrefix}:UserAuthenticationToken").Get<string>();
            return userAuthenticationToken;
        }

        protected string GetAzureApiUrl()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var azureApiUrl = config.GetSection($"{configPrefix}:AzureApiUrl").Get<string>();
            return azureApiUrl;
        }

        protected string GetAzureApiSubscriptionKey()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var azureApiUrl = config.GetSection($"{configPrefix}:AzureApiSubscriptionKey").Get<string>();
            return azureApiUrl;
        }

        protected MongoDbConfiguration GetMongoDbConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var mongoDbConfiguration = config.GetSection($"{configPrefix}:ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }

        protected MongoDbConfiguration GetVerbatimMongoDbConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var mongoDbConfiguration = config.GetSection($"{configPrefix}:VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }

        protected ElasticSearchConfiguration GetSearchDbConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var elasticConfiguration = config.GetSection($"{configPrefix}:SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            return elasticConfiguration;
        }

        protected ElasticSearchConfiguration GetCustomSearchDbConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var elasticConfiguration = config.GetSection($"{configPrefix}:CustomSearchDbConfiguration").Get<ElasticSearchConfiguration>();
            return elasticConfiguration;
        }

        protected UserServiceConfiguration GetUserServiceConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var userServiceConfiguration = config.GetSection($"{configPrefix}:UserServiceConfiguration").Get<UserServiceConfiguration>();
            return userServiceConfiguration;
        }

        protected ObservationApiConfiguration GetObservationApiConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var observationApiConfiguration = config.GetSection($"{configPrefix}:ObservationApiConfiguration").Get<ObservationApiConfiguration>();
            return observationApiConfiguration;
        }


        protected InputValaidationConfiguration GetInputValaidationConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var inputValaidationConfiguration = config.GetSection($"{configPrefix}:InputValaidationConfiguration").Get<InputValaidationConfiguration>();
            return inputValaidationConfiguration;
        }
       
        private async Task Initialize()
        {
            UserAuthenticationToken = GetUserAuthenticationToken();
            ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
            var blobStorageManagerMock = new Mock<IBlobStorageManager>();
            var observationApiConfiguration = GetObservationApiConfiguration();
            var elasticClientManager = new ElasticClientManager(elasticConfiguration);
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
            var verbatimDbConfiguration = GetVerbatimMongoDbConfiguration();
            var importClient = new VerbatimClient(verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.ReadBatchSize, verbatimDbConfiguration.WriteBatchSize);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var areaManager = CreateAreaManager(processClient);
            TaxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
            var taxonManager = CreateTaxonManager(processClient, TaxonRepository, memoryCache);
            var processedConfigurationCache = new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new MemoryCache(new MemoryCacheOptions()), new NullLogger<ProcessedConfigurationCache>());
            var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>());
            ProcessedObservationRepository = CreateProcessedObservationRepository(elasticConfiguration, elasticClientManager, processedConfigurationCache, taxonManager, processClient, memoryCache);
            EventRepository = new EventRepository(elasticClientManager, elasticConfiguration, processedConfigurationCache, new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()), new NullLogger<EventRepository>());
            InvalidObservationRepository = new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var processedTaxonRepository = CreateProcessedTaxonRepository(elasticConfiguration, elasticClientManager, processClient, taxonManager);
            var vocabularyRepository = new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyManger = CreateVocabularyManager(processClient, vocabularyRepository);
            var projectManger = CreateProjectManager(processClient);
            var processInfoRepository = new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var processInfoManager = new ProcessInfoManager(processInfoRepository, new NullLogger<ProcessInfoManager>());
            var dataProviderCache = new DataProviderCache(new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()), new MemoryCache(new MemoryCacheOptions()), new NullLogger<DataProviderCache>());
            var dataproviderManager = new DataProviderManager(dataProviderCache, processInfoManager, ProcessedObservationRepository, new NullLogger<DataProviderManager>());
            var fileService = new FileService();
            VocabularyValueResolver = new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration { ResolveValues = true, LocalizationCultureCode = "sv-SE" });
            var generalizationResolverMock = new Mock<GeneralizationResolver>();
            var csvFileWriter = new CsvFileWriter(ProcessedObservationRepository, fileService,
                VocabularyValueResolver, generalizationResolverMock.Object, new NullLogger<CsvFileWriter>());
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(VocabularyValueResolver, processClient);
            var dwcArchiveEventFileWriter = CreateDwcArchiveEventFileWriter(VocabularyValueResolver, processClient);
            var excelFileWriter = new ExcelFileWriter(ProcessedObservationRepository,
                projectManger,
                fileService,
                VocabularyValueResolver, generalizationResolverMock.Object, new NullLogger<ExcelFileWriter>());
            var geojsonFileWriter = new GeoJsonFileWriter(ProcessedObservationRepository, fileService,
                VocabularyValueResolver, generalizationResolverMock.Object, new NullLogger<GeoJsonFileWriter>());
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<AreaCache>());
            var userService = CreateUserService();
            var filterManager = new FilterManager(taxonManager, userService, areaCache, dataProviderCache);
            FilterManager = filterManager;
            ObservationManager = CreateObservationManager((ProcessedObservationRepository)ProcessedObservationRepository, VocabularyValueResolver, processClient, filterManager);
            var taxonSearchManager = CreateTaxonSearchManager(processedTaxonRepository, filterManager);
            var inputValaidationConfiguration = GetInputValaidationConfiguration();
            var exportManager = new ExportManager(csvFileWriter, dwcArchiveFileWriter, dwcArchiveEventFileWriter, excelFileWriter, geojsonFileWriter,
                ProcessedObservationRepository, processInfoRepository, filterManager, new NullLogger<ExportManager>());
            var userExportRepository = new UserExportRepository(processClient, new NullLogger<UserExportRepository>());
            var sortableFieldsCache = new SortableFieldsCache(ProcessedObservationRepository, memoryCache);
            var inputValidator = new InputValidator(areaCache, sortableFieldsCache, taxonManager, inputValaidationConfiguration);
            var searchFilterUtility = new SearchFilterUtility(new AreaConfiguration(), areaCache);
            ObservationsController = new ObservationsController(ObservationManager, taxonSearchManager, searchFilterUtility, inputValidator, observationApiConfiguration, 
                new Mock<ClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>>().Object,
                new Mock<ClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>>().Object,
                new SemaphoreLimitManager(new SemaphoreLimitsConfiguration(), new NullLogger<SemaphoreLimitManager>()),
                new NullLogger<ObservationsController>());
            var ctx = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            ObservationsController.ControllerContext = ctx;
            VocabulariesController = new VocabulariesController(vocabularyManger, projectManger, new NullLogger<VocabulariesController>());
            DataProvidersController = new DataProvidersController(dataproviderManager,
                ObservationManager,
                processInfoManager,
                ProcessedObservationRepository,
                new NullLogger<DataProvidersController>());
            var cryptoService = new CryptoService(new CryptoConfiguration() { Password = "password", Salt = "salt" });
            var userManager = new UserManager(userService, areaCache, new NullLogger<UserManager>());
            ExportsController = new ExportsController(ObservationManager, blobStorageManagerMock.Object, exportManager, userManager, cryptoService, fileService, userExportRepository, inputValidator, observationApiConfiguration,
                new NullLogger<ExportsController>());
            ExportsController.ControllerContext.HttpContext = new DefaultHttpContext();
            TaxonManager = taxonManager;

            ProcessedObservationRepositoryTest = CreateProcessedObservationRepositoryTest(elasticConfiguration, elasticClientManager, processedConfigurationCache, clusterHealthCache, processClient);

            var customElasticConfiguration = GetCustomSearchDbConfiguration();
            var customElasticClientManager = new ElasticClientManager(customElasticConfiguration);
            //CustomProcessedObservationRepository = CreateProcessedObservationRepository(customElasticConfiguration, customElasticClientManager, processedConfigurationCache, processClient, memoryCache);


            //var customObservationManager = CreateObservationManager((ProcessedObservationRepository)CustomProcessedObservationRepository, VocabularyValueResolver, processClient, filterManager);
            //CustomObservationsController = new ObservationsController(customObservationManager, taxonSearchManager, taxonManager, areaManager, observationApiConfiguration, customElasticConfiguration, new NullLogger<ObservationsController>());
            DwcArchiveFileWriter = dwcArchiveFileWriter;
            var healthCheckConfiguration = new HealthCheckConfiguration
            {
                AzureApiUrl = GetAzureApiUrl(),
                AzureSubscriptionKey = GetAzureApiSubscriptionKey()
            };
            SearchDataProvidersHealthCheck = new SearchDataProvidersHealthCheck(ObservationManager, dataProviderCache);
            SearchPerformanceHealthCheck = new SearchPerformanceHealthCheck(ObservationManager);
            AzureSearchHealthCheck = new AzureSearchHealthCheck(healthCheckConfiguration, new NullLogger<AzureSearchHealthCheck>());
            var devOpsService = new DevOpsService(new HttpClientService(new NullLogger<HttpClientService>()), new DevOpsConfiguration(), new NullLogger<DevOpsService>());
            var devOpsManager = new DevOpsManager(devOpsService, new DevOpsConfiguration(), new NullLogger<DevOpsManager>());
            SystemsController = new SystemsController(devOpsManager, processInfoManager, ProcessedObservationRepository, new NullLogger<SystemsController>());
            _userManager = new UserManager(userService, areaCache, new NullLogger<UserManager>());
            UserController = new UserController(_userManager, new NullLogger<UserController>());
            SersObservationVerbatimRepository = new SersObservationVerbatimRepository(importClient, new NullLogger<SersObservationVerbatimRepository>());

            var processedLocationController = new ProcessedLocationRepository(elasticClientManager,
                elasticConfiguration, processedConfigurationCache,
                new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
                new NullLogger<ProcessedLocationRepository>());
            var locationManager = new LocationManager(processedLocationController, filterManager, new NullLogger<LocationManager>());
            LocationsController = new LocationsController(locationManager, inputValidator, observationApiConfiguration, new NullLogger<LocationsController>());
            AreasController = new AreasController(areaManager, new NullLogger<AreasController>());
        }

        private DwcArchiveFileWriter CreateDwcArchiveFileWriter(VocabularyValueResolver vocabularyValueResolver, ProcessClient processClient)
        {
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new FileService(), new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DwcArchiveFileWriter>());

            return dwcArchiveFileWriter;
        }

        private DwcArchiveEventFileWriter CreateDwcArchiveEventFileWriter(VocabularyValueResolver vocabularyValueResolver, ProcessClient processClient)
        {
            var dwcArchiveEventFileWriter = new DwcArchiveEventFileWriter(
                new DwcArchiveOccurrenceCsvWriter(vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new DwcArchiveEventCsvWriter(vocabularyValueResolver,
                    new NullLogger<DwcArchiveEventCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new FileService(),
                new NullLogger<DwcArchiveEventFileWriter>()); ;

            return dwcArchiveEventFileWriter;
        }

        private AreaManager CreateAreaManager(ProcessClient processClient)
        {
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<AreaCache>());
            var areaManager = new AreaManager(areaCache, new NullLogger<AreaManager>());
            return areaManager;
        }

        private TaxonManager CreateTaxonManager(ProcessClient processClient, TaxonRepository taxonRepository, IMemoryCache memoryCache)
        {
            var taxonListRepository = new TaxonListRepository(processClient, new NullLogger<TaxonListRepository>());
            var taxonManager = new TaxonManager(taxonRepository, taxonListRepository,
                new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache, new NullLogger<ClassCache<TaxonTree<IBasicTaxon>>>()),
                new ClassCache<TaxonListSetsById>(memoryCache, new NullLogger<ClassCache<TaxonListSetsById>>()),
                new NullLogger<TaxonManager>());
            return taxonManager;
        }

        private ObservationManager CreateObservationManager(
            ProcessedObservationRepository processedObservationRepository,
            VocabularyValueResolver vocabularyValueResolver,
            ProcessClient processClient,
            FilterManager filterManager)
        {
            var protectedLogRepository = new ProtectedLogRepository(processClient, new NullLogger<ProtectedLogRepository>());
            MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions { SizeLimit = null };
            var artportalenApiService = new ArtportalenApiService(
                new HttpClientService(new NullLogger<HttpClientService>()),
                new ArtportalenApiServiceConfiguration { BaseAddress = "https://api.artdata.slu.se/observations/v2", AcceptHeaderContentType = "application/json" },
                new NullLogger<ArtportalenApiService>());

            var observationsManager = new ObservationManager(processedObservationRepository,
                protectedLogRepository,
                vocabularyValueResolver,
                filterManager,
                new HttpContextAccessor(),
                new TaxonObservationCountCache(),
                new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<int, TaxonSumAggregationItem>>>()) { CacheDuration = TimeSpan.FromHours(4) },
                new Mock<IGeneralizationResolver>().Object,
                new Mock<IDataProviderCache>().Object,
                new NullLogger<ObservationManager>());

            return observationsManager;
        }


        private TaxonSearchManager CreateTaxonSearchManager(
            ProcessedTaxonRepository processedTaxonRepository,
            FilterManager filterManager)
        {
            var taxonSearchManager = new TaxonSearchManager(processedTaxonRepository,
                filterManager,
                new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<int, TaxonSumAggregationItem>>>()) { CacheDuration = TimeSpan.FromHours(4) },
                new NullLogger<TaxonSearchManager>());

            return taxonSearchManager;
        }

        protected virtual IUserService CreateUserService()
        {
            var userServiceConfiguration = GetUserServiceConfiguration();
            var userService = new UserService(new Mock<IAuthorizationProvider>().Object,
                new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
            return userService;
        }

        protected virtual IUserService CreateUserService(string token)
        {
            var userServiceConfiguration = GetUserServiceConfiguration();
            IHttpContextAccessor contextAccessor = new HttpContextAccessor();
            contextAccessor.HttpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Request.Headers.TryAdd("Authorization", token);
            IAuthorizationProvider authorizationProvider = new CurrentUserAuthorization(contextAccessor);
            var userService = new UserService(authorizationProvider,
                new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
            return userService;
        }

        private VocabularyManager CreateVocabularyManager(ProcessClient processClient, VocabularyRepository vocabularyRepository)
        {
            var vocabularyCache = new VocabularyCache(vocabularyRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<VocabularyCache>());
            var vocabularyManager = new VocabularyManager(vocabularyCache, new NullLogger<VocabularyManager>());
            return vocabularyManager;
        }

        private ProjectManager CreateProjectManager(ProcessClient processClient)
        {
            var projectInfoRepository = new ProjectInfoRepository(processClient, new NullLogger<ProjectInfoRepository>());
            var projectInfoCache = new ProjectCache(projectInfoRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<ProjectCache>());
            var projectManager = new ProjectManager(projectInfoCache, new NullLogger<ProjectManager>());
            return projectManager;
        }

        private ProcessedObservationRepository CreateProcessedObservationRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClientManager elasticClientManager,
            ProcessedConfigurationCache processedConfigurationCache,
            ITaxonManager taxonManager,
            IProcessClient processClient,
            IMemoryCache memoryCache)
        {
            var processedObservationRepository = new ProcessedObservationRepository(
                elasticClientManager,
                processedConfigurationCache,
                elasticConfiguration,
                taxonManager,
                new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
                new NullLogger<ProcessedObservationRepository>());
            return processedObservationRepository;
        }

        private ProcessedObservationRepositoryTest CreateProcessedObservationRepositoryTest(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClientManager elasticClientManager,
            ProcessedConfigurationCache processedConfigurationCache,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            IProcessClient processClient)
        {
            var processedObservationRepository = new ProcessedObservationRepositoryTest(
                elasticClientManager,
                elasticConfiguration,
                processedConfigurationCache,
                clusterHealthCache,
                new NullLogger<ProcessedObservationRepositoryTest>());
            return processedObservationRepository;
        }

        private ProcessedTaxonRepository CreateProcessedTaxonRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClientManager elasticClientManager,
            IProcessClient processClient,
            ITaxonManager taxonManager)
        {
            var processedTaxonRepository = new ProcessedTaxonRepository(
                elasticClientManager,
                elasticConfiguration,
                new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new MemoryCache(new MemoryCacheOptions()), new NullLogger<ProcessedConfigurationCache>()),
                taxonManager,
                new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
                new NullLogger<ProcessedTaxonRepository>());
            return processedTaxonRepository;
        }

        public void UseMockUserService(params AuthorityModel[] authorities)
        {
            UserModel user = new UserModel();
            user.Id = 15;
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(userService => userService.GetUserAsync())
                .ReturnsAsync(user);
            userServiceMock.Setup(userService =>
                    userService.GetUserAuthoritiesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authorities);
            FilterManager.UserService = userServiceMock.Object;

            var contextAccessor = new HttpContextAccessor() { HttpContext = new DefaultHttpContext() };
            var claimsIdentity = new ClaimsIdentity();
            var claim = new Claim("scope", "SOS.Observations.Protected");
            claimsIdentity.AddClaim(claim);
            contextAccessor.HttpContext.User.AddIdentity(claimsIdentity);
        }

        public void UseUserServiceWithToken(string token)
        {
            var userService = CreateUserService(token);
            FilterManager.UserService = userService;
            _userManager.UserService = userService;
        }
    }
}