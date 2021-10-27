using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson.Serialization.Conventions;
using Moq;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.ObservationApi;
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
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.UserService;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Controllers;
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
        public VocabulariesController VocabulariesController { get; private set; }
        public DataProvidersController DataProvidersController { get; private set; }
        public IProcessedObservationRepository ProcessedObservationRepository { get; set; }
        public IProcessedObservationRepository CustomProcessedObservationRepository { get; set; }
        private IFilterManager _filterManager { get; set; }

        public TaxonManager TaxonManager { get; private set; }

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

        private void Initialize()
        {
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
            var taxonManager = CreateTaxonManager(processClient, memoryCache);
            var processedObservationRepository = CreateProcessedObservationRepository(elasticConfiguration, elasticClientManager, processClient, memoryCache);
            var vocabularyRepository = new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyManger = CreateVocabularyManager(processClient, vocabularyRepository);

            var processInfoRepository = new ProcessInfoRepository(processClient, elasticConfiguration, new NullLogger<ProcessInfoRepository>());
            var processInfoManager = new ProcessInfoManager(processInfoRepository, new NullLogger<ProcessInfoManager>());
            var dataProviderCache = new DataProviderCache(new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()));
            var dataproviderManager = new DataProviderManager(dataProviderCache, processInfoManager, new NullLogger<DataProviderManager>());
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
            ObservationsController = new ObservationsController(observationManager, taxonManager, areaManager, observationApiConfiguration, new NullLogger<ObservationsController>());
            VocabulariesController = new VocabulariesController(vocabularyManger, new NullLogger<VocabulariesController>());
            DataProvidersController = new DataProvidersController(dataproviderManager, observationManager, new NullLogger<DataProvidersController>());
            ExportsController = new ExportsController(observationManager, blobStorageManagerMock.Object, areaManager,
                taxonManager, exportManager, fileService, userExportRepository, observationApiConfiguration,
                new NullLogger<ExportsController>());
            TaxonManager = taxonManager;
            ProcessedObservationRepository = processedObservationRepository;
            ElasticSearchConfiguration customElasticConfiguration = GetCustomSearchDbConfiguration();
            CustomProcessedObservationRepository = CreateProcessedObservationRepository(customElasticConfiguration, elasticClientManager, processClient, memoryCache);
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
            ProcessedObservationRepository processedObservationRepository,
            VocabularyValueResolver vocabularyValueResolver,
            ProcessClient processClient,
            FilterManager filterManager)
        {
            var protectedLogRepository = new ProtectedLogRepository(processClient, new NullLogger<ProtectedLogRepository>());
            MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions { SizeLimit = null};
            
            var observationsManager = new ObservationManager(processedObservationRepository,
                protectedLogRepository,
                vocabularyValueResolver,
                filterManager,
                new HttpContextAccessor(),
                new TaxonObservationCountCache(),
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

        private VocabularyManager CreateVocabularyManager(ProcessClient processClient, VocabularyRepository vocabularyRepository)
        {
            var vocabularyCache = new VocabularyCache(vocabularyRepository);
            var projectInfoRepository = new ProjectInfoRepository(processClient, new NullLogger<ProjectInfoRepository>());
            var projectInfoCache = new ProjectCache(projectInfoRepository);
            var vocabularyManager = new VocabularyManager(vocabularyCache, projectInfoCache, new NullLogger<VocabularyManager>());
            return vocabularyManager;
        }

        private ProcessedObservationRepository CreateProcessedObservationRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClientManager elasticClientManager,
            IProcessClient processClient,
            IMemoryCache memoryCache)
        {
            var processedConfigurationCache = new ClassCache<ProcessedConfiguration>(memoryCache);
            var processedObservationRepository = new ProcessedObservationRepository(
                elasticClientManager,
                processClient,
                elasticConfiguration,
                processedConfigurationCache,
                new TelemetryClient(),
                new HttpContextAccessor(),
                new NullLogger<ProcessedObservationRepository>());
            return processedObservationRepository;
        }

        public void UseMockUserService(params AuthorityModel[] authorities)
        {
            UserModel user = new UserModel();
            var userServiceMock = new Moq.Mock<IUserService>();
            userServiceMock.Setup(userService => userService.GetUserAsync())
                .ReturnsAsync(user);
            userServiceMock.Setup(userService =>
                    userService.GetUserAuthoritiesAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(authorities);
            _filterManager.UserService = userServiceMock.Object;
        }
    }
}