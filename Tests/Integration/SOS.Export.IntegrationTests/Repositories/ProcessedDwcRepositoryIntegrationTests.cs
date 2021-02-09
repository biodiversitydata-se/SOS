using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using SOS.Lib.Database;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed;
using Xunit;

namespace SOS.Export.IntegrationTests.Repositories
{
    public class ProcessedDwcRepositoryIntegrationTests : TestBase
    {
        [Fact(Skip = "Not working")]
        public async Task Multimedia_is_fetched_from_ProcessedObservationRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedObservationRepository = GetProcessedObservationRepository();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processedObservationRepository.ScrollMultimediaAsync(new SearchFilter(), null);
            IEnumerable<SimpleMultimediaRow> multimediaRows = result.Records;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            multimediaRows.Should().NotBeEmpty();
        }

        [Fact(Skip = "Not working")]
        public async Task MeasurementOrFacts_is_fetched_from_ProcessedDarwinCoreRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedObservationRepository = GetProcessedObservationRepository();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processedObservationRepository.ScrollMeasurementOrFactsAsync(new SearchFilter(), null);
            var projectParameters = result.Records;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            projectParameters.Should().NotBeEmpty();
        }

        private ProcessedPublicObservationRepository GetProcessedObservationRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var uris = new Uri[elasticConfiguration.Hosts.Length];
            for (var i = 0; i < uris.Length; i++)
            {
                uris[i] = new Uri(elasticConfiguration.Hosts[i]);
            }

            var elasticClient = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris))
                .DisableDirectStreaming().EnableDebugMode().PrettyJson());

            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedObservationRepository =
                new ProcessedPublicObservationRepository(
                    exportClient,
                    elasticClient,
                    elasticConfiguration,
                    new NullLogger<ProcessedPublicObservationRepository>());

            return processedObservationRepository;
        }

    }
}