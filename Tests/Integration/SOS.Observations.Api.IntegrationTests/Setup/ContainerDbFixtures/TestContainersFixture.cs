using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Elastic.Clients.Elasticsearch;
using MongoDB.Driver;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Observations.Api.IntegrationTests.Extensions;
using Testcontainers.Elasticsearch;
using Testcontainers.MongoDb;

namespace SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;
public class TestContainersFixture : IAsyncLifetime
{
    private bool UseKibanaDebug;
    private const string ELASTIC_PASSWORD = "elastic";
    private const string ELASTIC_IMAGE_NAME = "elasticsearch:8.17.3";

    private const string MONGODB_USERNAME = "mongo";
    private const string MONGODB_PASSWORD = "admin";
    private const string MONGODB_IMAGE_NAME = "mongo:6.0.5";

    private ElasticsearchContainer? _elasticsearchContainer { get; set; }
    private IContainer? _kibanaContainer { get; set; }
    private MongoDbContainer? _mongoDbContainer { get; set; }
    private MongoDbContainer? _mongoDbHarvestDbContainer { get; set; }
    private IProcessClient? _processClient { get; set; }
    private IVerbatimClient? _verbatimClient { get; set; }
    private ElasticsearchClient? _elasticClient { get; set; }
    private INetwork? _network;
    private Task? _kibanaStartup;

    public TestContainersFixture()
    {
        UseKibanaDebug = false;
    }

    public async ValueTask InitializeAsync()
    {
        var processClient = InitializeMongoDbAsync();
        var verbatimClient = InitializeMongoDbHarvestDbAsync();
        var elasticClient = InitializeElasticsearchAsync();
        await Task.WhenAll(processClient, verbatimClient, elasticClient);
        _processClient = await processClient;
        _verbatimClient = await verbatimClient;
        _elasticClient = await elasticClient;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // Do not dispose Kibana while its optional background startup is still running.
            if (_kibanaStartup != null) await _kibanaStartup;
            IContainer?[] containers = [_elasticsearchContainer, _kibanaContainer, _mongoDbContainer, _mongoDbHarvestDbContainer];
            await Task.WhenAll(containers.OfType<IContainer>().Select(container => container.DisposeAsync().AsTask()));
        }
        finally
        {
            if (_network != null) await _network.DisposeAsync();
        }
    }

    public ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_processClient!);
        serviceCollection.AddSingleton(_verbatimClient!);
        serviceCollection.AddSingleton(_elasticClient!);

        return serviceCollection;
    }

    private async Task<ElasticsearchClient> InitializeElasticsearchAsync()
    {
        if (UseKibanaDebug)
        {
            return await InitializeElasticsearchWithKibanaAsync();
        }

        _elasticsearchContainer = new ElasticsearchBuilder(ELASTIC_IMAGE_NAME)
            .WithCleanUp(true)
            .WithPassword(ELASTIC_PASSWORD)
        .Build();

        await _elasticsearchContainer.StartAsync().ConfigureAwait(false);
        var elasticUri = new Uri(_elasticsearchContainer.GetConnectionString());
        var elasticClientSettings = ElasticSearchHelper.GetDefaultSettings(elasticUri).EnableDebugMode();
        var elasticClient = new ElasticsearchClient(elasticClientSettings);

        return elasticClient;
    }

    private async Task<ElasticsearchClient> InitializeElasticsearchWithKibanaAsync()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
        .Build();

        _elasticsearchContainer = new ElasticsearchBuilder(ELASTIC_IMAGE_NAME)
            .WithCleanUp(true)
            .WithPortBinding(9200, 9200)
            .WithNetwork(_network)
            .WithNetworkAliases("elastic-test-network")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("started")
                .UntilHttpRequestIsSucceeded(r => r.ForPort(9200).ForPath("/_cluster/health")))
        .Build();

        _kibanaContainer = new ContainerBuilder()
          .WithName(Guid.NewGuid().ToString("D"))
          .WithImage("docker.elastic.co/kibana/kibana:8.17.3")
          .WithPortBinding(5601, 5601)
          .WithNetwork(_network)
          .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elastic-test-network:9200")
          //.WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
        .Build();

        await _network.CreateAsync().ConfigureAwait(false);
        await _elasticsearchContainer.StartAsync().ConfigureAwait(false);

        // Kibana is only for debug inspection – start in background so tests are not blocked.
        Console.WriteLine("[Kibana] Starting container in background (image: docker.elastic.co/kibana/kibana:8.17.3)...");
        _kibanaStartup = _kibanaContainer.StartAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                Console.Error.WriteLine($"[Kibana] Failed to start: {t.Exception?.GetBaseException().Message}");
            else
                Console.WriteLine("[Kibana] Container ready – open http://localhost:5601 (Kibana may still be initializing)");
        }, TaskScheduler.Default);

        var elasticUri = new UriBuilder(Uri.UriSchemeHttp, _elasticsearchContainer.Hostname, _elasticsearchContainer.GetMappedPublicPort(9200));
        var elasticClientSettings = ElasticSearchHelper.GetDefaultSettings(elasticUri.Uri).EnableDebugMode();
        var elasticClient = new ElasticsearchClient(elasticClientSettings);
        return elasticClient;
    }

    private async Task<VerbatimClient> InitializeMongoDbHarvestDbAsync()
    {
        _mongoDbHarvestDbContainer = new MongoDbBuilder(MONGODB_IMAGE_NAME)
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
        _mongoDbContainer = new MongoDbBuilder(MONGODB_IMAGE_NAME)
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
