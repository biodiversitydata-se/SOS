using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Managers;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Services;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup
{
    public class TestFixture : IAsyncLifetime
    {
        public ApiWebApplicationFactory ApiFactory { get; set; }
        public TestContainersFixture TestContainerFixture { get; private set; }
        public ServiceProvider? ServiceProvider { get; private set; }
        public ProcessFixture? ProcessFixture { get; private set; }
        public HttpClient ApiClient => ApiFactory.CreateClient();

        public TestFixture() 
        { 
            ApiFactory = new ApiWebApplicationFactory();
            TestContainerFixture = new TestContainersFixture();
            TelemetryConfiguration.Active.DisableTelemetry = true;
        }

        public async Task InitializeAsync()
        {
            await TestContainerFixture.InitializeAsync();            
            ServiceProvider = RegisterServices();
            ApiFactory.ServiceProvider = ServiceProvider;
            using var scope = ServiceProvider.CreateScope();
            ProcessFixture = scope.ServiceProvider.GetService<ProcessFixture>();
            await ProcessFixture.InitializeElasticsearchIndices();
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

            var elasticConfiguration = CreateElasticSearchConfiguration();
            serviceCollection.AddSingleton(elasticConfiguration);
            serviceCollection.AddSingleton<ProcessConfiguration>();
            serviceCollection.AddSingleton<UserServiceConfiguration>();

            serviceCollection.AddSingleton<IAreaCache, AreaCache>();
            serviceCollection.AddSingleton<IDataProviderCache, DataProviderCache>();
            serviceCollection.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            serviceCollection.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            serviceCollection.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();

            serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();

            serviceCollection.AddSingleton<IElasticClientManager, ElasticClientTestManager>();
            serviceCollection.AddSingleton<IFilterManager, FilterManager>();
            serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
            serviceCollection.AddSingleton<ITaxonManager, TaxonManager>();

            serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
            serviceCollection.AddSingleton<IDataProviderRepository, DataProviderRepository>();
            serviceCollection.AddSingleton<IObservationDatasetRepository, ObservationDatasetRepository>();
            serviceCollection.AddSingleton<IObservationEventRepository, ObservationEventRepository>();
            serviceCollection.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            serviceCollection.AddSingleton<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
            serviceCollection.AddSingleton<ITaxonListRepository, TaxonListRepository>();
            serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
            serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();

            serviceCollection.AddSingleton<IHttpClientService, HttpClientService>();

            serviceCollection.AddSingleton<ProcessFixture>();
            serviceCollection.AddSingleton<TelemetryClient>();
            serviceCollection.AddSingleton<IAuthorizationProvider, CurrentUserAuthorization>();

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
