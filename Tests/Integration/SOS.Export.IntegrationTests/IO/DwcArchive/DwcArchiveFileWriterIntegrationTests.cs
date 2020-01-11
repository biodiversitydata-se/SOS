using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.IntegrationTests.TestHelpers;
using SOS.Export.IntegrationTests.TestHelpers.Factories;
using SOS.Export.IO.DwcArchive;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Services;
using SOS.Lib.Models.Search;
using Xunit;

namespace SOS.Export.IntegrationTests.IO.DwcArchive
{
    public class DwcArchiveFileWriterIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category","Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task Creates_a_DwcArchiveFile_with_all_processed_data_in_MongoDb_instance_and_saves_the_file_on_disk()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            string exportFolderPath = exportConfiguration.FileDestination.Path;
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processedDarwinCoreRepository = new ProcessedDarwinCoreRepository(
                exportClient,
                new Mock<ILogger<ProcessedDarwinCoreRepository>>().Object);
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(), 
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstance);
            var filename = FilenameGenerator.CreateFilename("sos_dwc_archive_with_all_data");
            var filter = new AdvancedFilter();
            //var filter = new AdvancedFilter {TaxonIds = new[] {102951}};

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                filter,
                filename,
                processedDarwinCoreRepository,
                processInfo,
                exportFolderPath,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bool fileExists = System.IO.File.Exists(zipFilePath);
            fileExists.Should().BeTrue("because the zip file should have been generated");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task Creates_a_DwcArchiveFile_with_ten_predefined_observations_and_saves_the_file_on_disk()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            string exportFolderPath = exportConfiguration.FileDestination.Path;
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(@"Resources\TenProcessedTestObservations.json");
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(),
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            
            var exportClient = new ExportClient(
                exportConfiguration.MongoDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.MongoDbConfiguration.DatabaseName,
                exportConfiguration.MongoDbConfiguration.BatchSize);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstance);
            var filename = FilenameGenerator.CreateFilename("sos_dwc_archive_with_ten_observations");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                new AdvancedFilter(),
                filename,
                processedDarwinCoreRepositoryStub.Object,
                processInfo,
                exportFolderPath,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bool fileExists = System.IO.File.Exists(zipFilePath);
            fileExists.Should().BeTrue("because the zip file should have been generated");
        }
    }
}