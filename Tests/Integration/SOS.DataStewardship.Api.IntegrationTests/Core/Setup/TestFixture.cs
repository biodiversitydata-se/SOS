﻿using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Managers;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Resource;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Managers;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch.Cluster;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup
{
    public class TestFixture : IAsyncLifetime
    {
        private readonly TelemetryConfiguration _telemetryConfiguration;

        public ApiWebApplicationFactory ApiFactory { get; set; }
        public TestContainersFixture TestContainerFixture { get; private set; }
        public ServiceProvider? ServiceProvider { get; private set; }
        public ProcessFixture? ProcessFixture { get; private set; }
        public HttpClient ApiClient => ApiFactory.CreateClient();

        /// <summary>
        /// Constructor
        /// </summary>
        public TestFixture()
        {
            ApiFactory = new ApiWebApplicationFactory();
            TestContainerFixture = new TestContainersFixture();

            _telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            _telemetryConfiguration.DisableTelemetry = true;
        }

        public async Task InitializeAsync()
        {
            await TestContainerFixture.InitializeAsync();
            ServiceProvider = RegisterServices();
            ApiFactory.ServiceProvider = ServiceProvider;
            using var scope = ServiceProvider.CreateScope();
            ProcessFixture = scope.ServiceProvider.GetService<ProcessFixture>();
            await ProcessFixture!.InitializeElasticsearchIndices();
        }

        public async Task DisposeAsync()
        {
            await ApiFactory.DisposeAsync();
        }

        private ServiceProvider RegisterServices()
        {
            var serviceCollection = GetServiceCollection();
            var testContainersServiceCollection = TestContainerFixture.GetServiceCollection();
            var serviceProvider = ServiceProviderExtensions.RegisterServices(serviceCollection, testContainersServiceCollection);
            return serviceProvider;
        }

        public ServiceCollection GetServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();            
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();
            serviceCollection.AddSingleton(new AreaConfiguration());
            serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
            serviceCollection.AddSingleton<ProcessFixture>();
            serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
            serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
            serviceCollection.AddSingleton<ITaxonListRepository, TaxonListRepository>();
            serviceCollection.AddSingleton<ITaxonManager, TaxonManager>();            
            serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
            serviceCollection.AddSingleton<ProcessConfiguration>();

            serviceCollection.AddSingleton(new TelemetryClient(_telemetryConfiguration));
            serviceCollection.AddSingleton<IElasticClientManager, ElasticClientTestManager>();
            serviceCollection.AddSingleton<IDatasetRepository, DatasetRepository>();
            serviceCollection.AddSingleton<IEventRepository, EventRepository>();
            serviceCollection.AddSingleton<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();

            var elasticConfiguration = CreateElasticSearchConfiguration();
            serviceCollection.AddSingleton(elasticConfiguration);
            serviceCollection.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            serviceCollection.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
            serviceCollection.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            serviceCollection.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>());
            serviceCollection.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);

            return serviceCollection;
        }

        private ElasticSearchConfiguration CreateElasticSearchConfiguration()
        {
            return new ElasticSearchConfiguration()
            {
                IndexSettings = new List<ElasticSearchIndexConfiguration>()
                {
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observation",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s",
                    },
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observationEvent",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s"
                    },
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observationDataset",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s"
                    }
                },
                RequestTimeout = 300,
                DebugMode = true,
                IndexPrefix = "",
                Clusters = null
                //Clusters = new[]
                //{
                //    new Cluster()
                //    {
                //        Hosts = strHosts                    
                //    }
                //}
            };
        }
    }
}
