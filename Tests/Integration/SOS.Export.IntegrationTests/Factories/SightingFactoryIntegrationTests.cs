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
        public async Task Create_DwcArchive_file_for_all_observations_and_delete_the_file_afterwards()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = CreateSightingFactory();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            bool result = await sightingFactory.ExportAllAsync(JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private SightingFactory CreateSightingFactory()
        {
            var exportConfiguration = GetExportConfiguration();
            var exportClient = new ExportClient(
                exportConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                exportConfiguration.ProcessedDbConfiguration.DatabaseName,
                exportConfiguration.ProcessedDbConfiguration.BatchSize);
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new ProcessedFieldMappingRepository(exportClient, new NullLogger<ProcessedFieldMappingRepository>()), 
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new FileService(),
                new NullLogger<DwcArchiveFileWriter>());
            SightingFactory sightingFactory = new SightingFactory(
                dwcArchiveFileWriter,
                new ProcessedSightingRepository(
                    exportClient,
                    new TaxonFactory(
                        new ProcessedTaxonRepository(exportClient, new Mock<ILogger<ProcessedTaxonRepository>>().Object), new Mock<ILogger<TaxonFactory>>().Object),
                    new Mock<ILogger<ProcessedSightingRepository>>().Object),
                new ProcessInfoRepository(exportClient, new Mock<ILogger<ProcessInfoRepository>>().Object),
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path },
                new Mock<ILogger<SightingFactory>>().Object);

            return sightingFactory;
        }
    }
}
