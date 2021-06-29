using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Export.IntegrationTests.Managers
{
    public class ObservationManagerIntegrationTests : TestBase
    {
        private ObservationManager CreateObservationManager()
        {
            var exportConfiguration = GetExportConfiguration();
            var elasticClient = new ElasticClient();

            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var vocabularyRepository =
                new VocabularyRepository(exportClient, new NullLogger<VocabularyRepository>());
            var vocabularyValueResolver =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()), 
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()), 
                new FileService(),
                new DataProviderRepository(exportClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DwcArchiveFileWriter>());

            var elasticConfiguration = GetElasticConfiguration();

            var filterManager = new Mock<IFilterManager>();
            filterManager
                .Setup(us => us
                    .PrepareFilter(null, new SearchFilter(), "Sighting", 0, false, false, true)
                );

            var processedPublicObservationRepository = new ProcessedPublicObservationRepository(
                exportClient,
                elasticClient,
                new ElasticSearchConfiguration {IndexPrefix = "test"},
                new Mock<ILogger<ProcessedPublicObservationRepository>>().Object);
            var observationManager = new ObservationManager(
                dwcArchiveFileWriter,
                processedPublicObservationRepository,
                new ProcessInfoRepository(exportClient, elasticConfiguration, new Mock<ILogger<ProcessInfoRepository>>().Object),
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new Mock<IZendToService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path},
                filterManager.Object, 
                new Mock<ILogger<ObservationManager>>().Object);

            return observationManager;
        }

        [Fact(Skip = "Not working")]
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
            var result =
                await observationManager.ExportAndStoreAsync(new SearchFilter(), "Test", "all", "", JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}