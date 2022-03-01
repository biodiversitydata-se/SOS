using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Database;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Verbatim;
using SOS.Harvest.Jobs;
using SOS.Harvest.Processors.Taxon;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
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

            var taxonProcessor = new TaxonProcessor(null, null, null, null, null); //Todo

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