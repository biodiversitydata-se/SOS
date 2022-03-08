using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nest;
using SOS.Lib.Cache;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Constants;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Services;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Processors.FishData;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Mvm;
using SOS.Process.Processors.Nors;
using SOS.Process.Processors.ObservationDatabase;
using SOS.Process.Processors.Sers;
using SOS.Process.Processors.Shark;
using SOS.Process.Processors.Taxon;
using SOS.Process.Processors.VirtualHerbarium;
using SOS.Process.Services;
using SOS.Process.Services.Interfaces;
using Xunit;

namespace SOS.Process.IntegrationTests.Jobs
{
    public class ProcessObservationsJobIntegrationTests : TestBase
    {
        private ProcessJob CreateProcessJob(bool storeProcessed)
        {
            var processConfiguration = GetProcessConfiguration();
            var elasticConfiguration = GetElasticConfiguration();
            var elasticClientManager = new ElasticClientManager(elasticConfiguration, true);
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

            var dataProviderRepository =
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>());


            var taxonCache = new TaxonCache(taxonProcessedRepository);
            var invalidObservationRepository =
                new InvalidObservationRepository(processClient, new NullLogger<InvalidObservationRepository>());
            var diffusionManager = new DiffusionManager(areaHelper, new NullLogger<DiffusionManager>());
            var processManager = new ProcessManager(processConfiguration);
            var validationManager = new ValidationManager(invalidObservationRepository, new NullLogger<ValidationManager>());
            IProcessedObservationRepository processedObservationRepository;
            if (storeProcessed)
            {
                processedObservationRepository = new ProcessedObservationRepository(elasticClientManager, processClient,
                    new ElasticSearchConfiguration(),
                    new ClassCache<ProcessedConfiguration>(new MemoryCache(new MemoryCacheOptions())),
                    new Mock<ITaxonManager>().Object,
                    new NullLogger<ProcessedObservationRepository>());
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
            var vocabularyValueResolver =
                new VocabularyValueResolver(vocabularyRepository, new VocabularyConfiguration());
            var dwcArchiveFileWriterCoordinator = new DwcArchiveFileWriterCoordinator(new DwcArchiveFileWriter(
                new DwcArchiveOccurrenceCsvWriter(
                    vocabularyValueResolver,
                    new NullLogger<DwcArchiveOccurrenceCsvWriter>()),
                new ExtendedMeasurementOrFactCsvWriter(new NullLogger<ExtendedMeasurementOrFactCsvWriter>()),
                new SimpleMultimediaCsvWriter(new NullLogger<SimpleMultimediaCsvWriter>()),
                new FileService(),
                new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()),
                new NullLogger<DwcArchiveFileWriter>()
            ), new FileService(), dataProviderRepository, verbatimClient, new DwcaFilesCreationConfiguration { IsEnabled = true, FolderPath = @"c:\temp" }, new NullLogger<DwcArchiveFileWriterCoordinator>());

          
            var clamPortalProcessor = new ClamPortalObservationProcessor(
                new ClamObservationVerbatimRepository(verbatimClient,
                    new NullLogger<ClamObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver, 
                dwcArchiveFileWriterCoordinator, 
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<ClamPortalObservationProcessor>());
            var fishDataProcessor = new FishDataObservationProcessor(
                new FishDataObservationVerbatimRepository(verbatimClient,
                    new NullLogger<FishDataObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<FishDataObservationProcessor>());
            var kulProcessor = new KulObservationProcessor(
                new KulObservationVerbatimRepository(verbatimClient,
                    new NullLogger<KulObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<KulObservationProcessor>());
            var mvmProcessor = new MvmObservationProcessor(
                new MvmObservationVerbatimRepository(verbatimClient,
                    new NullLogger<MvmObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<MvmObservationProcessor>());
            var norsProcessor = new NorsObservationProcessor(
                new NorsObservationVerbatimRepository(verbatimClient,
                    new NullLogger<NorsObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<NorsObservationProcessor>());
            var sersProcessor = new SersObservationProcessor(
                new SersObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SersObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<SersObservationProcessor>());
            var sharkProcessor = new SharkObservationProcessor(
                new SharkObservationVerbatimRepository(verbatimClient,
                    new NullLogger<SharkObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<SharkObservationProcessor>());
            var virtualHrbariumProcessor = new VirtualHerbariumObservationProcessor(
                new VirtualHerbariumObservationVerbatimRepository(verbatimClient,
                    new NullLogger<VirtualHerbariumObservationVerbatimRepository>()),
                areaHelper,
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<VirtualHerbariumObservationProcessor>());
            var artportalenProcessor = new ArtportalenObservationProcessor(
                new ArtportalenVerbatimRepository(verbatimClient, new NullLogger<ArtportalenVerbatimRepository>()),
                processedObservationRepository,
                vocabularyRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<ArtportalenObservationProcessor>());
            var instanceManager = new InstanceManager(
                new ProcessedObservationRepository(elasticClientManager, processClient,
                    new ElasticSearchConfiguration(),  
                    new ClassCache<ProcessedConfiguration>(new MemoryCache(new MemoryCacheOptions())),
                    new Mock<ITaxonManager>().Object,
                    new NullLogger<ProcessedObservationRepository>()),
                processInfoRepository,
                new NullLogger<InstanceManager>());

            // todo
            var taxonProcessor = new TaxonProcessor(new Mock<ITaxonService>().Object, new Mock<ITaxonAttributeService>().Object, new TaxonRepository(processClient, new NullLogger<TaxonRepository>()), processConfiguration, new NullLogger<TaxonProcessor>());

            var processTaxaJob = new ProcessTaxaJob(taxonProcessor, 
                harvestInfoRepository, processInfoRepository, new NullLogger<ProcessTaxaJob>());
            var dwcaProcessor = new DwcaObservationProcessor(
                verbatimClient,
                processedObservationRepository,
                vocabularyRepository,
                vocabularyValueResolver,
                areaHelper,
                dwcArchiveFileWriterCoordinator,
                processManager,
                validationManager,
                diffusionManager,
                processConfiguration,
                new NullLogger<DwcaObservationProcessor>());

            var observationDatabaseProcessor = new ObservationDatabaseProcessor(
                new ObservationDatabaseVerbatimRepository(verbatimClient,
                    new NullLogger<ObservationDatabaseVerbatimRepository>()),
                processedObservationRepository,
                vocabularyValueResolver,
                dwcArchiveFileWriterCoordinator,
                diffusionManager,
                processManager,
                validationManager,
                areaHelper,
                processConfiguration,
                new NullLogger<ObservationDatabaseProcessor>());

            var dataProviderCache = new DataProviderCache(new DataProviderRepository(processClient, new NullLogger<DataProviderRepository>()));
            

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
                observationDatabaseProcessor,
                sersProcessor,
                sharkProcessor,
                virtualHrbariumProcessor,
                dwcaProcessor,
                taxonCache,
                dataProviderCache,
                instanceManager,
                validationManager,
                processTaxaJob,
                areaHelper,
                dwcArchiveFileWriterCoordinator,
                processConfiguration, 
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