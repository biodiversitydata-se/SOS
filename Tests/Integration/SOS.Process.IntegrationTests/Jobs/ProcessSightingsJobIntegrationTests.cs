using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Database;
using SOS.Process.Factories;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessSightingsJobIntegrationTests : TestBase
    {
        [Fact]
        public async Task Run_the_process_Artportalen_job()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processJob = CreateProcessJob();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processJob.RunAsync(
                (int)DataProvider.Artdatabanken, 
                false, 
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private ProcessJob CreateProcessJob()
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
            var areaHelper = new AreaHelper(new AreaVerbatimRepository(verbatimClient, new NullLogger<AreaVerbatimRepository>()));
            var taxonProcessedRepository = new TaxonProcessedRepository(processClient, new NullLogger<TaxonProcessedRepository>());
            var inadequateItemRepository = new InadequateItemRepository(processClient, new NullLogger<InadequateItemRepository>());
            var processedSightingRepository = new ProcessedSightingRepository(processClient, inadequateItemRepository, new NullLogger<ProcessedSightingRepository>());
            var processInfoRepository = new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var harvestInfoRepository = new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var clamPortalProcessFactory = new ClamPortalProcessFactory(
                new ClamObservationVerbatimRepository(verbatimClient, new NullLogger<ClamObservationVerbatimRepository>()), 
                areaHelper, 
                processedSightingRepository, 
                new NullLogger<ClamPortalProcessFactory>());
            var kulProcessFactory = new KulProcessFactory(
                new KulObservationVerbatimRepository(verbatimClient, new NullLogger<KulObservationVerbatimRepository>()), 
                areaHelper,
                processedSightingRepository,
                new NullLogger<KulProcessFactory>());
            var speciesPortalProcessFactory = new SpeciesPortalProcessFactory(
                new SpeciesPortalVerbatimRepository(verbatimClient, new NullLogger<SpeciesPortalVerbatimRepository>()),
                processedSightingRepository,
                new NullLogger<SpeciesPortalProcessFactory>());
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());

            var processTaxaJob = new ProcessJob(
                processedSightingRepository,
                processInfoRepository,
                harvestInfoRepository,
                clamPortalProcessFactory,
                kulProcessFactory,
                speciesPortalProcessFactory,
                taxonProcessedRepository,
                processedFieldMappingRepository,
                areaHelper,
                new NullLogger<ProcessJob>());

            return processTaxaJob;
        }
    }
}
