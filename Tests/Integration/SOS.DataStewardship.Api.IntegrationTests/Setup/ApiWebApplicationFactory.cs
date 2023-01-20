using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Reflection;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class ApiWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime where T : class
{
    public ServiceProvider ServiceProvider { get; private set; }
    public TestcontainerDatabase ElasticsearchContainer { get; set; }
    public TestcontainerDatabase MongoDbContainer { get; set; }    
    public IObservationDatasetRepository? ObservationDatasetRepository { get; private set; }
    public IObservationEventRepository? ObservationEventRepository { get; private set; }
    public IProcessedObservationCoreRepository? ProcessedObservationCoreRepository { get; private set; }
    public IProcessClient? ProcessClient { get; private set; } = null;
    public ApiWebApplicationFactory()
    {
        //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "DEV");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");        
        var elasticsearchConfiguration = new ElasticsearchTestcontainerConfiguration { Password = "secret" };
        ElasticsearchContainer = new TestcontainersBuilder<ElasticsearchTestcontainer>()
                .WithDatabase(elasticsearchConfiguration)
                .WithCleanUp(true)
                .Build();

        //var mongoDbNoAuthConfiguration = new MongoDbTestcontainerConfiguration { Database = "db", Username = null, Password = null };
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
            services.Replace(ServiceDescriptor.Scoped<IObservationEventRepository>(x => ObservationEventRepository));
            services.Replace(ServiceDescriptor.Scoped<IProcessedObservationCoreRepository>(x => ProcessedObservationCoreRepository));
            services.Replace(ServiceDescriptor.Singleton<IProcessClient>(x => ProcessClient));
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

    private async Task InitializeElasticsearchRepositoriesAsync(ElasticClient elasticClient)
    {        
        string[] strHosts = elasticClient.ConnectionSettings.ConnectionPool.Nodes.Select(m => m.Uri.ToString()).ToArray();
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

        var elasticClientManager = new ElasticClientTestManager(elasticClient);
        var processClientMock = new Mock<IProcessClient>();
        var processedConfiguration = new ProcessedConfiguration();
        processedConfiguration.ActiveInstance = 0;
        var processedConfigurationCacheMock = new Mock<ICache<string, ProcessedConfiguration>>();
        processedConfigurationCacheMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(processedConfiguration));

        var processedConfigurationCache = new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClientMock.Object, new NullLogger<ProcessedConfigurationRepository>()));
        ObservationDatasetRepository = new ObservationDatasetRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, new NullLogger<ObservationDatasetRepository>());
        await ObservationDatasetRepository.ClearCollectionAsync();
        ObservationEventRepository = new ObservationEventRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, new NullLogger<ObservationEventRepository>());
        await ObservationEventRepository.ClearCollectionAsync();
        var telemetryClient = new TelemetryClient();
        TelemetryConfiguration.Active.DisableTelemetry = true;
        ProcessedObservationCoreRepository = new ProcessedObservationCoreRepository(elasticClientManager, elasticConfiguration, processedConfigurationCacheMock.Object, telemetryClient, new NullLogger<ProcessedObservationCoreRepository>());
        await ProcessedObservationCoreRepository.ClearCollectionAsync(false);
        await ProcessedObservationCoreRepository.ClearCollectionAsync(true);
    }

    private async Task InitializeMongoDbAsync()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var filePath = Path.Combine(assemblyPath, @"Resources\mongodb-sos-dev.gz");
        var mongoDbBackupBytes = await File.ReadAllBytesAsync(filePath);        

        await MongoDbContainer.StartAsync().ConfigureAwait(false);
        await MongoDbContainer.CopyFileAsync("/dump/mongodb-sos-dev.gz", mongoDbBackupBytes);
        var cmds = new List<string>()
        {
            "mongorestore", "--gzip", "--archive=/dump/mongodb-sos-dev.gz", "--username", "mongo", "--password", "mongo"            
        };

        var execResult = await MongoDbContainer.ExecAsync(cmds);        
        var mongoClientSettings = MongoClientSettings.FromConnectionString(MongoDbContainer.ConnectionString);
        ProcessClient = new ProcessClient(mongoClientSettings, "sos-dev", 10000, 10000);
    }

    public async Task InitializeAsync()
    {
        await InitializeMongoDbAsync();
        var elasticClient = await InitializeElasticsearchAsync();
        await InitializeElasticsearchRepositoriesAsync(elasticClient);
        ServiceProvider = RegisterServices();
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

    public ServiceProvider RegisterServices()
    {        
        var serviceCollection = new ServiceCollection();

        // Processed Mongo Db
        //var processedDbConfiguration = Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        var processedDbConfiguration = new MongoDbConfiguration()
        {
            AuthenticationDb = "admin",
            DatabaseName = "sos-dev",
            ReadBatchSize = 10000,
            WriteBatchSize = 10000,
            Hosts = new MongoDbServer[]
            {
                new MongoDbServer
                {
                    Name = "artmongo2-1-test.artdata.slu.se",
                    Port = 27017
                }
            }
        };

        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        serviceCollection.AddSingleton<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
            processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

        serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();
        serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
        serviceCollection.AddSingleton<ProcessFixture>();        

        serviceCollection.AddLogging();
        //serviceCollection.AddLogging(builder => builder.AddDebug());
        //serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
        serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
        serviceCollection.AddSingleton<ProcessConfiguration>();
        serviceCollection.AddSingleton(ProcessClient);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider;
    }
}