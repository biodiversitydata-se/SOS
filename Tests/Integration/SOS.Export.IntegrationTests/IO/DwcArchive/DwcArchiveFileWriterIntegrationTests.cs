using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Export.IntegrationTests.TestHelpers.Factories;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Services;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed;
using Xunit;

namespace SOS.Export.IntegrationTests.IO.DwcArchive
{
    public class DwcArchiveFileWriterIntegrationTests : TestBase
    {
        private ProcessClient CreateExportClient(MongoDbConfiguration exportConfiguration)
        {
            var exportClient = new ProcessClient(
                exportConfiguration.GetMongoDbSettings(),
                exportConfiguration.DatabaseName,
                exportConfiguration.ReadBatchSize,
                exportConfiguration.WriteBatchSize);
            return exportClient;
        }

        private DwcArchiveFileWriter CreateDwcArchiveFileWriter(ProcessClient exportClient)
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    CreateFieldMappingResolverHelper(CreateExportClient(processDbConfiguration)),
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new Mock<ILogger<ExtendedMeasurementOrFactCsvWriter>>().Object),
                new FileService(),
                new Mock<ILogger<DwcArchiveFileWriter>>().Object);
            return dwcArchiveFileWriter;
        }

        private FieldMappingResolverHelper CreateFieldMappingResolverHelper(ProcessClient client)
        {
            var processedFieldMappingRepository =
                new Lib.Repositories.Processed.ProcessedFieldMappingRepository(client, new NullLogger<Lib.Repositories.Processed.ProcessedFieldMappingRepository>());
            return new FieldMappingResolverHelper(processedFieldMappingRepository,
                new FieldMappingConfiguration { LocalizationCultureCode = "sv-SE", ResolveValues = true });
        }


        private static ProcessedObservationRepository CreateProcessedObservationRepository(ProcessClient exportClient)
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

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task
            Creates_a_DwcArchiveFile_with_all_processed_data_in_MongoDb_instance_and_saves_the_file_on_disk()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var exportConfiguration = GetExportConfiguration();
            var exportFolderPath = exportConfiguration.FileDestination.Path;
            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = CreateExportClient(processDbConfiguration);
            var processedObservationRepository = CreateProcessedObservationRepository(exportClient);
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(exportClient);
            var processInfoRepository =
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstanceName);
            var filename = FilenameHelper.CreateFilenameWithDate("sos_dwc_archive_with_all_data");
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
            var fileExists = File.Exists(zipFilePath);
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
            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = CreateExportClient(processDbConfiguration);
            var exportConfiguration = GetExportConfiguration();
            var exportFolderPath = exportConfiguration.FileDestination.Path;
            var processedDarwinCoreRepositoryStub =
                ProcessedDarwinCoreRepositoryStubFactory.Create(@"Resources\TenProcessedTestObservations.json");
            var dwcArchiveFileWriter = CreateDwcArchiveFileWriter(exportClient);
            var processInfoRepository =
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object);
            var processInfo = await processInfoRepository.GetAsync(processInfoRepository.ActiveInstanceName);
            var filename = FilenameHelper.CreateFilenameWithDate("sos_dwc_archive_with_ten_observations");

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
            var fileExists = File.Exists(zipFilePath);
            fileExists.Should().BeTrue("because the zip file should have been generated");
        }
    }
}