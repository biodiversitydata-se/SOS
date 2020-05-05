using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Nors;
using SOS.Process.Processors.Sers;
using SOS.Process.Processors.Shark;
using SOS.Process.Processors.VirtualHerbarium;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
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
            var processJob = CreateProcessJob(storeProcessed: false);

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


        [Fact]
        public async Task Run_process_job_for_dwca()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processJob = CreateProcessJob(storeProcessed: false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processJob.RunAsync(
                (int)ObservationProvider.Dwca,
                false,
                false,
                false,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        private ProcessJob CreateProcessJob(bool storeProcessed)
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
            var areaHelper = new AreaHelper(
                new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()), 
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>()));
            var taxonVerbatimRepository = new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            var fieldMappingVerbatimRepository = new FieldMappingVerbatimRepository(verbatimClient, new NullLogger<FieldMappingVerbatimRepository>());
            var taxonProcessedRepository = new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            var invalidObservationRepository = new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessed)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, invalidObservationRepository, new NullLogger<ProcessedObservationRepository>(), elasticClient);
            }
            else
            {
                processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            }

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
            var norsProcessor = new NorsObservationProcessor(
                new NorsObservationVerbatimRepository(verbatimClient, new NullLogger<NorsObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new NullLogger<NorsObservationProcessor>());
            var sersProcessor = new SersObservationProcessor(
                new SersObservationVerbatimRepository(verbatimClient, new NullLogger<SersObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new NullLogger<SersObservationProcessor>());
            var sharkProcessor = new SharkObservationProcessor(
                new SharkObservationVerbatimRepository(verbatimClient, new NullLogger<SharkObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new NullLogger<SharkObservationProcessor>());
            var virtualHrbariumProcessor = new VirtualHerbariumObservationProcessor(
                new VirtualHerbariumObservationVerbatimRepository(verbatimClient, new NullLogger<VirtualHerbariumObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                new NullLogger<VirtualHerbariumObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository, 
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                processConfiguration,
                new NullLogger<ArtportalenObservationProcessor>());
            var instanceManager = new InstanceManager(
                new ProcessedObservationRepository(processClient, invalidObservationRepository, new NullLogger<ProcessedObservationRepository>(), elasticClient),
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
                norsProcessor,
                sersProcessor,
                sharkProcessor,
                virtualHrbariumProcessor,
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
