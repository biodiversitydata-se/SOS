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
using SOS.Export.Services;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Constants;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Managers;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
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
                new AreaRepository(processClient, new NullLogger<AreaRepository>()));
           
            var taxonProcessedRepository =
                new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
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
            
            var processInfoRepository =
                new ProcessInfoRepository(processClient, new NullLogger<ProcessInfoRepository>());
            var harvestInfoRepository =
                new HarvestInfoRepository(verbatimClient, new NullLogger<HarvestInfoRepository>());
            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());
            var fieldMappingResolverHelper =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());
            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    fieldMappingResolverHelper,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new FileService(),
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" }, new NullLogger<DwcArchiveFileWriterCoordinator>());
            var clamPortalProcessor = new ClamPortalObservationProcessor(
                new ClamObservationVerbatimRepository(verbatimClient,
                    new NullLogger<ClamObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator, 
                validationManager,
                new NullLogger<ClamPortalObservationProcessor>());
            var fishDataProcessor = new FishDataObservationProcessor(
                new FishDataObservationVerbatimRepository(verbatimClient,
                    new NullLogger<FishDataObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator, 
                validationManager,
                new NullLogger<FishDataObservationProcessor>());
            var kulProcessor = new KulObservationProcessor(
                new KulObservationVerbatimRepository(verbatimClient,
                    new NullLogger<KulObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<KulObservationProcessor>());
            var mvmProcessor = new MvmObservationProcessor(
                new MvmObservationVerbatimRepository(verbatimClient,
                    new NullLogger<MvmObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<MvmObservationProcessor>());
            var norsProcessor = new NorsObservationProcessor(
                new NorsObservationVerbatimRepository(verbatimClient,
                    new NullLogger<NorsObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<NorsObservationProcessor>());
            var sersProcessor = new SersObservationProcessor(
                new SersObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SersObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<SersObservationProcessor>());
            var sharkProcessor = new SharkObservationProcessor(
                new SharkObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SharkObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<SharkObservationProcessor>());
            var virtualHrbariumProcessor = new VirtualHerbariumObservationProcessor(
                new VirtualHerbariumObservationVerbatimRepository(verbatimClient,
                    new NullLogger<VirtualHerbariumObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                fieldMappingResolverHelper, 
                dwcArchiveFileWriterCoordinator,
                validationManager,
                new NullLogger<VirtualHerbariumObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                vocabularyRepository,
                fieldMappingResolverHelper,
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
                new DarwinCoreArchiveVerbatimRepository(verbatimClient, new NullLogger<DarwinCoreArchiveVerbatimRepository>()),
                processedObservationRepository,
                vocabularyRepository,
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration()),
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