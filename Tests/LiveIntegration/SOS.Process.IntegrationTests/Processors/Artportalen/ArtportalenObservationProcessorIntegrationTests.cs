﻿using Elastic.Clients.Elasticsearch.Cluster;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Export;
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
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AreaRepository = SOS.Lib.Repositories.Resource.AreaRepository;
using TaxonRepository = SOS.Lib.Repositories.Resource.TaxonRepository;

namespace SOS.Process.LiveIntegrationTests.Processors.Artportalen
{
    public class ArtportalenObservationProcessorIntegrationTests : TestBase
    {
        [Fact]
        public async Task Process_Artportalen_observations_with_CSV_writing()
        {
            // Current test
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcArchiveFileWriterCoordinator = CreateDwcArchiveFileWriterCoordinator();
            var artportalenProcessor = CreateArtportalenObservationProcessor(dwcArchiveFileWriterCoordinator, storeProcessedObservations: false, 10000);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();
            var dwcaVocabularyById = await GetDwcaVocabularyByIdAsync();
            var dataProvider = new DataProvider
            {
                Id = 1,
                Identifier = "Artportalen",
                Names = new[] { new VocabularyValueTranslation { CultureCode = "en-GB", Value = "Artportalen" } },
                Type = DataProviderType.ArtportalenObservations
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
            var processingStatus = await artportalenProcessor.ProcessAsync(dataProvider, taxonByTaxonId, dwcaVocabularyById, JobRunModes.Full, JobCancellationToken.Null);
            await dwcArchiveFileWriterCoordinator.CreateDwcaFilesFromCreatedCsvFiles(); // FinishAndWriteDwcaFiles()
            dwcArchiveFileWriterCoordinator.DeleteTemporaryCreatedCsvFiles();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }

        private ArtportalenObservationProcessor CreateArtportalenObservationProcessor(
            DwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            bool storeProcessedObservations,
            int batchSize)
        {
            var processConfiguration = GetProcessConfiguration();
            var exportConfiguration = GetExportConfiguration();
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
            var dwcaVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                new DataProvider { Id = 1, Identifier = "test" },
                verbatimClient,
                new NullLogger<DarwinCoreArchiveVerbatimRepository>());
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var invalidEventRepository =
                new InvalidEventRepository(processClient, new NullLogger<InvalidEventRepository>());
            var validationManager = new ValidationManager(invalidObservationRepository, invalidEventRepository, new NullLogger<ValidationManager>());
            IProcessedObservationCoreRepository processedObservationRepository;
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationCoreRepository(elasticClientManager,
                    new ElasticSearchConfiguration(),
                    new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new MemoryCache(new MemoryCacheOptions()), new NullLogger<ProcessedConfigurationCache>()),
                    new Mock<ITaxonManager>().Object,
                    new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
                    new MemoryCache(new MemoryCacheOptions()),
                    new NullLogger<ProcessedObservationCoreRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock(batchSize).Object;
            }
            IUserObservationRepository userObservationRepository = new Mock<IUserObservationRepository>().Object;

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var datasetRepository = new ArtportalenDatasetMetadataRepository(processClient, new NullLogger<ArtportalenDatasetMetadataRepository>());
            var artportalenVerbatimRepository = new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>());


            var areaHelper = new AreaHelper(
                new AreaConfiguration(),
                new AreaRepository(processClient, new NullLogger<AreaRepository>()));
            var diffusionManager = new DiffusionManager(areaHelper, new NullLogger<DiffusionManager>());
            var processManager = new ProcessManager(processConfiguration);
            var processTimeManager = new ProcessTimeManager(processConfiguration);
            var artportalenConfiguration = GetArtportalenConfiguration();
            var artportalenDataService = new ArtportalenDataService(artportalenConfiguration, new NullLogger<ArtportalenDataService>());
            var sightingRepository = new SightingRepository(artportalenDataService, new NullLogger<SightingRepository>());

            return new ArtportalenObservationProcessor(
                artportalenVerbatimRepository,
                processedObservationRepository,
                vocabularyRepository,
                datasetRepository,
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration()),
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processTimeManager,
                sightingRepository,
                userObservationRepository,
                processConfiguration,
                new NullLogger<ArtportalenObservationProcessor>());
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
            mock.Setup(m => m.WriteBatchSize).Returns(batchSize);
            mock.Setup(m => m.ReadBatchSize).Returns(batchSize);
            return mock;
        }

        private async Task<IDictionary<int, Taxon>> GetTaxonDictionaryAsync()
        {
            var processedTaxonRepository = CreateProcessedTaxonRepository();
            var taxa = await processedTaxonRepository.GetAllAsync();
            return taxa.ToDictionary(taxon => taxon.Id, taxon => taxon);
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
