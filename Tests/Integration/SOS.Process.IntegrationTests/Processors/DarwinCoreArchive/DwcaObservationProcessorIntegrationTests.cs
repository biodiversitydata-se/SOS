using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationProcessorIntegrationTests : TestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DwcaObservationProcessorIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Process_Dwca_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcaProcessor = CreateDwcaObservationProcessor(storeProcessedObservations: false);
            var taxonByTaxonId = await GetTaxonDictionaryAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processingStatus = await dwcaProcessor.ProcessAsync(
                taxonByTaxonId, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            processingStatus.Status.Should().Be(RunStatus.Success);
        }

        private DwcaObservationProcessor CreateDwcaObservationProcessor(bool storeProcessedObservations)
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
            var dwcaVerbatimRepository = new DwcaVerbatimRepository(
                verbatimClient,
                new NullLogger<DwcaVerbatimRepository>());
            var invalidObservationRepository = new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessedObservations)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, invalidObservationRepository, new NullLogger<ProcessedObservationRepository>(), elasticClient);
            }
            else
            {
                processedObservationRepository = CreateProcessedObservationRepositoryMock().Object;
            }
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            return new DwcaObservationProcessor(
                dwcaVerbatimRepository,
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
            mock.Setup(m => m.DeleteProviderDataAsync(It.IsAny<ObservationProvider>())).ReturnsAsync(true);
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
