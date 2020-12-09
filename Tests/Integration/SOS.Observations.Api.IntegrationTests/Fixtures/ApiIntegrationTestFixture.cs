using System;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Resource;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Managers;
using SOS.TestHelpers;

namespace SOS.Observations.Api.IntegrationTests.Fixtures
{
    public class ApiIntegrationTestFixture : FixtureBase, IDisposable
    {
        public InstallationEnvironment InstallationEnvironment { get; private set; }
        public ObservationsController ObservationsController { get; private set; }
        public VocabulariesController VocabulariesController { get; private set; }
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

        private void Initialize()
        {
            ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
            var elasticClient = elasticConfiguration.GetClient(true);
            var mongoDbConfiguration = GetMongoDbConfiguration();
            var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
                mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
            var taxonManager = CreateTaxonManager(processClient);

            var processedObservationRepository = CreateProcessedObservationRepository(elasticConfiguration, elasticClient, processClient);
            var vocabularyManger = CreateVocabularyManager(processClient);
            var observationManager = CreateObservationManager(processedObservationRepository, vocabularyManger, processClient, taxonManager);
            ObservationsController = new ObservationsController(observationManager, taxonManager, new NullLogger<ObservationsController>());
            VocabulariesController = new VocabulariesController(vocabularyManger, new NullLogger<VocabulariesController>());
            TaxonManager = taxonManager;
        }

        private TaxonManager CreateTaxonManager(ProcessClient processClient)
        {
            var taxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
            var taxonManager = new TaxonManager(taxonRepository, new MemoryCache(new MemoryCacheOptions()), new NullLogger<TaxonManager>());
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
            var filterManager = new FilterManager(taxonManager, areaCache);
            var observationsManager = new ObservationManager(processedObservationRepository, vocabularyManager,
                filterManager, new NullLogger<ObservationManager>());

            return observationsManager;
        }

        private VocabularyManager CreateVocabularyManager(ProcessClient processClient)
        {
            var vocabularyRepository = new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyCache = new VocabularyCache(vocabularyRepository);
            var vocabularyManager = new VocabularyManager(vocabularyCache, new NullLogger<VocabularyManager>());
            return vocabularyManager;
        }

        private Repositories.ProcessedObservationRepository CreateProcessedObservationRepository(
            ElasticSearchConfiguration elasticConfiguration,
            IElasticClient elasticClient,
            IProcessClient processClient)
        {
            var processedObservationRepository = new Repositories.ProcessedObservationRepository(
                elasticClient, 
                processClient,
                elasticConfiguration, 
                new TelemetryClient(), 
                new NullLogger<Repositories.ProcessedObservationRepository>(),
                new ClassCache<ProcessedConfiguration>(new MemoryCache(new MemoryCacheOptions())));
            return processedObservationRepository;
        }
    }
}