using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Export.Extensions;
using SOS.Export.MongoDb;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.Test.Repositories.Destination
{
    public class ProcessedDwcRepositoryIntegrationTests : TestBase
    {
        [Fact]
        public async Task GetProjectParameters()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                exportClient,
                new NullLogger<ProcessedDarwinCoreRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var projectParameters = await processedDarwinCoreRepository.GetProjectParameters(0, 100);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            projectParameters.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetProjectParametersAndConvertToExtendedMeasurementOrFactModels()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                exportClient,
                new NullLogger<ProcessedDarwinCoreRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var projectParameters = await processedDarwinCoreRepository.GetProjectParameters(0, 100);
            var extendedMeasurementOrFactRows = projectParameters.ToExtendedMeasurementOrFactRows();
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            extendedMeasurementOrFactRows.Should().NotBeEmpty();
        }
    }
}
