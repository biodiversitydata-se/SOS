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
using SOS.Export.MongoDb;
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Constants;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Mvm;
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

            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var exportClient = new ExportClient(
                    processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                    processConfiguration.ProcessedDbConfiguration.DatabaseName,
                    processConfiguration.ProcessedDbConfiguration.BatchSize);
            var areaHelper = new AreaHelper(
                new ProcessedAreaRepository(processClient, new NullLogger<ProcessedAreaRepository>()),
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>()));
            var taxonVerbatimRepository =
                new TaxonVerbatimRepository(verbatimClient, new NullLogger<TaxonVerbatimRepository>());
            var fieldMappingVerbatimRepository =
                new FieldMappingVerbatimRepository(verbatimClient, new NullLogger<FieldMappingVerbatimRepository>());
            var taxonProcessedRepository =
                new ProcessedTaxonRepository(processClient, new NullLogger<ProcessedTaxonRepository>());
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessed)
            {
                processedObservationRepository = new ProcessedObservationRepository(processClient, elasticClient,
                    invalidObservationRepository,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedObservationRepository>());
            }
            else
            {
                processedObservationRepository = new Mock<IProcessedObservationRepository>().Object;
            }

            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    new Export.Repositories.ProcessedFieldMappingRepository(exportClient, new NullLogger<Export.Repositories.ProcessedFieldMappingRepository>()), 
                    new TaxonManager(
                        new Export.Repositories.ProcessedTaxonRepository(exportClient, new NullLogger<Export.Repositories.ProcessedTaxonRepository>()), 
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
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator, 
                new NullLogger<ClamPortalObservationProcessor>());
            var kulProcessor = new KulObservationProcessor(
                new KulObservationVerbatimRepository(verbatimClient,
                    new NullLogger<KulObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<KulObservationProcessor>());
            var mvmProcessor = new MvmObservationProcessor(
                new MvmObservationVerbatimRepository(verbatimClient,
                    new NullLogger<MvmObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<MvmObservationProcessor>());
            var norsProcessor = new NorsObservationProcessor(
                new NorsObservationVerbatimRepository(verbatimClient,
                    new NullLogger<NorsObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<NorsObservationProcessor>());
            var sersProcessor = new SersObservationProcessor(
                new SersObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SersObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<SersObservationProcessor>());
            var sharkProcessor = new SharkObservationProcessor(
                new SharkObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SharkObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<SharkObservationProcessor>());
            var virtualHrbariumProcessor = new VirtualHerbariumObservationProcessor(
                new VirtualHerbariumObservationVerbatimRepository(verbatimClient,
                    new NullLogger<VirtualHerbariumObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()), dwcArchiveFileWriterCoordinator,
                new NullLogger<VirtualHerbariumObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                processConfiguration, dwcArchiveFileWriterCoordinator,
                new NullLogger<ArtportalenObservationProcessor>());
            var instanceManager = new InstanceManager(
                new ProcessedObservationRepository(processClient, elasticClient, invalidObservationRepository,
                    new ElasticSearchConfiguration(), new NullLogger<ProcessedObservationRepository>()),
                processInfoRepository,
                new NullLogger<InstanceManager>());

            var copyFieldMappingsJob = new CopyFieldMappingsJob(fieldMappingVerbatimRepository,
                processedFieldMappingRepository, harvestInfoRepository, processInfoRepository,
                new NullLogger<CopyFieldMappingsJob>());
            var processTaxaJob = new ProcessTaxaJob(taxonVerbatimRepository, taxonProcessedRepository,
                harvestInfoRepository, processInfoRepository, new NullLogger<ProcessTaxaJob>());
            var dwcaProcessor = new DwcaObservationProcessor(
                new DwcaVerbatimRepository(verbatimClient, new NullLogger<DwcaVerbatimRepository>()),
                processedObservationRepository,
                processedFieldMappingRepository,
                new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration()),
                areaHelper,
                processConfiguration,
                dwcArchiveFileWriterCoordinator,
                new NullLogger<DwcaObservationProcessor>());

            var dataProviderManager = new DataProviderManager(
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DataProviderManager>());

            var processJob = new ProcessJob(
                processedObservationRepository,
                processInfoRepository,
                harvestInfoRepository,
                clamPortalProcessor,
                kulProcessor,
                mvmProcessor,
                norsProcessor,
                sersProcessor,
                sharkProcessor,
                virtualHrbariumProcessor,
                artportalenProcessor,
                dwcaProcessor,
                dataProviderManager,
                taxonProcessedRepository,
                instanceManager,
                copyFieldMappingsJob,
                processTaxaJob,
                areaHelper,
                dwcArchiveFileWriterCoordinator,
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
                false,
                false,
                false,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().BeTrue();
        }
    }
}