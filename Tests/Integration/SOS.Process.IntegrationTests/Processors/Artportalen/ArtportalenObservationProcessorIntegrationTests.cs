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
using SOS.Process.Processors.Artportalen;
using Xunit;

namespace SOS.Process.IntegrationTests.Processors.Artportalen
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
            var dataProvider = new DataProvider
            {
                Id = 1,
                Identifier = "Artportalen",
                Name = "Artportalen",
                Type = DataProviderType.ArtportalenObservations
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            dwcArchiveFileWriterCoordinator.BeginWriteDwcCsvFiles();
            var processingStatus = await artportalenProcessor.ProcessAsync(dataProvider, taxonByTaxonId, JobRunModes.Full, JobCancellationToken.Null);
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
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var artportalenVerbatimRepository = new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>());

            return new ArtportalenObservationProcessor(
                artportalenVerbatimRepository,
                processedObservationRepository,
                processedFieldMappingRepository,
                new VocabularyValueResolver(processedFieldMappingRepository, new VocabularyConfiguration()),
                processConfiguration, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<ArtportalenObservationProcessor>());
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
                new VocabularyRepository(exportClient, new NullLogger<VocabularyRepository>());
            var fieldMappingResolverHelper =
                new VocabularyValueResolver(processedFieldMappingRepository, new VocabularyConfiguration());
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
