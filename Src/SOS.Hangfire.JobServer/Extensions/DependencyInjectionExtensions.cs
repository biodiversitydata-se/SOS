

using Elastic.Clients.Elasticsearch.Cluster;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Export.Jobs;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Harvest.Containers;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.Factories.Validation;
using SOS.Harvest.Factories.Vocabularies;
using SOS.Harvest.Harvesters;
using SOS.Harvest.Harvesters.AquaSupport.FishData;
using SOS.Harvest.Harvesters.AquaSupport.FishData.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Kul;
using SOS.Harvest.Harvesters.AquaSupport.Kul.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Nors;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Sers;
using SOS.Harvest.Harvesters.AquaSupport.Sers.Interfaces;
using SOS.Harvest.Harvesters.Artportalen;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.Biologg;
using SOS.Harvest.Harvesters.Biologg.Interfaces;
using SOS.Harvest.Harvesters.DwC;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.iNaturalist;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Harvesters.Mvm;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Harvesters.ObservationDatabase;
using SOS.Harvest.Harvesters.ObservationDatabase.Interfaces;
using SOS.Harvest.Harvesters.Shark;
using SOS.Harvest.Harvesters.Shark.Interfaces;
using SOS.Harvest.Harvesters.VirtualHerbarium;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Harvest.Helpers;
using SOS.Harvest.Helpers.Interfaces;
using SOS.Harvest.Jobs;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Mappings;
using SOS.Harvest.Mappings.Interfaces;
using SOS.Harvest.Processors.Artportalen;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.FishData;
using SOS.Harvest.Processors.FishData.Interfaces;
using SOS.Harvest.Processors.iNaturalist;
using SOS.Harvest.Processors.iNaturalist.Interfaces;
using SOS.Harvest.Processors.Kul;
using SOS.Harvest.Processors.Kul.Interfaces;
using SOS.Harvest.Processors.Mvm;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Harvest.Processors.Nors;
using SOS.Harvest.Processors.Nors.Interfaces;
using SOS.Harvest.Processors.ObservationDatabase;
using SOS.Harvest.Processors.ObservationDatabase.Interfaces;
using SOS.Harvest.Processors.Sers;
using SOS.Harvest.Processors.Sers.Interfaces;
using SOS.Harvest.Processors.Shark;
using SOS.Harvest.Processors.Shark.Interfaces;
using SOS.Harvest.Processors.Taxon;
using SOS.Harvest.Processors.Taxon.Interfaces;
using SOS.Harvest.Processors.VirtualHerbarium;
using SOS.Harvest.Processors.VirtualHerbarium.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Repositories.Source.ObservationsDatabase;
using SOS.Harvest.Repositories.Source.ObservationsDatabase.Interfaces;
using SOS.Harvest.Services;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvest.Services.Taxon;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Jobs.Shared;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace SOS.Hangfire.JobServer.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        // Get configuration
        var apiManagementServiceConfiguration = configuration.GetSection("ApiManagementServiceConfiguration").Get<ApiManagementServiceConfiguration>();
        var cryptoConfiguration = configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>();
        var verbatimDbConfiguration = configuration.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
        var processDbConfiguration = configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        var searchDbConfiguration = configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        var importConfiguration = configuration.GetSection(nameof(ImportConfiguration)).Get<ImportConfiguration>();
        var processConfiguration = configuration.GetSection(nameof(ProcessConfiguration)).Get<ProcessConfiguration>();
        var exportConfiguration = configuration.GetSection(nameof(ExportConfiguration)).Get<ExportConfiguration>();
        var blobStorageConfiguration = configuration.GetSection(nameof(BlobStorageConfiguration)).Get<BlobStorageConfiguration>();
        var dataCiteServiceConfiguration = configuration.GetSection(nameof(DataCiteServiceConfiguration)).Get<DataCiteServiceConfiguration>();
        var applicationInsightsConfiguration = configuration.GetSection(nameof(ApplicationInsightsConfiguration)).Get<ApplicationInsightsConfiguration>();
        var sosApiConfiguration = configuration.GetSection(nameof(SosApiConfiguration)).Get<SosApiConfiguration>();
        var userServiceConfiguration = configuration.GetSection(nameof(UserServiceConfiguration)).Get<UserServiceConfiguration>();
        var areaConfiguration = configuration.GetSection(nameof(AreaConfiguration)).Get<AreaConfiguration>();

        // Elasticsearch
        services.AddSingleton(searchDbConfiguration);
        services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(searchDbConfiguration));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Add configuration
        services.AddSingleton(importConfiguration.ArtportalenConfiguration);
        services.AddSingleton(importConfiguration.BiologgConfiguration);
        services.AddSingleton(importConfiguration.DwcaConfiguration);
        services.AddSingleton(importConfiguration.ClamServiceConfiguration);
        services.AddSingleton(importConfiguration.GeoRegionApiConfiguration);
        services.AddSingleton(importConfiguration.FishDataServiceConfiguration);
        services.AddSingleton(importConfiguration.KulServiceConfiguration);
        services.AddSingleton(importConfiguration.iNaturalistServiceConfiguration);
        services.AddSingleton(importConfiguration.MvmServiceConfiguration);
        services.AddSingleton(importConfiguration.NorsServiceConfiguration);
        services.AddSingleton(importConfiguration.ObservationDatabaseConfiguration);
        services.AddSingleton(importConfiguration.SersServiceConfiguration);
        services.AddSingleton(importConfiguration.SharkServiceConfiguration);
        services.AddSingleton(importConfiguration.VirtualHerbariumServiceConfiguration);
        services.AddSingleton(importConfiguration.TaxonListServiceConfiguration);
        services.AddSingleton(apiManagementServiceConfiguration);
        if (applicationInsightsConfiguration != null)
            services.AddSingleton(applicationInsightsConfiguration);
        else
            services.AddSingleton(new ApplicationInsightsConfiguration());
        services.AddSingleton(sosApiConfiguration);
        services.AddSingleton(userServiceConfiguration);
        services.AddSingleton(areaConfiguration);
        services.AddSingleton(processConfiguration.VocabularyConfiguration);
        services.AddSingleton(processConfiguration.TaxonAttributeServiceConfiguration);
        services.AddSingleton(processConfiguration.TaxonServiceConfiguration);
        services.AddSingleton(processConfiguration);

        // MongoDb VerbatimClient
        var verbatimSettings = verbatimDbConfiguration.GetMongoDbSettings();
        services.AddSingleton<IVerbatimClient>(new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName,
            verbatimDbConfiguration.ReadBatchSize, verbatimDbConfiguration.WriteBatchSize));

        // MongoDb ProcessClient
        var processedSettings = processDbConfiguration.GetMongoDbSettings();
        services.AddSingleton<IProcessClient>(new ProcessClient(processedSettings, processDbConfiguration.DatabaseName,
            processDbConfiguration.ReadBatchSize, processDbConfiguration.WriteBatchSize));

        // Add Caches
        services.AddMemoryCache();
        services.AddSingleton<IAreaCache, AreaCache>();
        services.AddSingleton<IDataProviderCache, DataProviderCache>();
        services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        services.AddSingleton<ICache<int, Taxon>, TaxonCache>();
        services.AddSingleton<ICache<int, TaxonList>, TaxonListCache>();
        services.AddSingleton<ICache<VocabularyId, Vocabulary>, VocabularyCache>();
        services.AddSingleton<ICache<int, ProjectInfo>, ProjectCache>();
        services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
        services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
        services.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);
        //services.AddSingleton<SortableFieldsCache>();

        // Helpers, single instance since static data
        services.AddSingleton<IAreaNameMapper, AreaNameMapper>();
        services.AddSingleton<IAreaHelper, AreaHelper>();
        services.AddSingleton<IVocabulariesDiffHelper, VocabulariesDiffHelper>();
        services.AddSingleton<IVocabularyValueResolver, VocabularyValueResolver>();

        // Containers, single instance for best performance (re-init on full harvest)
        services.AddSingleton<IArtportalenMetadataContainer, ArtportalenMetadataContainer>();

        // Repositories elastic
        services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();

        // Repositories source
        services.AddScoped<Harvest.Repositories.Source.Artportalen.Interfaces.IAreaRepository, Harvest.Repositories.Source.Artportalen.AreaRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();
        services.AddScoped<IDiaryEntryRepository, DiaryEntryRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IMetadataRepository, MetadataRepository>();
        services.AddScoped<IObservationDatabaseRepository, ObservationDatabaseRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ISightingRepository, SightingRepository>();
        services.AddScoped<IApiUsageStatisticsRepository, ApiUsageStatisticsRepository>();
        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<Harvest.Repositories.Source.Artportalen.Interfaces.IDatasetRepository, Harvest.Repositories.Source.Artportalen.DatasetRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ISightingRelationRepository, SightingRelationRepository>();
        services.AddScoped<ISpeciesCollectionItemRepository, SpeciesCollectionItemRepository>();

        // Repositories resource
        services.AddScoped<Lib.Repositories.Resource.Interfaces.IAreaRepository, Lib.Repositories.Resource.AreaRepository>();
        services.AddScoped<IDataProviderRepository, DataProviderRepository>();
        services.AddScoped<IHarvestInfoRepository, HarvestInfoRepository>();
        services.AddScoped<IInvalidObservationRepository, InvalidObservationRepository>();
        services.AddScoped<IInvalidEventRepository, InvalidEventRepository>();
        services.AddScoped<IProjectInfoRepository, ProjectInfoRepository>();
        services.AddScoped<IArtportalenDatasetMetadataRepository, ArtportalenDatasetMetadataRepository>();
        services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<Lib.Repositories.Resource.Interfaces.ITaxonRepository, Lib.Repositories.Resource.TaxonRepository>();
        services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();

        // Repositories verbatim
        services.AddScoped<IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int>, ArtportalenChecklistVerbatimRepository>();
        services.AddScoped<IArtportalenVerbatimRepository, ArtportalenVerbatimRepository>();
        services.AddScoped<IDarwinCoreArchiveVerbatimRepository, DarwinCoreArchiveVerbatimRepository>();
        services.AddScoped<IFishDataObservationVerbatimRepository, FishDataObservationVerbatimRepository>();
        services.AddScoped<IKulObservationVerbatimRepository, KulObservationVerbatimRepository>();
        services.AddScoped<IMvmObservationVerbatimRepository, MvmObservationVerbatimRepository>();
        services.AddScoped<INorsObservationVerbatimRepository, NorsObservationVerbatimRepository>();
        services.AddScoped<IObservationDatabaseVerbatimRepository, ObservationDatabaseVerbatimRepository>();
        services.AddScoped<ISersObservationVerbatimRepository, SersObservationVerbatimRepository>();
        services.AddScoped<ISharkObservationVerbatimRepository, SharkObservationVerbatimRepository>();
        services.AddScoped<IVirtualHerbariumObservationVerbatimRepository, VirtualHerbariumObservationVerbatimRepository>();
        services.AddScoped<IiNaturalistObservationVerbatimRepository, iNaturalistObservationVerbatimRepository>();

        // Repositories processed
        services.AddScoped<IUserObservationRepository, UserObservationRepository>();
        services.AddScoped<Lib.Repositories.Processed.Interfaces.IDatasetRepository, Lib.Repositories.Processed.DatasetRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IProcessedChecklistRepository, ProcessedChecklistRepository>();

        // Add harvesters observations
        services.AddScoped<IAreaHarvester, AreaHarvester>();
        services.AddScoped<IArtportalenObservationHarvester, ArtportalenObservationHarvester>();
        services.AddScoped<IBiologgObservationHarvester, BiologgObservationHarvester>();
        services.AddScoped<IDwcObservationHarvester, DwcObservationHarvester>();
        services.AddScoped<IVocabularyHarvester, VocabularyHarvester>();
        services.AddScoped<IProjectHarvester, ProjectHarvester>();
        services.AddScoped<IArtportalenDatasetMetadataHarvester, ArtportalenDatasetMetadataHarvester>();
        services.AddScoped<ITaxonListHarvester, TaxonListHarvester>();
        services.AddScoped<IFishDataObservationHarvester, FishDataObservationHarvester>();
        services.AddScoped<IKulObservationHarvester, KulObservationHarvester>();
        services.AddScoped<IiNaturalistObservationHarvester, iNaturalistObservationHarvester>();
        services.AddScoped<IMvmObservationHarvester, MvmObservationHarvester>();
        services.AddScoped<INorsObservationHarvester, NorsObservationHarvester>();
        services.AddScoped<IObservationDatabaseHarvester, ObservationDatabaseHarvester>();
        services.AddScoped<ISersObservationHarvester, SersObservationHarvester>();
        services.AddScoped<ISharkObservationHarvester, SharkObservationHarvester>();
        services.AddScoped<Harvest.Repositories.Source.Artportalen.Interfaces.ITaxonRepository, Harvest.Repositories.Source.Artportalen.TaxonRepository>();
        services.AddScoped<IVirtualHerbariumObservationHarvester, VirtualHerbariumObservationHarvester>();

        // Add harvesters checklists
        services.AddScoped<IArtportalenChecklistHarvester, ArtportalenChecklistHarvester>();
        services.AddScoped<IDwcChecklistHarvester, DwcChecklistHarvester>();

        // Add processors
        services.AddScoped<IArtportalenObservationProcessor, ArtportalenObservationProcessor>();
        services.AddScoped<IDwcaObservationProcessor, DwcaObservationProcessor>();
        services.AddScoped<IFishDataObservationProcessor, FishDataObservationProcessor>();
        services.AddScoped<IKulObservationProcessor, KulObservationProcessor>();
        services.AddScoped<IMvmObservationProcessor, MvmObservationProcessor>();
        services.AddScoped<INorsObservationProcessor, NorsObservationProcessor>();
        services.AddScoped<IObservationDatabaseProcessor, ObservationDatabaseProcessor>();
        services.AddScoped<ISersObservationProcessor, SersObservationProcessor>();
        services.AddScoped<ISharkObservationProcessor, SharkObservationProcessor>();
        services.AddScoped<IVirtualHerbariumObservationProcessor, VirtualHerbariumObservationProcessor>();
        services.AddScoped<IiNaturalistObservationProcessor, iNaturalistObservationProcessor>();
        services.AddScoped<ITaxonProcessor, TaxonProcessor>();
        services.AddScoped<IDwcaDatasetProcessor, DwcaDatasetProcessor>();
        services.AddScoped<IArtportalenDatasetProcessor, ArtportalenDatasetProcessor>();
        services.AddScoped<IArtportalenEventProcessor, ArtportalenEventProcessor>();
        services.AddScoped<IDwcaEventProcessor, DwcaEventProcessor>();

        // Add checklist processors
        services.AddScoped<IArtportalenChecklistProcessor, ArtportalenChecklistProcessor>();
        services.AddScoped<IDwcaChecklistProcessor, DwcaChecklistProcessor>();

        // Add Vocabulary Factories
        services.AddScoped<AccessRightsVocabularyFactory>();
        services.AddScoped<ActivityVocabularyFactory>();
        services.AddScoped<AreaTypeVocabularyFactory>();
        services.AddScoped<BasisOfRecordVocabularyFactory>();
        services.AddScoped<BehaviorVocabularyFactory>();
        services.AddScoped<BiotopeVocabularyFactory>();
        services.AddScoped<BirdNestActivityVocabularyFactory>();
        services.AddScoped<ContinentVocabularyFactory>();
        services.AddScoped<CountryVocabularyFactory>();
        services.AddScoped<DeterminationMethodVocabularyFactory>();
        services.AddScoped<DiscoveryMethodVocabularyFactory>();
        services.AddScoped<EstablishmentMeansVocabularyFactory>();
        services.AddScoped<InstitutionVocabularyFactory>();
        services.AddScoped<LifeStageVocabularyFactory>();
        services.AddScoped<OccurrenceStatusVocabularyFactory>();
        services.AddScoped<ReproductiveConditionVocabularyFactory>();
        services.AddScoped<SensitivityCategoryVocabularyFactory>();
        services.AddScoped<SexVocabularyFactory>();
        services.AddScoped<SubstrateVocabularyFactory>();
        services.AddScoped<TypeVocabularyFactory>();
        services.AddScoped<UnitVocabularyFactory>();
        services.AddScoped<VerificationStatusVocabularyFactory>();
        services.AddScoped<TaxonCategoryVocabularyFactory>();

        // Add Validation Report Factories
        services.AddScoped<DwcaDataValidationReportFactory>();
        services.AddScoped<FishDataValidationReportFactory>();
        services.AddScoped<KulDataValidationReportFactory>();
        services.AddScoped<MvmDataValidationReportFactory>();
        services.AddScoped<NorsDataValidationReportFactory>();
        services.AddScoped<SersDataValidationReportFactory>();
        services.AddScoped<VirtualHerbariumValidationReportFactory>();
        services.AddScoped<iNaturalistDataValidationReportFactory>();

        // Add verbatim data Services
        services.AddSingleton<IAquaSupportRequestService, AquaSupportRequestService>(); // Note: Changed to singleton as original had no lifetime
        services.AddScoped<IGeoRegionApiService, GeoRegionApiService>();
        services.AddScoped<IArtportalenDataService, ArtportalenDataService>();
        services.AddScoped<IFishDataObservationService, FishDataObservationService>();
        services.AddScoped<IFileDownloadService, FileDownloadService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<IKulObservationService, KulObservationService>();
        services.AddScoped<IiNaturalistObservationService, iNaturalistObservationService>();
        services.AddScoped<iNaturalistApiObservationService>();
        services.AddScoped<IMvmObservationService, MvmObservationService>();
        services.AddScoped<INorsObservationService, NorsObservationService>();
        services.AddScoped<IObservationDatabaseDataService, ObservationDatabaseDataService>();
        services.AddScoped<ISersObservationService, SersObservationService>();
        services.AddScoped<ISharkObservationService, SharkObservationService>();
        services.AddScoped<IVirtualHerbariumObservationService, VirtualHerbariumObservationService>();

        // Service Clients
        services.AddScoped<MvmService.ISpeciesObservationChangeService, MvmService.SpeciesObservationChangeServiceClient>();

        // Add misc Services
        services.AddScoped<IApiManagementUserService, ApiManagementUserService>();
        services.AddScoped<IApplicationInsightsService, ApplicationInsightsService>();
        services.AddScoped<ITaxonAttributeService, TaxonAttributeService>();
        services.AddScoped<ITaxonService, TaxonService>();
        services.AddScoped<ITaxonServiceProxy, TaxonServiceProxy>();
        services.AddScoped<ITaxonListService, TaxonListService>();
        services.AddScoped<IUserService, UserService>();

        // Add managers
        services.AddScoped<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();
        services.AddScoped<ICacheManager, CacheManager>();
        services.AddScoped<IDataProviderManager, DataProviderManager>();
        services.AddScoped<IDataValidationReportManager, DataValidationReportManager>();
        services.AddScoped<IDiffusionManager, DiffusionManager>();
        services.AddScoped<IDwcaDataValidationReportManager, DwcaDataValidationReportManager>();
        services.AddScoped<IInvalidObservationsManager, InvalidObservationsManager>();
        services.AddScoped<IInstanceManager, InstanceManager>();
        services.AddScoped<IObservationHarvesterManager, ObservationHarvesterManager>();
        services.AddScoped<IObservationProcessorManager, ObservationProcessorManager>();
        services.AddScoped<IProcessManager, ProcessManager>();
        services.AddScoped<IProcessTimeManager, ProcessTimeManager>();
        services.AddScoped<IReportManager, ReportManager>();
        services.AddScoped<IValidationManager, ValidationManager>();

        // Add jobs
        services.AddScoped<IActivateInstanceJob, ActivateInstanceJob>();
        services.AddScoped<IAreasHarvestJob, AreasHarvestJob>();
        services.AddScoped<IApiUsageStatisticsHarvestJob, ApiUsageStatisticsHarvestJob>();
        services.AddScoped<IChecklistsHarvestJob, ChecklistsHarvestJob>();
        services.AddScoped<ICleanUpJob, CleanUpJob>();
        services.AddScoped<IClearCacheJob, ClearCacheJob>();
        services.AddScoped<ICreateDwcaDataValidationReportJob, CreateDwcaDataValidationReportJob>();
        services.AddScoped<IDataValidationReportJob, DataValidationReportJob>();
        services.AddScoped<IDwcArchiveHarvestJob, DwcArchiveHarvestJob>();
        services.AddScoped<IInvalidObservationsReportsJob, InvalidObservationsReportsJob>();
        services.AddScoped<IObservationsHarvestJobFull, ObservationsHarvestJobFull>();
        services.AddScoped<IObservationsHarvestJobIncremental, ObservationsHarvestJobIncremental>();
        services.AddScoped<IProcessChecklistsJob, ProcessChecklistsJob>();
        services.AddScoped<IProcessObservationsJobFull, ProcessObservationsJobFull>();
        services.AddScoped<IProcessObservationsJobIncremental, ProcessObservationsJobIncremental>();
        services.AddScoped<IProcessTaxaJob, ProcessTaxaJob>();
        services.AddScoped<IProjectsHarvestJob, ProjectsHarvestJob>();
        services.AddScoped<ITaxonListsHarvestJob, TaxonListsHarvestJob>();
        services.AddScoped<IVocabulariesImportJob, VocabulariesImportJob>();
        services.AddScoped<IArtportalenDatasetMetadataHarvestJob, ArtportalenDatasetMetadataHarvestJob>();

        // Add Application Insights
        if (!string.IsNullOrEmpty(applicationInsightsConfiguration?.InstrumentationKey))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = $"InstrumentationKey={applicationInsightsConfiguration.InstrumentationKey}";
            });
        }
        else
        {
            // If no instrumentation key, register a disabled TelemetryConfiguration
            services.AddSingleton(sp =>
            {
                var config = new TelemetryConfiguration();
                config.DisableTelemetry = true;
                return config;
            });
            services.AddSingleton<TelemetryClient>();
        }


        // --- ExportModule dependencies ---

        // Add configuration
        services.AddSingleton(blobStorageConfiguration);
        services.AddSingleton(cryptoConfiguration);
        services.AddSingleton(dataCiteServiceConfiguration);
        services.AddSingleton(exportConfiguration.DOIConfiguration);
        services.AddSingleton(exportConfiguration.DwcaFilesCreationConfiguration);
        services.AddSingleton(exportConfiguration.FileDestination);
        services.AddSingleton(exportConfiguration.ZendToConfiguration);
        services.AddSingleton(exportConfiguration.VocabularyConfiguration);


        // Add security
        services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

        // Add managers
        services.AddScoped<IFilterManager, FilterManager>();
        services.AddScoped<Export.Managers.Interfaces.IObservationManager, Export.Managers.ObservationManager>();
        services.AddScoped<IProjectManager, ProjectManager>();
        services.AddSingleton<ITaxonManager, TaxonManager>();

        // Repositories elastic
        services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();

        // Repositories mongo
        services.AddScoped<IUserExportRepository, UserExportRepository>();

        // Services
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<ICryptoService, CryptoService>();
        services.AddScoped<IDataCiteService, DataCiteService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IZendToService, ZendToService>();

        // Add jobs
        services.AddScoped<ICreateDoiJob, CreateDoiJob>();
        services.AddScoped<IExportAndSendJob, ExportAndSendJob>();
        services.AddScoped<IExportAndStoreJob, ExportAndStoreJob>();
        services.AddScoped<IExportToDoiJob, ExportToDoiJob>();
        services.AddScoped<IUploadToStoreJob, UploadToStoreJob>();

        // DwC Archive
        services.AddScoped<IDwcArchiveFileWriterCoordinator, DwcArchiveFileWriterCoordinator>();
        services.AddScoped<IDwcArchiveFileWriter, DwcArchiveFileWriter>();
        services.AddScoped<IDwcArchiveEventCsvWriter, DwcArchiveEventCsvWriter>();
        services.AddScoped<IDwcArchiveEventFileWriter, DwcArchiveEventFileWriter>();
        services.AddScoped<IDwcArchiveOccurrenceCsvWriter, DwcArchiveOccurrenceCsvWriter>();
        services.AddScoped<IExtendedMeasurementOrFactCsvWriter, ExtendedMeasurementOrFactCsvWriter>();
        services.AddScoped<ISimpleMultimediaCsvWriter, SimpleMultimediaCsvWriter>();

        // File writers
        services.AddScoped<ICsvFileWriter, CsvFileWriter>();
        services.AddScoped<IExcelFileWriter, ExcelFileWriter>();
        services.AddScoped<IGeoJsonFileWriter, GeoJsonFileWriter>();
        services.AddScoped<IAnalysisManager, AnalysisManager>();
        services.AddSingleton<IGeneralizationResolver, GeneralizationResolver>();

        return services;
    }    
}