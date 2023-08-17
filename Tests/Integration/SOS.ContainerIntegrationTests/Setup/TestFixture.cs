using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using SOS.ContainerIntegrationTests.Extensions;
using SOS.ContainerIntegrationTests.Stubs;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Managers;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Cache.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Represents a fixture for integration tests, implementing the <see cref="IAsyncLifetime"/> interface.
/// This fixture provides the necessary setup and teardown logic for integration tests using the Observations API.
/// </summary>
public class IntegrationTestsFixture : IAsyncLifetime
{
    /// <summary>
    /// The <see cref="ObservationsApiWebApplicationFactory"/> used to create the API client.
    /// </summary>
    public ObservationsApiWebApplicationFactory ApiFactory { get; set; }

    public TestContainersFixture TestContainerFixture { get; private set; }
    public ServiceProvider? ServiceProvider { get; private set; }
    public ProcessFixture? ProcessFixture { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestsFixture"/> class.    
    /// </summary>
    public IntegrationTestsFixture()
    {
        ApiFactory = new ObservationsApiWebApplicationFactory();
        TestContainerFixture = new TestContainersFixture();
        TelemetryConfiguration.Active.DisableTelemetry = true;
    }

    /// <summary>
    /// Creates and returns a new <see cref="HttpClient"/> instance that is ready to interact with the REST API.
    /// </summary>
    /// <returns>A new <see cref="HttpClient"/> instance.</returns>
    public HttpClient CreateApiClient()
    {
        return ApiFactory.CreateClient();        
    }

    /// <summary>
    /// Performs asynchronous setup logic before running the integration tests.
    /// For example, this method could be used to initialize temporary databases running in containers.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        await TestContainerFixture.InitializeAsync();
        ServiceProvider = RegisterServices();
        ApiFactory.ServiceProvider = ServiceProvider;
        using var scope = ServiceProvider.CreateScope();
        ProcessFixture = scope.ServiceProvider.GetService<ProcessFixture>();
        await ProcessFixture.InitializeElasticsearchIndices();
    }

    /// <summary>
    /// Performs asynchronous cleanup logic after running the integration tests.    
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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
        serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();
        serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
        serviceCollection.AddSingleton<ProcessFixture>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
        serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
        serviceCollection.AddSingleton<ProcessConfiguration>();

        serviceCollection.AddSingleton<TelemetryClient>();
        serviceCollection.AddSingleton<IElasticClientManager, ElasticClientTestManager>();
        serviceCollection.AddSingleton<IDatasetRepository, DatasetRepository>();
        serviceCollection.AddSingleton<IEventRepository, EventRepository>();
        
        serviceCollection.AddSingleton<IProcessedObservationRepository, ProcessedObservationRepository>();
        serviceCollection.AddSingleton<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();

        var elasticConfiguration = CreateElasticSearchConfiguration();
        serviceCollection.AddSingleton(elasticConfiguration);
        serviceCollection.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        serviceCollection.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();

        serviceCollection.AddSingleton<IVocabularyValueResolver, VocabularyValueResolver>();
        serviceCollection.AddSingleton<IArtportalenDatasetMetadataRepository, ArtportalenDatasetMetadataRepository>();

        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();

        VocabularyConfiguration vocabularyConfiguration = new VocabularyConfiguration()
        {
            ResolveValues = true,
            LocalizationCultureCode = "sv-SE"
        };
        serviceCollection.AddSingleton(vocabularyConfiguration);


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
