﻿using SOS.Lib.Database.Interfaces;
using MongoDB.Driver;
using SOS.Lib.Database;
using Testcontainers.MongoDb;
using Testcontainers.Elasticsearch;
using SOS.Lib.Helpers;
using System;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup
{
    public class TestContainersFixture : IAsyncLifetime
    {
        private bool UseKibanaDebug;
        private const string ELASTIC_PASSWORD = "elastic";
        private const string ELASTIC_IMAGE_NAME = "elasticsearch:8.17.3";

        private const string MONGODB_USERNAME = "mongo";
        private const string MONGODB_PASSWORD = "admin";
        private const string MONGODB_IMAGE_NAME = "mongo:6.0.5";

        public ElasticsearchContainer ElasticsearchContainer { get; set; }
        public MongoDbContainer MongoDbContainer { get; set; }
        public TestSubstituteModels TestSubstitutes { get; set; }

        public class TestSubstituteModels
        {
            public IProcessClient? ProcessClient { get; set; } = null;
            public ElasticsearchClient? ElasticClient { get; set; }
        }

        public TestContainersFixture()
        {
            UseKibanaDebug = false;
            ElasticsearchContainer = new ElasticsearchBuilder()
                .WithImage(ELASTIC_IMAGE_NAME)
                .WithCleanUp(true)
                .WithPassword(ELASTIC_PASSWORD)
                .Build();
       
            MongoDbContainer = new MongoDbBuilder()
                .WithImage(MONGODB_IMAGE_NAME)
                .WithCleanUp(true)
                .WithUsername(MONGODB_USERNAME)
                .WithPassword(MONGODB_PASSWORD)
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

        private async Task<ElasticsearchClient> InitializeElasticsearchAsync()
        {
            if (UseKibanaDebug)
            {
                return await InitializeElasticsearchWithKibanaAsync();
            }
            await ElasticsearchContainer.StartAsync().ConfigureAwait(false);
            var settings = ElasticSearchHelper.GetDefaultSettings(new Uri(ElasticsearchContainer.GetConnectionString()))
                .EnableDebugMode();
            var elasticClient = new ElasticsearchClient(settings);
            
            return elasticClient;
        }

        private async Task<ElasticsearchClient> InitializeElasticsearchWithKibanaAsync()
        {
            var network = new NetworkBuilder()
                .WithName(Guid.NewGuid().ToString("D"))
            .Build();

            ElasticsearchContainer = new ElasticsearchBuilder()
                .WithImage(ELASTIC_IMAGE_NAME)
                .WithCleanUp(true)
                .WithPortBinding(9200, 9200)
                .WithNetwork(network)
                .WithNetworkAliases("elastic-test-network")
                .WithEnvironment("xpack.security.enabled", "false")
            .Build();

            var kibanaContainer = new ContainerBuilder()
              .WithName(Guid.NewGuid().ToString("D"))
              .WithImage("docker.elastic.co/kibana/kibana:8.17.3")
              .WithPortBinding(5601, 5601)
              .WithNetwork(network)
              .WithEnvironment("ELASTICSEARCH_HOSTS", $"http://elastic-test-network:9200")
            .Build();

            await network.CreateAsync().ConfigureAwait(false);
            await ElasticsearchContainer.StartAsync().ConfigureAwait(false);
            await kibanaContainer.StartAsync().ConfigureAwait(false);

            var elasticUri = new UriBuilder(Uri.UriSchemeHttp, ElasticsearchContainer.Hostname, ElasticsearchContainer.GetMappedPublicPort(9200)).ToString();
            var settings = ElasticSearchHelper.GetDefaultSettings(new Uri(ElasticsearchContainer.GetConnectionString()))
                .EnableDebugMode();

            var elasticClient = new ElasticsearchClient(settings);
            return elasticClient;
        }

        private async Task<ProcessClient> InitializeMongoDbAsync()
        {
            await MongoDbContainer.StartAsync().ConfigureAwait(false);
            await RestoreMongoDbBackup(MongoDbContainer);
            var mongoClientSettings = MongoClientSettings.FromConnectionString(MongoDbContainer.GetConnectionString());
            var processClient = new ProcessClient(mongoClientSettings, "sos-dev", 10000, 10000);
            return processClient;
        }

        private async Task RestoreMongoDbBackup(MongoDbContainer mongoDbTestcontainer)
        {
            string filePath = @"Data/Resources/mongodb-sos-dev.gz".GetAbsoluteFilePath();
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
}