using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public sealed class DataStewardshipApiWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime where T : class
{

    private readonly TestcontainerDatabase _elasticsearchContainer = new TestcontainersBuilder<ElasticsearchTestcontainer>()
        .WithDatabase(new ElasticsearchTestcontainerConfiguration { Password = "secret" })
        .Build();

    public IObservationDatasetRepository? ObservationDatasetRepository { get; private set; }

    public IObservationEventRepository? ObservationEventRepository { get; private set; }

    public IProcessedObservationCoreRepository? ProcessedObservationCoreRepository { get; private set; }

    static DataStewardshipApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.Configure<AuthenticationOptions>(options =>
            {
                options.SchemeMap.Clear();
                ((IList<AuthenticationSchemeBuilder>)options.Schemes).Clear();
            });

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
            services.Replace(ServiceDescriptor.Scoped<IObservationDatasetRepository>(_ => ObservationDatasetRepository));
            services.Replace(ServiceDescriptor.Scoped<IObservationEventRepository>(_ => ObservationEventRepository));
            services.Replace(ServiceDescriptor.Scoped<IProcessedObservationCoreRepository>(_ => ProcessedObservationCoreRepository));
        });
    }
    
    public async Task InitializeAsync()
    {
        await _elasticsearchContainer.StartAsync()
            .ConfigureAwait(false);
        
        var elasticClient = new ElasticClient(new ConnectionSettings(new Uri(_elasticsearchContainer.ConnectionString))
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .EnableApiVersioningHeader()
            .BasicAuthentication(_elasticsearchContainer.Username, _elasticsearchContainer.Password)
            .EnableDebugMode());
        
        var elasticClientManager = new ElasticClientTestManager(elasticClient);
        
        var elasticConfiguration = new ElasticSearchConfiguration
        {
            WriteBatchSize = 1000,
            RequestTimeout = 300,
            ReadBatchSize = 10000,
            DebugMode = true,
            ScrollBatchSize = 5000,
            ScrollTimeout = "600s",
            NumberOfShards = 10,
            NumberOfReplicas = 0,
            IndexPrefix = "",
            Clusters = null
        };
        
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration { DisableTelemetry = true });
        
        var processedConfigurationCacheMock = new Mock<ICache<string, ProcessedConfiguration>>();
        processedConfigurationCacheMock.Setup(expression => expression.GetAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new ProcessedConfiguration { ActiveInstance = 1 }));
        
        ObservationDatasetRepository = new ObservationDatasetRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, NullLogger<ObservationDatasetRepository>.Instance);
        await ObservationDatasetRepository.ClearCollectionAsync()
            .ConfigureAwait(false);
        
        ObservationEventRepository = new ObservationEventRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object,  NullLogger<ObservationEventRepository>.Instance);
        await ObservationEventRepository.ClearCollectionAsync()
            .ConfigureAwait(false);

        ProcessedObservationCoreRepository = new ProcessedObservationCoreRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, telemetryClient, NullLogger<ProcessedObservationCoreRepository>.Instance);
        await ProcessedObservationCoreRepository.ClearCollectionAsync(false)
            .ConfigureAwait(false);
        await ProcessedObservationCoreRepository.ClearCollectionAsync(true)
            .ConfigureAwait(false);
    }

    public new Task DisposeAsync()
    {
        return Task.WhenAll(base.DisposeAsync().AsTask(), _elasticsearchContainer.DisposeAsync().AsTask());
    }
}