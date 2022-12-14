using Elasticsearch.Net;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class DataStewardshipApiWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime where T : class
{    
    public TestcontainerDatabase ElasticsearchContainer { get; set; }
    public TestcontainerDatabase MongoDbContainer { get; set; }    
    public IObservationDatasetRepository? ObservationDatasetRepository { get; private set; }

    public DataStewardshipApiWebApplicationFactory()
    {
        //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DEV");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");        
        var elasticsearchConfiguration = new ElasticsearchTestcontainerConfiguration { Password = "secret" };
        ElasticsearchContainer = new TestcontainersBuilder<ElasticsearchTestcontainer>()
                .WithDatabase(elasticsearchConfiguration)
                .WithCleanUp(true)
                .Build();

        var mongoDbNoAuthConfiguration = new MongoDbTestcontainerConfiguration { Database = "db", Username = null, Password = null };
        var mongodbConfiguration = new MongoDbTestcontainerConfiguration { Database = "db", Username = "mongo", Password = "mongo" };
        MongoDbContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
                .WithDatabase(mongodbConfiguration)
                .WithCleanUp(true)                
                .Build();        
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

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
            services.Replace(ServiceDescriptor.Scoped<IObservationDatasetRepository>(x => ObservationDatasetRepository));            
        });
    }

    private async Task<ElasticClient> InitializeElasticsearchAsync()
    {
        await ElasticsearchContainer.StartAsync().ConfigureAwait(false);
        var elasticClient = new ElasticClient(new ConnectionSettings(new Uri(this.ElasticsearchContainer.ConnectionString))
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .EnableApiVersioningHeader()
            .BasicAuthentication(this.ElasticsearchContainer.Username, this.ElasticsearchContainer.Password)
        .EnableDebugMode());
        return elasticClient;
    }

    private void InitializeObservationDatasetRepository(ElasticClient elasticClient)
    {
        ElasticSearchConfiguration elasticConfiguration = new ElasticSearchConfiguration()
        {
            WriteBatchSize = 1000,
            RequestTimeout = 300,
            ReadBatchSize = 10000,
            DebugMode = true,
            ScrollBatchSize = 5000,
            ScrollTimeout = "600s",
            NumberOfShards = 10,
            NumberOfReplicas = 0,
            IndexPrefix = ""
        };

        var elasticClientManager = new ElasticClientTestManager(elasticClient);
        var processClientMock = new Mock<IProcessClient>();
        var processedConfiguration = new ProcessedConfiguration();
        processedConfiguration.ActiveInstance = 0;
        var processedConfigurationCacheMock = new Mock<ICache<string, ProcessedConfiguration>>();
        processedConfigurationCacheMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(processedConfiguration));

        var processedConfigurationCache = new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClientMock.Object, new NullLogger<ProcessedConfigurationRepository>()));
        ObservationDatasetRepository = new ObservationDatasetRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, new NullLogger<ObservationDatasetRepository>());
    }

    private async Task InitializeMongoDbAsync()
    {
        await MongoDbContainer.StartAsync().ConfigureAwait(false);
        var mongoDbClient = new MongoClient(MongoDbContainer.ConnectionString);
        var mongoDbDatabase = mongoDbClient.GetDatabase(this.MongoDbContainer.Database);
    }

    public async Task InitializeAsync()
    {
        await InitializeMongoDbAsync();
        var elasticClient = await InitializeElasticsearchAsync();
        InitializeObservationDatasetRepository(elasticClient);
    }

    public new async Task DisposeAsync()
    {
        await ElasticsearchContainer.DisposeAsync();
        await MongoDbContainer.DisposeAsync();        
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

}
