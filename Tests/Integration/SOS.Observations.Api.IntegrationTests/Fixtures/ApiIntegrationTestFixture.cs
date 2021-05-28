using System;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Managers;
using SOS.TestHelpers;
using DataProviderManager = SOS.Observations.Api.Managers.DataProviderManager;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{
    public class ApiIntegrationTestFixture : FixtureBase, IDisposable
    {
        public InstallationEnvironment InstallationEnvironment { get; private set; }
        public ObservationsController ObservationsController { get; private set; }
        public VocabulariesController VocabulariesController { get; private set; }
        public DataProvidersController DataProvidersController { get; private set; }

        public TaxonManager TaxonManager { get; private set; }

        public ApiIntegrationTestFixture()
        {
            InstallationEnvironment = GetEnvironmentFromAppSettings();
            Initialize();
        }

        public void Dispose() { }

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

        protected ObservationApiConfiguration GetObservationApiConfiguration()
        {
            var config = GetAppSettings();
            var configPrefix = GetConfigPrefix(InstallationEnvironment);
            var observationApiConfiguration = config.GetSection($"{configPrefix}:ObservationApiConfiguration").Get<ObservationApiConfiguration>();
            return observationApiConfiguration;
        }

        private void Initialize()
        {
            ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
            var observationApiConfiguration = GetObservationApiConfiguration();
            var elasticClient = elasticConfiguration.GetClient(true);
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var areaManager = CreateAreaManager(processClient);
            var taxonManager = CreateTaxonManager(processClient, memoryCache);
            var processedObservationRepository = CreateProcessedObservationRepository(elasticConfiguration, elasticClient, processClient, memoryCache);
            var vocabularyManger = CreateVocabularyManager(processClient);
            var observationManager = CreateObservationManager(processedObservationRepository, vocabularyManger, processClient, taxonManager);
            var processInfoRepository = new ProcessInfoRepository(processClient, elasticConfiguration, new NullLogger<ProcessInfoRepository>());
            var processInfoManager = new ProcessInfoManager(processInfoRepository, new NullLogger<ProcessInfoManager>());
            var dataProviderCache = new DataProviderCache(new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()));
            var dataproviderManager = new DataProviderManager(dataProviderCache, processInfoManager, new NullLogger<DataProviderManager>());
            ObservationsController = new ObservationsController(observationManager, taxonManager, areaManager, observationApiConfiguration,  new NullLogger<ObservationsController>());
            VocabulariesController = new VocabulariesController(vocabularyManger, new NullLogger<VocabulariesController>());
            DataProvidersController = new DataProvidersController(dataproviderManager, observationManager, new NullLogger<DataProvidersController>());
            TaxonManager = taxonManager;
        }

        private AreaManager CreateAreaManager(ProcessClient processClient)
        {
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository);
            var areaManager = new AreaManager(areaCache, new NullLogger<AreaManager>());
            return areaManager;
        }

        private TaxonManager CreateTaxonManager(ProcessClient processClient, IMemoryCache memoryCache)
        {
            var taxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
            var taxonListRepository = new TaxonListRepository(processClient, new NullLogger<TaxonListRepository>());
            var taxonManager = new TaxonManager(taxonRepository, taxonListRepository, 
                new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache),
                new ClassCache<TaxonListSetsById>(memoryCache), 
                new NullLogger<TaxonManager>());
            return taxonManager;
        }

        private ObservationManager CreateObservationManager(
            Repositories.ProcessedObservationRepository processedObservationRepository, 
            VocabularyManager vocabularyManager,
            ProcessClient processClient,
            TaxonManager taxonManager)
        {
            var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
            var areaCache = new AreaCache(areaRepository);
            var dataproviderRepsoitory = new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>());
            var dataproviderCache = new DataProviderCache(dataproviderRepsoitory);
            var userServiceMock = new Moq.Mock<IUserService>();
            var userServiceConfiguration = new UserServiceConfiguration();
            userServiceConfiguration.BaseAddress = "https://artdatauser-st.artdata.slu.se/api";
            userServiceConfiguration.AcceptHeaderContentType = "application/json";
            var userService = new UserService(new Mock<IAuthorizationProvider>().Object,
                new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
            var filterManager = new FilterManager(taxonManager, userService, areaCache, dataproviderCache);
            //var filterManager = new FilterManager(taxonManager, userServiceMock.Object, areaCache, dataproviderCache);
            var observationsManager = new ObservationManager(processedObservationRepository, vocabularyManager,
                filterManager,  new NullLogger<ObservationManager>());

            return observationsManager;
        }

        private VocabularyManager CreateVocabularyManager(ProcessClient processClient)
        {
            var vocabularyRepository = new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyCache = new VocabularyCache(vocabularyRepository);
            var projectInfoRepository = new ProjectInfoRepository(processClient, new NullLogger<ProjectInfoRepository>());
            var projectInfoCache = new ProjectCache(projectInfoRepository);
            var vocabularyManager = new VocabularyManager(vocabularyCache, projectInfoCache, new NullLogger<VocabularyManager>());
            return vocabularyManager;
        }

        private Repositories.ProcessedObservationRepository CreateProcessedObservationRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClient elasticClient,
            IProcessClient processClient,
            IMemoryCache memoryCache)
        {
            var processedConfigurationCache = new ClassCache<ProcessedConfiguration>(memoryCache);
            var processedObservationRepository = new Repositories.ProcessedObservationRepository(
                elasticClient, 
                processClient,
                elasticConfiguration, 
                new TelemetryClient(), 
                new NullLogger<Repositories.ProcessedObservationRepository>(),
                processedConfigurationCache,
                new HttpContextAccessor());
            return processedObservationRepository;
        }
    }
}