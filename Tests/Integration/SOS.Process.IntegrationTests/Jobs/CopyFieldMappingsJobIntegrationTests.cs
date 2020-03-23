using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Enums;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class CopyFieldMappingsJobIntegrationTests : TestBase
    {
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

        private CopyFieldMappingsJob CreateCopyFieldMappingsJob()
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var fieldMappingVerbatimRepository = new FieldMappingVerbatimRepository(verbatimClient, new NullLogger<FieldMappingVerbatimRepository>());
            var fieldMappingProcessedRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var harvestInfoRepository = new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var processInfoRepository = new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());

            var copyFieldMappingsJob = new CopyFieldMappingsJob(
                fieldMappingVerbatimRepository,
                fieldMappingProcessedRepository,
                harvestInfoRepository,
                processInfoRepository,
                new NullLogger<CopyFieldMappingsJob>());

            return copyFieldMappingsJob;
        }
    }
}
