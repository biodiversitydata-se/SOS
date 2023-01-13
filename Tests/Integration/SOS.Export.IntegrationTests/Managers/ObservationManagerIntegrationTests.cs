using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Export.Managers;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Services;
using Xunit;
using SOS.Lib.IO.DwcArchive.Interfaces;

namespace SOS.Export.IntegrationTests.Managers
{
    public class ObservationManagerIntegrationTests : TestBase
    {
        private ObservationManager CreateObservationManager()
        {
            var exportConfiguration = GetExportConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var elasticClientManager = new ElasticClientManager(elasticConfiguration);

            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyValueResolver =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());
            var dwcArchiveFileWriter = new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()), 
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()), 
                new FileService(),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DwcArchiveFileWriter>());

            var dwcArchiveEventFileWriter = new DwcArchiveEventFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new DwcArchiveEventCsvWriter(vocabularyValueResolver, new NullLogger<DwcArchiveEventCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new FileService(),
                new NullLogger<DwcArchiveEventFileWriter>());


            var processedConfigurationRepository = new ProcessedConfigurationRepository(processClient,
                new NullLogger<ProcessedConfigurationRepository>());

            var processedObservationRepository = new ProcessedObservationCoreRepository(
                elasticClientManager,
                elasticConfiguration,
                new ProcessedConfigurationCache(processedConfigurationRepository),
                new TelemetryClient(),
                new Mock<ILogger<ProcessedObservationCoreRepository>>().Object);

            var excelWriter = new ExcelFileWriter(processedObservationRepository, new FileService(), vocabularyValueResolver,
                new NullLogger<ExcelFileWriter>());
            var geoJsonlWriter = new GeoJsonFileWriter(processedObservationRepository, new FileService(), vocabularyValueResolver,
                new NullLogger<GeoJsonFileWriter>());
            var csvWriter = new CsvFileWriter(processedObservationRepository, new FileService(), vocabularyValueResolver,
                new NullLogger<CsvFileWriter>());

            var filterManager = new Mock<IFilterManager>();
            filterManager
                .Setup(us => us
                    .PrepareFilterAsync(0, null, new SearchFilter(0, ProtectionFilter.Public), "Sighting", 0, false, false, true)
                );

            var observationManager = new ObservationManager(
                dwcArchiveFileWriter,
                dwcArchiveEventFileWriter,
                excelWriter,
                geoJsonlWriter,
                csvWriter,
                processedObservationRepository,
                new ProcessInfoRepository(processClient, new Mock<ILogger<ProcessInfoRepository>>().Object),
                new FileService(),
                new Mock<IBlobStorageService>().Object,
                new Mock<IZendToService>().Object,
                new FileDestination { Path = exportConfiguration.FileDestination.Path },
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
                await observationManager.ExportAndStoreAsync(new SearchFilter(0), "Test", "all", "", JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        [Fact(Skip = "Not working")]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task Create_GeoJson_file_for_AP_and_delete_the_file_afterwards()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = CreateObservationManager();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result =
                await observationManager.ExportAndSendAsync(null, null, 
                    new SearchFilter(0)
                    {
                        DataProviderIds = new List<int> { 1 },
                        Output = new OutputFilter
                        {
                            Fields = new [] {
                                "datasetName",
                                "event.startDate",
                                "event.endDate",
                                "identification.verified",
                                "location.decimalLongitude",
                                "location.decimalLatitude",
                                "occurrence.occurrenceId",
                                "occurrence.reportedBy",
                                "taxon.id",
                                "taxon.scientificName",
                                "taxon.vernacularName"
                            }
                        }
                    }, 
                    "mats.lindgren@slu.se", 
                    "AP", 
                    ExportFormat.GeoJson, 
                    "en-GB",
                    false,
                    PropertyLabelType.PropertyPath,
                    false,
                    false,
                    false,
                    null,
                    JobCancellationToken.Null
                );

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "DwcArchiveIntegration")]
        public async Task Create_Excel_file_for_AP_and_delete_the_file_afterwards()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = CreateObservationManager();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result =
                await observationManager.ExportAndSendAsync(null, null, new SearchFilter(0)
                {
                    DataProviderIds = new List<int> { 1 },
                    Output = new OutputFilter
                    {
                        Fields = new[] {
                            "datasetName",
                            "event.startDate",
                            "event.endDate",
                            "identification.verified",
                            "location.decimalLongitude",
                            "location.decimalLatitude",
                            "occurrence.occurrenceId",
                            "occurrence.reportedBy",
                            "taxon.id",
                            "taxon.scientificName",
                            "taxon.vernacularName"
                        }
                    }
                }, "mats.lindgren@slu.se", "AP", ExportFormat.Excel, "en-GB", false,  PropertyLabelType.PropertyPath, false, false,
                    false,
                    null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Success.Should().BeTrue();
        }
    }
}