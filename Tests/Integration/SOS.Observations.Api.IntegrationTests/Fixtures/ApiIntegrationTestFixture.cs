using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson.Serialization.Conventions;
using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.Artportalen;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.UserService;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.HealthChecks;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.TestHelpers;
using DataProviderManager = SOS.Observations.Api.Managers.DataProviderManager;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{
    public class ApiIntegrationTestFixture : FixtureBase, IDisposable
    {        
        public InstallationEnvironment InstallationEnvironment { get; private set; }
        public ObservationsController ObservationsController { get; private set; }
        public ExportsController ExportsController { get; private set; }
        public SystemsController SystemsController { get; private set; }
        public VocabulariesController VocabulariesController { get; private set; }
        public UserController UserController { get; private set; }
        public DataProvidersController DataProvidersController { get; private set; }
        public IProcessedObservationRepository ProcessedObservationRepository { get; set; }
        public IProcessedObservationRepository CustomProcessedObservationRepository { get; set; }
        public ObservationsController CustomObservationsController { get; private set; }
        public DwcArchiveFileWriter DwcArchiveFileWriter { get; set; }
        private IFilterManager _filterManager;
        private IUserManager _userManager;
        public string UserAuthenticationToken { get; set; }

        public TaxonManager TaxonManager { get; private set; }

        public SearchDataProvidersHealthCheck SearchDataProvidersHealthCheck { get; set; }
        public SearchPerformanceHealthCheck SearchPerformanceHealthCheck { get; set; }
        public AzureSearchHealthCheck AzureSearchHealthCheck { get; set; }

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

        private async Task Initialize()
        {            
            UserAuthenticationToken = GetUserAuthenticationToken();
            ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
            var blobStorageManagerMock = new Mock<IBlobStorageManager>();
            var observationApiConfiguration = GetObservationApiConfiguration();
            var elasticClientManager = new ElasticClientManager(elasticConfiguration, true);
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var areaManager = CreateAreaManager(processClient);
            var taxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
            var taxonManager = CreateTaxonManager(processClient, taxonRepository, memoryCache);
            var processedObservationRepository = CreateProcessedObservationRepository(elasticConfiguration, elasticClientManager, processClient, memoryCache, taxonManager);
            var vocabularyRepository = new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyManger = CreateVocabularyManager(processClient, vocabularyRepository);
            var projectManger = CreateProjectManager(processClient);
            var processInfoRepository = new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var processInfoManager = new ProcessInfoManager(processInfoRepository, new NullLogger<ProcessInfoManager>());
            var dataProviderCache = new DataProviderCache(new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()));
            var dataproviderManager = new DataProviderManager(dataProviderCache, processInfoManager, processedObservationRepository, new NullLogger<DataProviderManager>());
            var fileService = new FileService();
            VocabularyValueResolver vocabularyValueResolver = new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration { ResolveValues = true, LocalizationCultureCode = "sv-SE" });
            var csvFileWriter = new CsvFileWriter(processedObservationRepository, fileService,
                vocabularyValueResolver, new NullLogger<CsvFileWriter>());
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(vocabularyValueResolver, processClient);
            var excelFileWriter = new ExcelFileWriter(processedObservationRepository, fileService,
                vocabularyValueResolver, new NullLogger<ExcelFileWriter>());
            var geojsonFileWriter = new GeoJsonFileWriter(processedObservationRepository, fileService,
                vocabularyValueResolver, new NullLogger<GeoJsonFileWriter>());
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository);
            var userService = CreateUserService();
            var filterManager = new FilterManager(taxonManager, userService, areaCache, dataProviderCache);
            _filterManager = filterManager;
            var observationManager = CreateObservationManager(processedObservationRepository, vocabularyValueResolver, processClient, filterManager);

            var exportManager = new ExportManager(csvFileWriter, dwcArchiveFileWriter, excelFileWriter, geojsonFileWriter,
                processedObservationRepository, processInfoRepository, filterManager, new NullLogger<ExportManager>());
            var userExportRepository = new UserExportRepository(processClient, new NullLogger<UserExportRepository>());
            ObservationsController = new ObservationsController(observationManager, taxonManager, areaManager, observationApiConfiguration, elasticConfiguration, new NullLogger<ObservationsController>());
            VocabulariesController = new VocabulariesController(vocabularyManger, projectManger, new NullLogger<VocabulariesController>());
            DataProvidersController = new DataProvidersController(dataproviderManager, observationManager, new NullLogger<DataProvidersController>());
            ExportsController = new ExportsController(observationManager, blobStorageManagerMock.Object, areaManager,
                taxonManager, exportManager, fileService, userExportRepository, observationApiConfiguration,
                new NullLogger<ExportsController>());
            ExportsController.ControllerContext.HttpContext = new DefaultHttpContext();
            TaxonManager = taxonManager;
            ProcessedObservationRepository = processedObservationRepository;
            ElasticSearchConfiguration customElasticConfiguration = GetCustomSearchDbConfiguration();
            CustomProcessedObservationRepository = CreateProcessedObservationRepository(customElasticConfiguration, elasticClientManager, processClient, memoryCache, taxonManager);
            var customObservationManager = CreateObservationManager((ProcessedObservationRepository)CustomProcessedObservationRepository, vocabularyValueResolver, processClient, filterManager);
            CustomObservationsController = new ObservationsController(customObservationManager, taxonManager, areaManager, observationApiConfiguration, customElasticConfiguration, new NullLogger<ObservationsController>());
            DwcArchiveFileWriter = dwcArchiveFileWriter;
            var healthCheckConfiguration = new HealthCheckConfiguration
            {
                AzureApiUrl = GetAzureApiUrl(),
                AzureSubscriptionKey = GetAzureApiSubscriptionKey()
            };
            SearchDataProvidersHealthCheck = new SearchDataProvidersHealthCheck(observationManager, dataProviderCache);
            SearchPerformanceHealthCheck = new SearchPerformanceHealthCheck(observationManager);
            AzureSearchHealthCheck = new AzureSearchHealthCheck(healthCheckConfiguration);
            SystemsController = new SystemsController(processInfoManager, processedObservationRepository, new NullLogger<SystemsController>());
            _userManager = new UserManager(userService, new NullLogger<UserManager>());
            UserController = new UserController(_userManager, new NullLogger<UserController>());
            var artportalenDataProvider = new Lib.Models.Shared.DataProvider { Id = 1 };
            var taxa = await taxonRepository.GetAllAsync();
            var taxaById = taxa.ToDictionary(m => m.Id, m => m);
            var processTimeManager = new ProcessTimeManager(new ProcessConfiguration());                       
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

        private AreaManager CreateAreaManager(ProcessClient processClient)
        {
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository);
            var areaManager = new AreaManager(areaCache, new NullLogger<AreaManager>());
            return areaManager;
        }

        private TaxonManager CreateTaxonManager(ProcessClient processClient, TaxonRepository taxonRepository, IMemoryCache memoryCache)
        {            
            var taxonListRepository = new TaxonListRepository(processClient, new NullLogger<TaxonListRepository>());
            var taxonManager = new TaxonManager(taxonRepository, taxonListRepository,
                new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache),
                new ClassCache<TaxonListSetsById>(memoryCache),
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
            MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions { SizeLimit = null};
            var artportalenApiService = new ArtportalenApiService(new Mock<IAuthorizationProvider>().Object,
                new HttpClientService(new NullLogger<HttpClientService>()),
                new ArtportalenApiServiceConfiguration { BaseAddress = "https://api.artdata.slu.se/observations/v2", AcceptHeaderContentType = "application/json" },
                new NullLogger<ArtportalenApiService>());
            var artportalenApiManager = new ArtportalenApiManager(artportalenApiService, new NullLogger<ArtportalenApiManager>());


            var observationsManager = new ObservationManager(processedObservationRepository,
                protectedLogRepository,
                vocabularyValueResolver,
                filterManager,
                new HttpContextAccessor(),
                new TaxonObservationCountCache(),
                artportalenApiManager,
                new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions())) { CacheDuration = TimeSpan.FromHours(4) },
                new NullLogger<ObservationManager>());

            return observationsManager;
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
            contextAccessor.HttpContext.Request.Headers.Add("Authorization", token);
            IAuthorizationProvider authorizationProvider = new CurrentUserAuthorization(contextAccessor);
            var userService = new UserService(authorizationProvider,
                new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
            return userService;
        }
        
        private VocabularyManager CreateVocabularyManager(ProcessClient processClient, VocabularyRepository vocabularyRepository)
        {
            var vocabularyCache = new VocabularyCache(vocabularyRepository);
            var vocabularyManager = new VocabularyManager(vocabularyCache, new NullLogger<VocabularyManager>());
            return vocabularyManager;
        }

        private ProjectManager CreateProjectManager(ProcessClient processClient)
        {
            var projectInfoRepository = new ProjectInfoRepository(processClient, new NullLogger<ProjectInfoRepository>());
            var projectInfoCache = new ProjectCache(projectInfoRepository);
            var projectManager = new ProjectManager(projectInfoCache, new NullLogger<ProjectManager>());
            return projectManager;
        }

        private ProcessedObservationRepository CreateProcessedObservationRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClientManager elasticClientManager,
            IProcessClient processClient,
            IMemoryCache memoryCache,
            ITaxonManager taxonManager)
        {
            var processedConfigurationCache = new ClassCache<ProcessedConfiguration>(memoryCache);
            var processedObservationRepository = new ProcessedObservationRepository(
                elasticClientManager,
                processClient,
                elasticConfiguration,
                new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
                new TelemetryClient(),
                new HttpContextAccessor(),
                taxonManager,
                new NullLogger<ProcessedObservationRepository>());
            return processedObservationRepository;
        }

        public void UseMockUserService(params AuthorityModel[] authorities)
        {
            UserModel user = new UserModel();
            user.Id = 15;
            var userServiceMock = new Moq.Mock<IUserService>();
            userServiceMock.Setup(userService => userService.GetUserAsync())
                .ReturnsAsync(user);
            userServiceMock.Setup(userService =>
                    userService.GetUserAuthoritiesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authorities);
            _filterManager.UserService = userServiceMock.Object;

            var contextAccessor = new HttpContextAccessor() { HttpContext = new DefaultHttpContext() };
            var claimsIdentity = new ClaimsIdentity();
            var claim = new Claim("scope", "SOS.Observations.Protected");
            claimsIdentity.AddClaim(claim);
            contextAccessor.HttpContext.User.AddIdentity(claimsIdentity);
            ProcessedObservationRepository.HttpContextAccessor = contextAccessor;
        }


        public void UseUserServiceWithToken(string token)
        {
            var userService = CreateUserService(token);
            _filterManager.UserService = userService;
            _userManager.UserService = userService;
        }
    }
}