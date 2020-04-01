using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Export.IntegrationTests.TestHelpers.Factories;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Models.Search;
using SOS.TestHelpers.IO;
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
            var exportClient = CreateExportClient(exportConfiguration);
            var processedObservationRepository = CreateProcessedObservationRepository(exportClient);
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(exportClient);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.CollectionName);
            var filename = FilenameGenerator.CreateFilename("sos_dwc_archive_with_all_data");
            //var filter = new AdvancedFilter();
            var filter = new SearchFilter {TaxonIds = new[] {102951}};

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                filter,
                filename,
                processedObservationRepository,
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
            var exportClient = CreateExportClient(exportConfiguration);
            string exportFolderPath = exportConfiguration.FileDestination.Path;
            var processedDarwinCoreRepositoryStub = ProcessedDarwinCoreRepositoryStubFactory.Create(@"Resources\TenProcessedTestObservations.json");
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(exportClient);
            var processInfoRepository = new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.CollectionName);
            var filename = FilenameGenerator.CreateFilename("sos_dwc_archive_with_ten_observations");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var zipFilePath = await dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                new SearchFilter(),
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

        private static ExportClient CreateExportClient(ExportConfiguration exportConfiguration)
        {
            var exportClient = new ExportClient(
                exportConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.ProcessedDbConfiguration.DatabaseName,
                exportConfiguration.ProcessedDbConfiguration.BatchSize);
            return exportClient;
        }

        private static DwcArchiveFileWriter CreateDwcArchiveFileWriter(ExportClient exportClient)
        {
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(
                exportClient,
                new NullLogger<ProcessedFieldMappingRepository>());
            DwcArchiveFileWriter dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    processedFieldMappingRepository,
                    new TaxonManager(
                        new ProcessedTaxonRepository(exportClient, new Mock<ILogger<ProcessedTaxonRepository>>().Object), new Mock<ILogger<TaxonManager>>().Object),
                    new Mock<ILogger<DwcArchiveOccurrenceCsvWriter>>().Object),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(),
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            return dwcArchiveFileWriter;
        }

        private static ProcessedObservationRepository CreateProcessedObservationRepository(ExportClient exportClient)
        {
          /*  var processedObservationRepository = new ProcessedObservationRepository(
                exportClient,
                new TaxonManager(
                    new ProcessedTaxonRepository(
                        exportClient,
                        new Mock<ILogger<ProcessedTaxonRepository>>().Object),
                    new Mock<ILogger<TaxonManager>>().Object),
                new Mock<ILogger<ProcessedObservationRepository>>().Object);
            return processedObservationRepository;*/

          return null;
        }
    }
}