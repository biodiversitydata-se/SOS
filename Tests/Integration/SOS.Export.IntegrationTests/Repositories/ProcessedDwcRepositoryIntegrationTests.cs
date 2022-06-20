using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Database;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;
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
                    elasticConfiguration,
                    new ProcessedConfigurationCache(new ProcessedConfigurationRepository(exportClient, new NullLogger<ProcessedConfigurationRepository>())),
                    new Mock<ITaxonManager>().Object,                    
                    new NullLogger<ProcessedObservationRepository>());

            return processedObservationRepository;
        }

    }
}