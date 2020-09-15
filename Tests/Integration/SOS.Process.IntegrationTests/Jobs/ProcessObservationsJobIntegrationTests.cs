using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Export.IO.DwcArchive;
using SOS.Export.Managers;
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Constants;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Processors.FishData;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Mvm;
using SOS.Process.Processors.Nors;
using SOS.Process.Processors.Sers;
using SOS.Process.Processors.Shark;
using SOS.Process.Processors.VirtualHerbarium;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessObservationsJobIntegrationTests : TestBase
    {
        private ProcessJob CreateProcessJob(bool storeProcessed)
        {
            var processConfiguration = GetProcessConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var uris = new Uri[elasticConfiguration.Hosts.Length];
            for (var i = 0; i < uris.Length; i++)
            {
                uris[i] = new Uri(elasticConfiguration.Hosts[i]);
            }

            var elasticClient = new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris)));

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
            
            var areaHelper = new AreaHelper(
                new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>()));
           
            var taxonProcessedRepository =
                new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var validationManager = new ValidationManager(invalidObservationRepository, new NullLogger<ValidationManager>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessed)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, elasticClient,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedObservationRepository>());
            }
            else
            {
                processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            }

            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new Export.Repositories.ProcessedFieldMappingRepository(processClient, new NullLogger<Export.Repositories.ProcessedFieldMappingRepository>()), 
                    new TaxonManager(
                        new Export.Repositories.ProcessedTaxonRepository(processClient, new NullLogger<Export.Repositories.ProcessedTaxonRepository>()), 
                        new NullLogger<Export.Managers.TaxonManager>() ), new NullLogger<DwcArchiveOccurrenceCsvWriter>()), 
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()), 
                new FileService(), 
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), new DwcaFilesCreationConfiguration {IsEnabled = true, FolderPath = @"c:\temp"}, new NullLogger<DwcArchiveFileWriterCoordinator>());
            var processInfoRepository =
                new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var harvestInfoRepository =
                new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            var clamPortalProcessor = new ClamPortalObservationProcessor(
                new ClamObservationVerbatimRepository(verbatimClient,
                    new NullLogger<ClamObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator, 
                validationManager,
                new NullLogger<ClamPortalObservationProcessor>());
            var fishDataProcessor = new FishDataObservationProcessor(
                new FishDataObservationVerbatimRepository(verbatimClient,
                    new NullLogger<FishDataObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator, validationManager,
                new NullLogger<FishDataObservationProcessor>());
            var kulProcessor = new KulObservationProcessor(
                new KulObservationVerbatimRepository(verbatimClient,
                    new NullLogger<KulObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<KulObservationProcessor>());
            var mvmProcessor = new MvmObservationProcessor(
                new MvmObservationVerbatimRepository(verbatimClient,
                    new NullLogger<MvmObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<MvmObservationProcessor>());
            var norsProcessor = new NorsObservationProcessor(
                new NorsObservationVerbatimRepository(verbatimClient,
                    new NullLogger<NorsObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<NorsObservationProcessor>());
            var sersProcessor = new SersObservationProcessor(
                new SersObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SersObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<SersObservationProcessor>());
            var sharkProcessor = new SharkObservationProcessor(
                new SharkObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SharkObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<SharkObservationProcessor>());
            var virtualHrbariumProcessor = new VirtualHerbariumObservationProcessor(
                new VirtualHerbariumObservationVerbatimRepository(verbatimClient,
                    new NullLogger<VirtualHerbariumObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<VirtualHerbariumObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                processConfiguration, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<ArtportalenObservationProcessor>());
            var instanceManager = new InstanceManager(
                new ProcessedObservationRepository(processClient, elasticClient,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedObservationRepository>()),
                processInfoRepository,
                new NullLogger<InstanceManager>());

            
            var processTaxaJob = new ProcessTaxaJob(null, // todo
                harvestInfoRepository, processInfoRepository, new NullLogger<ProcessTaxaJob>());
            var dwcaProcessor = new DwcaObservationProcessor(
                new DwcaVerbatimRepository(verbatimClient, new NullLogger<DwcaVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                areaHelper,
                processConfiguration,
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<DwcaObservationProcessor>());

            var dataProviderManager = new DataProviderManager(
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DataProviderManager>());

            var processJob = new ProcessJob(
                processedObservationRepository,
                processInfoRepository,
                harvestInfoRepository,
                artportalenProcessor,
                clamPortalProcessor,
                fishDataProcessor,
                kulProcessor,
                mvmProcessor,
                norsProcessor,
                sersProcessor,
                sharkProcessor,
                virtualHrbariumProcessor,
                dwcaProcessor,
                taxonProcessedRepository,
                dataProviderManager,
                instanceManager,
                validationManager,
                processTaxaJob,
                areaHelper,
                dwcArchiveFileWriterCoordinator,
                new ProcessConfiguration(), 
                new NullLogger<ProcessJob>());

            return processJob;
        }


        [Fact]
        public async Task Run_process_job_for_butterflymonitoring_dwca()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processJob = CreateProcessJob(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processJob.RunAsync(
                new List<string> {DataProviderIdentifiers.ButterflyMonitoring},
                JobRunModes.Full,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Run_the_process_Artportalen_job()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processJob = CreateProcessJob(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await processJob.RunAsync(
                new List<string> {DataProviderIdentifiers.Artportalen},
                JobRunModes.Full,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}