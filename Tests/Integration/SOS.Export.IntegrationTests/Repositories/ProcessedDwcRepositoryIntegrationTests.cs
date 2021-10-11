using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Database;
using SOS.Lib.Managers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Configuration;
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

        private ProcessedObservationRepository GetProcessedObservationRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedObservationRepository =
                new ProcessedObservationRepository(
                    new ElasticClientManager(elasticConfiguration, true),
                    exportClient,
                    elasticConfiguration,
                    new ClassCache<ProcessedConfiguration>(new MemoryCache(new MemoryCacheOptions())),
                    new NullLogger<ProcessedObservationRepository>());

            return processedObservationRepository;
        }

    }
}