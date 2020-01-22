using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessTaxaJobIntegrationTests : TestBase
    {
        [Fact]
        public async Task Runs_the_AddProcessedTaxaJob()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var taxonVerbatimRepository = new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            var taxonProcessedRepository = new TaxonProcessedRepository(processClient, new NullLogger<TaxonProcessedRepository>());
            var addProcessedTaxaJob = new ProcessTaxaJob(
                taxonVerbatimRepository,
                taxonProcessedRepository,
                new NullLogger<ProcessTaxaJob>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await addProcessedTaxaJob.RunAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

    }
}
