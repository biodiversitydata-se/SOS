using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Export.Factories;
using SOS.Export.IO.DwcArchive;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using Xunit;

namespace SOS.Export.Test.Factories
{
    public class SightingFactoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task TestCreateDwcArchiveFileForAllObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()), 
                    new FileService(), 
                    new NullLogger<DwcArchiveFileWriter>());

            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            SightingFactory sightingFactory = new SightingFactory(
                dwcArchiveFileWriter,
                new ProcessedDarwinCoreRepository(
                    exportClient,
                    new Mock<ILogger<ProcessedDarwinCoreRepository>>().Object),
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object), 
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path },
                new Mock<ILogger<SightingFactory>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result = await sightingFactory.ExportAllAsync("fileName", JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}
