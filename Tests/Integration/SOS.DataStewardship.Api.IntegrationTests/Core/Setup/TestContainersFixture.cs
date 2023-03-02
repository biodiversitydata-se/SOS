using Elasticsearch.Net;
using SOS.Lib.Database.Interfaces;
using MongoDB.Driver;
using SOS.Lib.Database;
using SOS.DataStewardship.Api.IntegrationTests.Core.Extensions;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup
{
    public class TestContainersFixture : IAsyncLifetime
    {
        public ElasticsearchTestcontainer ElasticsearchContainer { get; set; }
        public MongoDbTestcontainer MongoDbContainer { get; set; }
        public TestSubstituteModels TestSubstitutes { get; set; }

        public class TestSubstituteModels
        {
            public IProcessClient? ProcessClient { get; set; } = null;
            public IElasticClient? ElasticClient { get; set; }
        }

        public TestContainersFixture()
        {
            var elasticsearchConfiguration = new ElasticsearchTestcontainerConfiguration { Password = "secret" };
            ElasticsearchContainer = new TestcontainersBuilder<ElasticsearchTestcontainer>()
                    .WithDatabase(elasticsearchConfiguration)
                    .WithCleanUp(true)
                    .Build();

            var mongodbConfiguration = new MongoDbTestcontainerConfiguration { Database = "db", Username = "mongo", Password = "mongo" };
            MongoDbContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
                    .WithDatabase(mongodbConfiguration)
                    .WithCleanUp(true)
                    .Build();
        }

        public async Task InitializeAsync()
        {
            var processClient = await InitializeMongoDbAsync();
            var elasticClient = await InitializeElasticsearchAsync();
            TestSubstitutes = new TestSubstituteModels
            {
                ProcessClient = processClient,
                ElasticClient = elasticClient
            };
        }

        public async Task DisposeAsync()
        {
            await ElasticsearchContainer.DisposeAsync();
            await MongoDbContainer.DisposeAsync();
        }

        public ServiceCollection GetServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(TestSubstitutes.ProcessClient);
            serviceCollection.AddSingleton(TestSubstitutes.ElasticClient);

            return serviceCollection;
        }

        private async Task<ElasticClient> InitializeElasticsearchAsync()
        {
            await ElasticsearchContainer.StartAsync().ConfigureAwait(false);
            var elasticClient = new ElasticClient(new ConnectionSettings(new Uri(ElasticsearchContainer.ConnectionString))
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                .EnableApiVersioningHeader()
                .BasicAuthentication(ElasticsearchContainer.Username, ElasticsearchContainer.Password)
            .EnableDebugMode());
            return elasticClient;
        }

        private async Task<ProcessClient> InitializeMongoDbAsync()
        {
            await MongoDbContainer.StartAsync().ConfigureAwait(false);
            await RestoreMongoDbBackup(MongoDbContainer);
            var mongoClientSettings = MongoClientSettings.FromConnectionString(MongoDbContainer.ConnectionString);
            var processClient = new ProcessClient(mongoClientSettings, "sos-dev", 10000, 10000);
            return processClient;
        }

        private async Task RestoreMongoDbBackup(MongoDbTestcontainer mongoDbTestcontainer)
        {
            string filePath = @"data\resources\mongodb-sos-dev.gz".GetAbsoluteFilePath();
            byte[] mongoDbBackupBytes = await File.ReadAllBytesAsync(filePath);
            await mongoDbTestcontainer.CopyFileAsync("/dump/mongodb-sos-dev.gz", mongoDbBackupBytes);
            var cmds = new List<string>()
            {
                "mongorestore", "--gzip", "--archive=/dump/mongodb-sos-dev.gz", "--username", "mongo", "--password", "mongo"
            };
            ExecResult execResult = await mongoDbTestcontainer.ExecAsync(cmds);
            if (execResult.ExitCode != 0) throw new Exception("MongoRestore failed");
        }
    }
}