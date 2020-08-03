using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessTaxaJobIntegrationTests : TestBase
    {
        private ProcessTaxaJob CreateProcessTaxaJob()
        {
            var processConfiguration = GetProcessConfiguration();
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
            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            var taxonProcessedRepository =
                new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            var harvestInfoRepository =
                new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var processInfoRepository =
                new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var processTaxaJob = new ProcessTaxaJob(
                taxonVerbatimRepository,
                taxonProcessedRepository,
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