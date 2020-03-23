using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.Kul;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessObservationsJobIntegrationTests : TestBase
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
                (int)ObservationProvider.Artportalen, 
                false,
                false, 
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
            var areaHelper = new AreaHelper(
                new AreaVerbatimRepository(verbatimClient, new NullLogger<AreaVerbatimRepository>()), 
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>()));
            var taxonVerbatimRepository = new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            var fieldMappingVerbatimRepository = new FieldMappingVerbatimRepository(verbatimClient, new NullLogger<FieldMappingVerbatimRepository>());
            var taxonProcessedRepository = new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            var invalidObservationRepository = new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var processedObservationRepository = new ProcessedObservationRepository(processClient, invalidObservationRepository, new NullLogger<ProcessedObservationRepository>());
            var processInfoRepository = new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var harvestInfoRepository = new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var clamPortalProcessor = new ClamPortalObservationProcessor(
                new ClamObservationVerbatimRepository(verbatimClient, new NullLogger<ClamObservationVerbatimRepository>()), 
                areaHelper, 
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                new NullLogger<ClamPortalObservationProcessor>());
            var kulProcessor = new KulObservationProcessor(
                new KulObservationVerbatimRepository(verbatimClient, new NullLogger<KulObservationVerbatimRepository>()), 
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new NullLogger<KulObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository, 
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                processConfiguration,
                new NullLogger<ArtportalenObservationProcessor>());
            var instanceManager = new InstanceManager(
                new ProcessedObservationRepository(processClient, invalidObservationRepository, new NullLogger<ProcessedObservationRepository>()),
                processInfoRepository,
                new NullLogger<InstanceManager>());

            var copyFieldMappingsJob = new CopyFieldMappingsJob(fieldMappingVerbatimRepository, processedFieldMappingRepository, harvestInfoRepository, processInfoRepository, new NullLogger<CopyFieldMappingsJob>());
            var processTaxaJob = new ProcessTaxaJob(taxonVerbatimRepository, taxonProcessedRepository, harvestInfoRepository, processInfoRepository, new NullLogger<ProcessTaxaJob>());
            
            var processJob = new ProcessJob(
                processedObservationRepository,
                processInfoRepository,
                harvestInfoRepository,
                clamPortalProcessor,
                kulProcessor,
                artportalenProcessor,
                taxonProcessedRepository,
                instanceManager,
                copyFieldMappingsJob,
                processTaxaJob,
                areaHelper,
                new NullLogger<ProcessJob>());

            return processJob;
        }
    }
}
