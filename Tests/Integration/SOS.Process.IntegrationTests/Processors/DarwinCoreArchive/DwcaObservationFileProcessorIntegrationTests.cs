using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DwC_A;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using Nest;
using Moq;
using SOS.Import.DarwinCore;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Processors;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationFileProcessorIntegrationTests : TestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DwcaObservationFileProcessorIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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
            List<DwcObservationVerbatim> observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var dwcaProcessor = CreateDwcaObservationProcessor(storeProcessedObservations: false, dwcObservationVerbatims: observations);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus = await dwcaProcessor.ProcessAsync(null, taxonByTaxonId, JobCancellationToken.Null);

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
            List<DwcObservationVerbatim> observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var dwcaProcessor = CreateDwcaObservationProcessor(storeProcessedObservations: false, dwcObservationVerbatims: observations);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus = await dwcaProcessor.ProcessAsync(null, taxonByTaxonId, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }


        private DwcaObservationProcessor CreateDwcaObservationProcessor(
            bool storeProcessedObservations,
            List<DwcObservationVerbatim> dwcObservationVerbatims)
        {
            var processConfiguration = GetProcessConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var uris = new Uri[elasticConfiguration.Hosts.Length];
            for (int i = 0; i < uris.Length; i++)
            {
                uris[i] = new Uri(elasticConfiguration.Hosts[i]);
            }
            var elasticClient = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris)));
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
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
            var dwcaVerbatimRepository = new Mock<IDwcaVerbatimRepository>();
            dwcaVerbatimRepository.Setup(m => m.GetAllByCursorAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(mockCursor.Object);
            var invalidObservationRepository = new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, elasticClient, invalidObservationRepository, 
                    new ElasticSearchConfiguration(),  new NullLogger<ProcessedObservationRepository>());
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock().Object;
            }
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            return new DwcaObservationProcessor(
                dwcaVerbatimRepository.Object,
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new AreaHelper(new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()), processedFieldMappingRepository), 
                processConfiguration,
                new NullLogger<DwcaObservationProcessor>());
        }

        private Mock<IProcessedObservationRepository> CreateProcessedObservationRepositoryMock()
        {
            var mock = new Mock<IProcessedObservationRepository>();
            mock.Setup(m => m.DeleteProviderDataAsync(It.IsAny<DataProvider>())).ReturnsAsync(true);
            mock.Setup(m => m.BatchSize).Returns(100000);
            return mock;
        }

        private async Task<IDictionary<int, ProcessedTaxon>> GetTaxonDictionaryAsync()
        {
            var processedTaxonRepository = CreateProcessedTaxonRepository();
            var taxa = await processedTaxonRepository.GetAllAsync();
            return taxa.ToDictionary(taxon => taxon.Id, taxon => taxon);
        }

        private ProcessedTaxonRepository CreateProcessedTaxonRepository()
        {
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            return new ProcessedTaxonRepository(
                processClient,
                new NullLogger<ProcessedTaxonRepository>());
        }
    }
}
