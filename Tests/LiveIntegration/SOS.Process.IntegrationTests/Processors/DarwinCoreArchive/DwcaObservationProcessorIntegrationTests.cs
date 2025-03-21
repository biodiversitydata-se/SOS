﻿using Elastic.Clients.Elasticsearch.Cluster;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Process.LiveIntegrationTests.Processors.DarwinCoreArchive
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
            var dwcaVocabularyById = await GetDwcaVocabularyByIdAsync();
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
                await dwcaProcessor.ProcessAsync(dataProvider, taxonByTaxonId, dwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);

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
            var dwcaVocabularyById = await GetDwcaVocabularyByIdAsync();
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
            var processingStatus = await dwcaProcessor.ProcessAsync(dataProvider, taxonByTaxonId, dwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);
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
            var elasticConfiguration = GetElasticConfiguration();
            var elasticClientManager = new ElasticClientManager(elasticConfiguration);
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
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var invalidEventRepository =
                new InvalidEventRepository(processClient, new NullLogger<InvalidEventRepository>());
            var processManager = new ProcessManager(processConfiguration);
            var validationManager = new ValidationManager(invalidObservationRepository, invalidEventRepository, new NullLogger<ValidationManager>());
            var areaHelper = new AreaHelper(new AreaConfiguration(), new AreaRepository(processClient, new NullLogger<AreaRepository>()));
            var diffusionManager = new DiffusionManager(areaHelper, new NullLogger<DiffusionManager>());
            var processTimeManager = new ProcessTimeManager(processConfiguration);
            IProcessedObservationCoreRepository processedObservationRepository;

            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationCoreRepository(elasticClientManager,
                    new ElasticSearchConfiguration(),
                    new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new MemoryCache(new MemoryCacheOptions()),new NullLogger<ProcessedConfigurationCache>()),
                    new Mock<ITaxonManager>().Object,
                    new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
                    new NullLogger<ProcessedObservationCoreRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock(batchSize).Object;
            }

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());

            var dwcaConfiguration = new DwcaConfiguration()
            {
                BatchSize = 5000,
                ImportPath = @"C:\Temp"
            };

            return new DwcaObservationProcessor(
                verbatimClient,
                processedObservationRepository,                
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

        private DwcArchiveFileWriterCoordinator CreateDwcArchiveFileWriterCoordinator()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var verbatimClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var vocabularyValueResolver =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());


            var dataProviderRepository =
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>());
            var extendedMeasurementOrFactCsvWriter = new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>());
            var simpleMultimediaCsvWriter = new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>());
            var fileService = new FileService();

            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(
                new DwcArchiveFileWriter(
                    new DwcArchiveOccurrenceCsvWriter(
                        vocabularyValueResolver,
                        new NullLogger<DwcArchiveOccurrenceCsvWriter>()
                    ),
                    extendedMeasurementOrFactCsvWriter,
                    simpleMultimediaCsvWriter,
                    fileService,
                    dataProviderRepository,
                    new NullLogger<DwcArchiveFileWriter>()
                ),
                new DwcArchiveEventFileWriter(
                    new DwcArchiveOccurrenceCsvWriter(
                        vocabularyValueResolver,
                        new NullLogger<DwcArchiveOccurrenceCsvWriter>()
                    ),
                    new DwcArchiveEventCsvWriter(vocabularyValueResolver, new NullLogger<DwcArchiveEventCsvWriter>()),
                    extendedMeasurementOrFactCsvWriter,
                    simpleMultimediaCsvWriter,
                    dataProviderRepository,
                    fileService,
                    new NullLogger<DwcArchiveEventFileWriter>()
                ),
                fileService,
                dataProviderRepository,
                verbatimClient,
                new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" },
                new NullLogger<DwcArchiveFileWriterCoordinator>()
            );
            return dwcArchiveFileWriterCoordinator;
        }

        private Mock<IProcessedObservationCoreRepository> CreateProcessedObservationRepositoryMock(int batchSize)
        {
            var mock = new Mock<IProcessedObservationCoreRepository>();
            mock.Setup(m => m.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>())).ReturnsAsync(true);
            mock.Setup(m => m.ReadBatchSize).Returns(batchSize);
            mock.Setup(m => m.WriteBatchSize).Returns(batchSize);
            return mock;
        }

        private async Task<IDictionary<VocabularyId, IDictionary<object, int>>> GetDwcaVocabularyByIdAsync()
        {
            var vocabularyRepository = CreateVocabularyRepository();
            var vocabularies = await vocabularyRepository.GetAllAsync();
            var vocabularyById = VocabularyHelper.GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                vocabularies.ToArray(),
                true);
            return vocabularyById;
        }

        private VocabularyRepository CreateVocabularyRepository()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);
            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            return vocabularyRepository;
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