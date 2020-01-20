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
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.TestHelpers.IO;
using Xunit;

namespace SOS.Export.IntegrationTests.Factories
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
                new DwcArchiveOccurrenceCsvWriter(new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()), 
                    new FileService(), 
                    new NullLogger<DwcArchiveFileWriter>());

            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            SightingFactory sightingFactory = new SightingFactory(
                dwcArchiveFileWriter,
                new ProcessedSightingRepository(
                    exportClient,
                    new Mock<ILogger<ProcessedSightingRepository>>().Object),
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object), 
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path },
                new Mock<ILogger<SightingFactory>>().Object);
            var filename = FilenameGenerator.CreateFilename("sos_dwc_archive_with_all_data");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result = await sightingFactory.ExportAllAsync(filename, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}
