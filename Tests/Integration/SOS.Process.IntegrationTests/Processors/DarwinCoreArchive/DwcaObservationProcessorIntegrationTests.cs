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
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Process.Processors.DarwinCoreArchive;
using Xunit;
using Xunit.Abstractions;

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
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Swedish Butterfly Monitoring Scheme (SeBMS)" } },
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
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Swedish Butterfly Monitoring Scheme (SeBMS)" } },
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
            var dwcaVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                verbatimClient,
                new NullLogger<DarwinCoreArchiveVerbatimRepository>());
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var validationManager = new ValidationManager(invalidObservationRepository, new NullLogger<ValidationManager>());
            IProcessedPublicObservationRepository processedObservationRepository;
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedPublicObservationRepository(processClient, elasticClient,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedPublicObservationRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock(batchSize).Object;
            }

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());

            return new DwcaObservationProcessor(
                dwcaVerbatimRepository,
                processedObservationRepository,
                vocabularyRepository,
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration()),
                new AreaHelper(new AreaRepository(processClient, new NullLogger<AreaRepository>())),
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
            var vocabularyRepository =
                new VocabularyRepository(exportClient, new NullLogger<VocabularyRepository>());
            var fieldMappingResolverHelper =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());

            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    fieldMappingResolverHelper,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new FileService(),
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" }, new NullLogger<DwcArchiveFileWriterCoordinator>());
            return dwcArchiveFileWriterCoordinator;
        }

        private Mock<IProcessedPublicObservationRepository> CreateProcessedObservationRepositoryMock(int batchSize)
        {
            var mock = new Mock<IProcessedPublicObservationRepository>();
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

        private TaxonRepository CreateProcessedTaxonRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            return new TaxonRepository(
                processClient,
                new NullLogger<TaxonRepository>());
        }
    }
}