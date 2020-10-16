using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Managers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Repositories.Source;
using Xunit;
using Xunit.Abstractions;
using TaxonManager = SOS.Export.Managers.TaxonManager;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationProcessorIntegrationTests : TestBase
    {
        public DwcaObservationProcessorIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [Fact]
        public async Task Process_Dwca_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveFileWriterCoordinator = CreateDwcArchiveFileWriterCoordinator();
            var dwcaProcessor = CreateDwcaObservationProcessor(dwcArchiveFileWriterCoordinator, storeProcessedObservations: false, batchSize: 10000);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();
            var dataProvider = new DataProvider
            {
                Id = 13,
                Identifier = "ButterflyMonitoring",
                Name = "Swedish Butterfly Monitoring Scheme (SeBMS)",
                Type = DataProviderType.DwcA
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus =
                await dwcaProcessor.ProcessAsync(dataProvider, taxonByTaxonId, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Process_Dwca_observations_with_CSV_writing()
        {
            // Current test
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveFileWriterCoordinator = CreateDwcArchiveFileWriterCoordinator();
            var dwcaProcessor = CreateDwcaObservationProcessor(dwcArchiveFileWriterCoordinator, storeProcessedObservations: false, 10000);
            
            var taxonByTaxonId = await GetTaxonDictionaryAsync();
            var dataProvider = new DataProvider
            {
                Id = 13,
                Identifier = "ButterflyMonitoring",
                Name = "Swedish Butterfly Monitoring Scheme (SeBMS)",
                Type = DataProviderType.DwcA
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
            var processingStatus = await dwcaProcessor.ProcessAsync(dataProvider, taxonByTaxonId, JobRunModes.Full, JobCancellationToken.Null);
            await dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles(); // FinishAndWriteDwcaFiles()
            dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }

        private DwcaObservationProcessor CreateDwcaObservationProcessor(
            DwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            bool storeProcessedObservations,
            int batchSize)
        {
            var processConfiguration = GetProcessConfiguration();
            var exportConfiguration = GetExportConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var uris = new Uri[elasticConfiguration.Hosts.Length];
            for (var i = 0; i < uris.Length; i++)
            {
                uris[i] = new Uri(elasticConfiguration.Hosts[i]);
            }

            var elasticClient = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris)));
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var verbatimClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var dwcaVerbatimRepository = new DwcaVerbatimRepository(
                verbatimClient,
                new NullLogger<DwcaVerbatimRepository>());
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var validationManager = new ValidationManager(invalidObservationRepository, new NullLogger<ValidationManager>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, elasticClient,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedObservationRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock(batchSize).Object;
            }

            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());

            return new DwcaObservationProcessor(
                dwcaVerbatimRepository,
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new AreaHelper(new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                    processedFieldMappingRepository),
                processConfiguration, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<DwcaObservationProcessor>());
        }

        private DwcArchiveFileWriterCoordinator CreateDwcArchiveFileWriterCoordinator()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var exportClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(exportClient, new NullLogger<ProcessedFieldMappingRepository>());
            var fieldMappingResolverHelper =
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration());

            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    fieldMappingResolverHelper,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new FileService(),
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" }, new NullLogger<DwcArchiveFileWriterCoordinator>());
            return dwcArchiveFileWriterCoordinator;
        }

        private Mock<IProcessedObservationRepository> CreateProcessedObservationRepositoryMock(int batchSize)
        {
            var mock = new Mock<IProcessedObservationRepository>();
            mock.Setup(m => m.DeleteProviderDataAsync(It.IsAny<DataProvider>())).ReturnsAsync(true);
            mock.Setup(m => m.BatchSize).Returns(batchSize);
            return mock;
        }

        private async Task<IDictionary<int, Taxon>> GetTaxonDictionaryAsync()
        {
            var processedTaxonRepository = CreateProcessedTaxonRepository();
            var taxa = await processedTaxonRepository.GetAllAsync();
            return taxa.ToDictionary(taxon => taxon.Id, taxon => taxon);
        }

        private ProcessedTaxonRepository CreateProcessedTaxonRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            return new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessedTaxonRepository>());
        }
    }
}