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
    public class CopyFieldMappingsJobIntegrationTests : TestBase
    {
        private CopyFieldMappingsJob CreateCopyFieldMappingsJob()
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
            var fieldMappingVerbatimRepository =
                new FieldMappingVerbatimRepository(verbatimClient, new NullLogger<FieldMappingVerbatimRepository>());
            var fieldMappingProcessedRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var harvestInfoRepository =
                new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var processInfoRepository =
                new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());

            var copyFieldMappingsJob = new CopyFieldMappingsJob(
                fieldMappingVerbatimRepository,
                fieldMappingProcessedRepository,
                harvestInfoRepository,
                processInfoRepository,
                new NullLogger<CopyFieldMappingsJob>());

            return copyFieldMappingsJob;
        }

        [Fact]
        public async Task Runs_the_CopyFieldMappingsJob()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var copyFieldMappingsJob = CreateCopyFieldMappingsJob();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await copyFieldMappingsJob.RunAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}