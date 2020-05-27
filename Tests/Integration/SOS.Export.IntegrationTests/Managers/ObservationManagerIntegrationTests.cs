using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.MongoDb;
using SOS.Export.Repositories;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using Xunit;

namespace SOS.Export.IntegrationTests.Managers
{
    public class ObservationManagerIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task Create_DwcArchive_file_for_all_observations_and_delete_the_file_afterwards()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = CreateObservationManager();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result = await observationManager.ExportAndStoreAsync(null, "Test", "all", false, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private ObservationManager CreateObservationManager()
        {
            var exportConfiguration = GetExportConfiguration();
            var elasticClient = new ElasticClient();
            var exportClient = new ExportClient(
                exportConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.ProcessedDbConfiguration.DatabaseName,
                exportConfiguration.ProcessedDbConfiguration.BatchSize);
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new ProcessedFieldMappingRepository(exportClient, new NullLogger<ProcessedFieldMappingRepository>()),
                    new TaxonManager(
                        new ProcessedTaxonRepository(exportClient, new Mock<ILogger<ProcessedTaxonRepository>>().Object), new Mock<ILogger<TaxonManager>>().Object),
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new FileService(),
                new NullLogger<DwcArchiveFileWriter>());
            ObservationManager observationManager = new ObservationManager(
                new DOIRepository(exportClient, new Mock<ILogger<DOIRepository>>().Object), 
                dwcArchiveFileWriter,
                new ProcessedObservationRepository(
                    elasticClient, 
                    exportClient,
                    new ElasticSearchConfiguration(), 
                    new Mock<ILogger<ProcessedObservationRepository>>().Object),
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object),
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new Mock<IZendToService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path },
                new Mock<ILogger<ObservationManager>>().Object);

            return observationManager;
        }
    }
}
