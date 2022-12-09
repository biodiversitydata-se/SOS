using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using SOS.Harvest.DarwinCore;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.DarwinCoreArchive;
using Xunit;
using Xunit.Abstractions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationFileProcessorIntegrationTests : TestBase
    {
        public DwcaObservationFileProcessorIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;


        private DwcaObservationProcessor CreateDwcaObservationProcessor(
            bool storeProcessedObservations,
            IEnumerable<DwcObservationVerbatim> dwcObservationVerbatims)
        {
            var processConfiguration = GetProcessConfiguration();
            var elasticConfiguration = GetElasticConfiguration();

            var elasticClientManager = new ElasticClientManager(elasticConfiguration);
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var mockCursor = new Mock<IAsyncCursor<DwcObservationVerbatim>>();
            mockCursor.Setup(_ => _.Current).Returns(dwcObservationVerbatims); //<-- Note the entities here
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            var verbatimClient = new Mock<VerbatimClient>();
           
            var dwcaVerbatimRepository = new Mock<DarwinCoreArchiveVerbatimRepository>();
            dwcaVerbatimRepository.Setup(m => m.GetAllByCursorAsync())
                .ReturnsAsync(mockCursor.Object);
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var processManager = new ProcessManager(processConfiguration);
            var validationManager = new ValidationManager(invalidObservationRepository, new NullLogger<ValidationManager>());
            var areaHelper = new AreaHelper(new AreaRepository(processClient, new NullLogger<AreaRepository>()));
            var diffusionManager = new DiffusionManager(areaHelper, new NullLogger<DiffusionManager>());
            IProcessedObservationCoreRepository processedObservationRepository;
            var processTimeManager = new ProcessTimeManager(processConfiguration);
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationCoreRepository(elasticClientManager,
                    new ElasticSearchConfiguration(),
                    new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
                    new NullLogger<ProcessedObservationCoreRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock().Object;
            }

            var dataProviderRepository =
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>());

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyValueResolver =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());
            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new FileService(),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), dataProviderRepository, importClient, new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" }, new NullLogger<DwcArchiveFileWriterCoordinator>());
            var dwcaConfiguration = new DwcaConfiguration()
            {
                BatchSize = 5000,
                ImportPath = @"C:\Temp",
                UseDwcaCollectionRepository = true
            };

            return new DwcaObservationProcessor(
                verbatimClient.Object,
                processedObservationRepository,
                vocabularyRepository,
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration()),
                areaHelper,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processTimeManager,
                processConfiguration,
                dwcaConfiguration,
                new NullLogger<DwcaObservationProcessor>());
        }

        private Mock<IProcessedObservationCoreRepository> CreateProcessedObservationRepositoryMock()
        {
            var mock = new Mock<IProcessedObservationCoreRepository>();
            mock.Setup(m => m.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>())).ReturnsAsync(true);
            mock.Setup(m => m.ReadBatchSize).Returns(10000);
            mock.Setup(m => m.WriteBatchSize).Returns(1000);
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

        [Fact]
        public async Task Process_Dwca_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 100,
                Identifier = "TestButterflyMonitoring"
            };
            var dwcaReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var dwcaProcessor = CreateDwcaObservationProcessor(false, observations);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus = await dwcaProcessor.ProcessAsync(null, taxonByTaxonId, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Process_SHARK_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip";
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 101,
                Identifier = "TestSHARK"
            };
            var dwcaReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var dwcaProcessor = CreateDwcaObservationProcessor(false, observations);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus = await dwcaProcessor.ProcessAsync(null, taxonByTaxonId, JobRunModes.Full, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }
    }
}