using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Jobs;
using SOS.Harvest.Processors.Taxon;
using SOS.Harvest.Services.Taxon;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Process.LiveIntegrationTests.Jobs
{
    public class ProcessTaxaJobIntegrationTests : TestBase
    {
        private ProcessTaxaJob CreateProcessTaxaJob()
        {
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
            var elasticConfiguration = GetElasticConfiguration();

            var taxonServiceConfiguration = new TaxonServiceConfiguration()
            {
                AcceptHeaderContentType = "application/text",
                BaseAddress = "https://taxonapi.artdata.slu.se/darwincore/download?version=custom"
            };
            var taxonService = new TaxonService(new TaxonServiceProxy(), taxonServiceConfiguration, new NullLogger<TaxonService>());
            var taxonAttributeService = new TaxonAttributeService(new HttpClientService(new NullLogger<HttpClientService>()), new TaxonAttributeServiceConfiguration()
            {
                AcceptHeaderContentType = "application/json",
                BaseAddress = "https://taxonattributeservice.artdata.slu.se/api"
            }, new NullLogger<TaxonAttributeService>());
            var processedTaxonRepositoryMock = new Mock<ITaxonRepository>();
            var apTaxonRepositoryMock = new Mock<Harvest.Repositories.Source.Artportalen.Interfaces.ITaxonRepository>();
            var processConfiguration = new ProcessConfiguration() { NoOfThreads = 1 };
            var taxonProcessor = new TaxonProcessor(taxonService, taxonAttributeService, processedTaxonRepositoryMock.Object, apTaxonRepositoryMock.Object,  processConfiguration, new NullLogger<TaxonProcessor>());

            var harvestInfoRepository =
                new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());

            var processInfoRepository =
                new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var processTaxaJob = new ProcessTaxaJob(
                taxonProcessor,
                harvestInfoRepository,
                processInfoRepository,
                new NullLogger<ProcessTaxaJob>());

            return processTaxaJob;
        }

        [Fact]
        public async Task Runs_the_ProcessTaxaJob()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processTaxaJob = CreateProcessTaxaJob();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processTaxaJob.RunAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}