using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Export.Extensions;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Lib.Models.Search;
using Xunit;

namespace SOS.Export.IntegrationTests.Repositories
{
    public class ProcessedDwcRepositoryIntegrationTests : TestBase
    {
        [Fact]
        public async Task Project_parameters_is_fetched_from_ProcessedDarwinCoreRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedSightingRepository = GetProcessedSightingRepository();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var projectParameters = await processedSightingRepository.GetProjectParameters(new AdvancedFilter(), 0, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            projectParameters.Should().NotBeEmpty();
        }

        [Fact]
        public async Task DarwinCoreProject_objects_is_converted_to_ExtendedMeasurementOrFactRow_objects()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedSightingRepository = GetProcessedSightingRepository();
            var projectParameters = await processedSightingRepository.GetProjectParameters(new AdvancedFilter(), 0,100);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var extendedMeasurementOrFactRows = projectParameters.ToExtendedMeasurementOrFactRows();
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            extendedMeasurementOrFactRows.Should().NotBeEmpty();
        }

        private ProcessedSightingRepository GetProcessedSightingRepository()
        {
            var exportConfiguration = GetExportConfiguration();
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            ProcessedSightingRepository processedSightingRepository = new ProcessedSightingRepository(
                exportClient,
                new NullLogger<ProcessedSightingRepository>());

            return processedSightingRepository;
        }
    }
}
