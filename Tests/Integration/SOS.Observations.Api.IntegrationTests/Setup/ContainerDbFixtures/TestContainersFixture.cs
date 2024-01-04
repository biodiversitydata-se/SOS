using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Elasticsearch.Net;
using MongoDB.Driver;
using Nest;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Observations.Api.IntegrationTests.Extensions;
using Testcontainers.Elasticsearch;
using Testcontainers.MongoDb;

namespace SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;
public class TestContainersFixture : IAsyncLifetime
{
    private bool UseKibanaDebug;
    private const string ELASTIC_PASSWORD = "elastic";
    private const string ELASTIC_IMAGE_NAME = "elasticsearch:8.7.1";

    private const string MONGODB_USERNAME = "mongo";
    private const string MONGODB_PASSWORD = "admin";
    private const string MONGODB_IMAGE_NAME = "mongo:6.0.5";

    private ElasticsearchContainer? _elasticsearchContainer { get; set; }
    private MongoDbContainer? _mongoDbContainer { get; set; }
    private MongoDbContainer? _mongoDbHarvestDbContainer { get; set; }
    private IProcessClient? _processClient { get; set; }
    private IVerbatimClient? _verbatimClient { get; set; }
    private IElasticClient? _elasticClient { get; set; }

    public TestContainersFixture()
    {
        UseKibanaDebug = false;
    }

    public async Task InitializeAsync()
    {
        _processClient = await InitializeMongoDbAsync();
        _verbatimClient = await InitializeMongoDbHarvestDbAsync();
        _elasticClient = await InitializeElasticsearchAsync();
    }

    public async Task DisposeAsync()
    {
        await _elasticsearchContainer!.DisposeAsync();
        await _mongoDbContainer!.DisposeAsync();
        await _mongoDbHarvestDbContainer!.DisposeAsync();
    }

    public ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_processClient!);
        serviceCollection.AddSingleton(_verbatimClient!);
        serviceCollection.AddSingleton(_elasticClient!);

        return serviceCollection;
    }

    private async Task<ElasticClient> InitializeElasticsearchAsync()
    {
        if (UseKibanaDebug)
        {
            return await InitializeElasticsearchWithKibanaAsync();
        }

        _elasticsearchContainer = new ElasticsearchBuilder()
            .WithImage(ELASTIC_IMAGE_NAME)
            .WithCleanUp(true)
            .WithPassword(ELASTIC_PASSWORD)
        .Build();
        await _elasticsearchContainer.StartAsync().ConfigureAwait(false);
        var elasticClient = new ElasticClient(new ConnectionSettings(new Uri(_elasticsearchContainer.GetConnectionString()))
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .EnableApiVersioningHeader()
        .EnableDebugMode());

        return elasticClient;
    }

    private async Task<ElasticClient> InitializeElasticsearchWithKibanaAsync()
    {
        var network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
        .Build();

        _elasticsearchContainer = new ElasticsearchBuilder()
            .WithImage(ELASTIC_IMAGE_NAME)
            .WithCleanUp(true)
            .WithPortBinding(9200, 9200)
            .WithNetwork(network)
            .WithNetworkAliases("elastic-test-network")
            .WithEnvironment("xpack.security.enabled", "false")
        .Build();

        var kibanaContainer = new ContainerBuilder()
          .WithName(Guid.NewGuid().ToString("D"))
          .WithImage("docker.elastic.co/kibana/kibana:8.7.1")
          .WithPortBinding(5601, 5601)
          .WithNetwork(network)
          .WithEnvironment("ELASTICSEARCH_HOSTS", $"http://elastic-test-network:9200")
        .Build();

        await network.CreateAsync().ConfigureAwait(false);
        await _elasticsearchContainer.StartAsync().ConfigureAwait(false);
        await kibanaContainer.StartAsync().ConfigureAwait(false);

        var elasticUri = new UriBuilder(Uri.UriSchemeHttp, _elasticsearchContainer.Hostname, _elasticsearchContainer.GetMappedPublicPort(9200)).ToString();
        var elasticClient = new ElasticClient(new ConnectionSettings(new Uri(elasticUri))
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .EnableApiVersioningHeader()
        .EnableDebugMode());
        return elasticClient;
    }

    private async Task<VerbatimClient> InitializeMongoDbHarvestDbAsync()
    {
        _mongoDbHarvestDbContainer = new MongoDbBuilder()
            .WithImage(MONGODB_IMAGE_NAME)
            .WithCleanUp(true)
            .WithUsername(MONGODB_USERNAME)
            .WithPassword(MONGODB_PASSWORD)
            .Build();

        await _mongoDbHarvestDbContainer.StartAsync().ConfigureAwait(false);
        var mongoClientSettings = MongoClientSettings.FromConnectionString(_mongoDbHarvestDbContainer.GetConnectionString());
        var verbatimClient = new VerbatimClient(mongoClientSettings, "sos-harvest-dev", 10000, 10000);
        return verbatimClient;
    }

    private async Task<ProcessClient> InitializeMongoDbAsync()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage(MONGODB_IMAGE_NAME)
            .WithCleanUp(true)
            .WithUsername(MONGODB_USERNAME)
            .WithPassword(MONGODB_PASSWORD)
            .Build();

        await _mongoDbContainer.StartAsync().ConfigureAwait(false);
        await RestoreMongoDbBackup(_mongoDbContainer);
        var mongoClientSettings = MongoClientSettings.FromConnectionString(_mongoDbContainer.GetConnectionString());
        var processClient = new ProcessClient(mongoClientSettings, "sos-dev", 10000, 10000);
        return processClient;
    }

    private async Task RestoreMongoDbBackup(MongoDbContainer mongoDbTestcontainer)
    {
        string filePath = "Resources/MongoDb/mongodb-sos-dev.gz".GetAbsoluteFilePath();
        byte[] mongoDbBackupBytes = await File.ReadAllBytesAsync(filePath);
        await mongoDbTestcontainer.CopyAsync(mongoDbBackupBytes, "/dump/mongodb-sos-dev.gz");
        var cmds = new List<string>()
            {
                "mongorestore", "--gzip", "--archive=/dump/mongodb-sos-dev.gz", "--username", MONGODB_USERNAME, "--password", MONGODB_PASSWORD
            };
        ExecResult execResult = await mongoDbTestcontainer.ExecAsync(cmds);
        if (execResult.ExitCode != 0) throw new Exception("MongoRestore failed");
    }
}