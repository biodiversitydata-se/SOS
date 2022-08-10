using System.Text;
using Autofac;
using SOS.Harvest.Containers;
using SOS.Harvest.Containers.Interfaces;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.DarwinCore.Interfaces;
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
using SOS.Harvest.HarvestersAquaSupport.Sers.Interfaces;
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
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

using AreaRepository = SOS.Lib.Repositories.Resource.AreaRepository;
using IAreaRepository = SOS.Lib.Repositories.Resource.Interfaces.IAreaRepository;
using ITaxonRepository = SOS.Lib.Repositories.Resource.Interfaces.ITaxonRepository;
using TaxonRepository = SOS.Lib.Repositories.Resource.TaxonRepository;

namespace SOS.Harvest.IoC.Modules
{
    public class HarvestModule : Module
    {
        public (ImportConfiguration ImportConfiguration,
            ApiManagementServiceConfiguration ApiManagementServiceConfiguration,
            MongoDbConfiguration VerbatimDbConfiguration,
            ProcessConfiguration ProcessConfiguration,
            MongoDbConfiguration ProcessDbConfiguration,
            ApplicationInsightsConfiguration ApplicationInsightsConfiguration,
            SosApiConfiguration SosApiConfiguration,
            UserServiceConfiguration UserServiceConfiguration) Configurations
        { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Add configuration
            if (Configurations.ImportConfiguration.ArtportalenConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ArtportalenConfiguration).As<ArtportalenConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.BiologgConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.BiologgConfiguration).As<BiologgConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.DwcaConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.DwcaConfiguration).As<DwcaConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.ClamServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ClamServiceConfiguration).As<ClamServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.GeoRegionApiConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.GeoRegionApiConfiguration).As<GeoRegionApiConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.FishDataServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.FishDataServiceConfiguration).As<FishDataServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.KulServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.KulServiceConfiguration).As<KulServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.iNaturalistServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.iNaturalistServiceConfiguration).As<iNaturalistServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.MvmServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.MvmServiceConfiguration).As<MvmServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.NorsServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.NorsServiceConfiguration).As<NorsServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.ObservationDatabaseConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ObservationDatabaseConfiguration).As<ObservationDatabaseConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.SersServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.SersServiceConfiguration).As<SersServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.SharkServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.SharkServiceConfiguration).As<SharkServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.VirtualHerbariumServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.VirtualHerbariumServiceConfiguration)
                    .As<VirtualHerbariumServiceConfiguration>().SingleInstance();
            if (Configurations.ImportConfiguration.TaxonListServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.TaxonListServiceConfiguration)
                    .As<TaxonListServiceConfiguration>().SingleInstance();
            if (Configurations.ApiManagementServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ApiManagementServiceConfiguration).As<ApiManagementServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ApplicationInsightsConfiguration != null)
                builder.RegisterInstance(Configurations.ApplicationInsightsConfiguration).As<ApplicationInsightsConfiguration>()
                    .SingleInstance();
            if (Configurations.SosApiConfiguration != null)
                builder.RegisterInstance(Configurations.SosApiConfiguration).As<SosApiConfiguration>()
                    .SingleInstance();
            if (Configurations.UserServiceConfiguration != null)
                builder.RegisterInstance(Configurations.UserServiceConfiguration).As<UserServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ProcessConfiguration.VocabularyConfiguration != null)
            {
                builder.RegisterInstance(Configurations.ProcessConfiguration.VocabularyConfiguration).As<VocabularyConfiguration>().SingleInstance();
            }
            if (Configurations.ProcessConfiguration.TaxonAttributeServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ProcessConfiguration.TaxonAttributeServiceConfiguration)
                    .As<TaxonAttributeServiceConfiguration>().SingleInstance();
            if (Configurations.ProcessConfiguration.TaxonServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ProcessConfiguration.TaxonServiceConfiguration).As<TaxonServiceConfiguration>()
                    .SingleInstance();
            builder.RegisterInstance(Configurations.ProcessConfiguration).As<ProcessConfiguration>().SingleInstance();

            // Vebatim Mongo Db
            var verbatimSettings = Configurations.VerbatimDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IVerbatimClient>(new VerbatimClient(verbatimSettings, Configurations.VerbatimDbConfiguration.DatabaseName,
                Configurations.VerbatimDbConfiguration.ReadBatchSize, Configurations.VerbatimDbConfiguration.WriteBatchSize)).SingleInstance();

            // Processed Mongo Db
            var processedSettings = Configurations.ProcessDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IProcessClient>(new ProcessClient(processedSettings, Configurations.ProcessDbConfiguration.DatabaseName,
                Configurations.ProcessDbConfiguration.ReadBatchSize, Configurations.ProcessDbConfiguration.WriteBatchSize)).SingleInstance();

            // Caches
            builder.RegisterType<AreaCache>().As<IAreaCache>().SingleInstance();
            builder.RegisterType<DataProviderCache>().As<IDataProviderCache>().SingleInstance();
            builder.RegisterType<ProcessedConfigurationCache>().As<ICache<string, ProcessedConfiguration>>().SingleInstance();
            builder.RegisterType<TaxonCache>().As<ICache<int, Taxon>>().SingleInstance();
            builder.RegisterType<TaxonListCache>().As<ICache<int, TaxonList>>().SingleInstance();
            builder.RegisterType<VocabularyCache>().As<ICache<VocabularyId, Vocabulary>>().SingleInstance();

            // Helpers, single instance since static data
            builder.RegisterType<AreaNameMapper>().As<IAreaNameMapper>().SingleInstance();
            builder.RegisterType<AreaHelper>().As<IAreaHelper>().SingleInstance();
            builder.RegisterType<VocabulariesDiffHelper>().As<IVocabulariesDiffHelper>().SingleInstance();
            builder.RegisterType<VocabularyValueResolver>().As<IVocabularyValueResolver>().SingleInstance();

            // Darwin Core
            builder.RegisterType<DwcArchiveReader>().As<IDwcArchiveReader>().InstancePerLifetimeScope();

            // Containers, single instance for best performance (re-init on full harvest)
            builder.RegisterType<ArtportalenMetadataContainer>().As<IArtportalenMetadataContainer>().SingleInstance();

            // Managers
            builder.RegisterType<ApiUsageStatisticsManager>().As<IApiUsageStatisticsManager>().InstancePerLifetimeScope();
            builder.RegisterType<CacheManager>().As<ICacheManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataValidationReportManager>().As<IDataValidationReportManager>().InstancePerLifetimeScope();
            builder.RegisterType<DwcaDataValidationReportManager>().As<IDwcaDataValidationReportManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTimeManager>().As<IProcessTimeManager>().InstancePerLifetimeScope();
            builder.RegisterType<ReportManager>().As<IReportManager>().InstancePerLifetimeScope();

            // Repositories elastic
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .InstancePerLifetimeScope();

            // Repositories source
            builder.RegisterType<Repositories.Source.Artportalen.AreaRepository>().As<Repositories.Source.Artportalen.Interfaces.IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ChecklistRepository>().As<IChecklistRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MediaRepository>().As<IMediaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MetadataRepository>().As<IMetadataRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseRepository>().As<IObservationDatabaseRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRepository>().As<ISightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ApiUsageStatisticsRepository>().As<IApiUsageStatisticsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SiteRepository>().As<ISiteRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PersonRepository>().As<IPersonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRelationRepository>().As<ISightingRelationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SpeciesCollectionItemRepository>().As<ISpeciesCollectionItemRepository>()
                .InstancePerLifetimeScope();

            // Repositories resource
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InvalidObservationRepository>().As<IInvalidObservationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectInfoRepository>().As<IProjectInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedConfigurationRepository>().As<IProcessedConfigurationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ReportRepository>().As<IReportRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonRepository>().As<ITaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonListRepository>().As<ITaxonListRepository>().InstancePerLifetimeScope();
            builder.RegisterType<VocabularyRepository>().As<IVocabularyRepository>().InstancePerLifetimeScope();

            // Repositories verbatim
            builder.RegisterType<ArtportalenChecklistVerbatimRepository>().As<IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int>>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenVerbatimRepository>().As<IArtportalenVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveVerbatimRepository>().As<IDarwinCoreArchiveVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationVerbatimRepository>().As<IFishDataObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationVerbatimRepository>().As<IMvmObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationVerbatimRepository>().As<INorsObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseVerbatimRepository>().As<IObservationDatabaseVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SersObservationVerbatimRepository>().As<ISersObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationVerbatimRepository>().As<ISharkObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationVerbatimRepository>()
                .As<IVirtualHerbariumObservationVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories processed 
            builder.RegisterType<UserObservationRepository>().As<IUserObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessedChecklistRepository>().As<IProcessedChecklistRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .InstancePerLifetimeScope();

            // Add harvesters observations
            builder.RegisterType<AreaHarvester>().As<IAreaHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenObservationHarvester>().As<IArtportalenObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<BiologgObservationHarvester>().As<IBiologgObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcObservationHarvester>().As<IDwcObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<VocabularyHarvester>().As<IVocabularyHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectHarvester>().As<IProjectHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonListHarvester>().As<ITaxonListHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationHarvester>().As<IFishDataObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationHarvester>().As<IKulObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<iNaturalistObservationHarvester>().As<IiNaturalistObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationHarvester>().As<IMvmObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationHarvester>().As<INorsObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseHarvester>().As<IObservationDatabaseHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationHarvester>().As<ISersObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationHarvester>().As<ISharkObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<Repositories.Source.Artportalen.TaxonRepository>().As<Repositories.Source.Artportalen.Interfaces.ITaxonRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationHarvester>().As<IVirtualHerbariumObservationHarvester>()
                .InstancePerLifetimeScope();

            // Add harvesters checklists
            builder.RegisterType<ArtportalenChecklistHarvester>().As<IArtportalenChecklistHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcChecklistHarvester>().As<IDwcChecklistHarvester>()
                .InstancePerLifetimeScope();

            // Add processors
            builder.RegisterType<ArtportalenObservationProcessor>().As<IArtportalenObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcaObservationProcessor>().As<IDwcaObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationProcessor>().As<IFishDataObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationProcessor>().As<IKulObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationProcessor>().As<IMvmObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationProcessor>().As<INorsObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseProcessor>().As<IObservationDatabaseProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationProcessor>().As<ISersObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationProcessor>().As<ISharkObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationProcessor>().As<IVirtualHerbariumObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<TaxonProcessor>().As<ITaxonProcessor>()
                .InstancePerLifetimeScope();

            // Add checklist processors
            builder.RegisterType<ArtportalenChecklistProcessor>().As<IArtportalenChecklistProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcaChecklistProcessor>().As<IDwcaChecklistProcessor>()
                .InstancePerLifetimeScope();

            // Add Vocabulary Factories
            builder.RegisterType<AccessRightsVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AreaTypeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BasisOfRecordVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BehaviorVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BiotopeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BirdNestActivityVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ContinentVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CountryVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DeterminationMethodVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DiscoveryMethodVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EstablishmentMeansVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<InstitutionVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LifeStageVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OccurrenceStatusVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ReproductiveConditionVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SensitivityCategoryVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SexVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SubstrateVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TypeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UnitVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<VerificationStatusVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonCategoryVocabularyFactory>().InstancePerLifetimeScope();

            // Add Validation Report Factories
            builder.RegisterType<DwcaDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<KulDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<MvmDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<NorsDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SersDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumValidationReportFactory>().InstancePerLifetimeScope();

            // Add verbatim data Services
            builder.RegisterType<AquaSupportRequestService>().As<IAquaSupportRequestService>();
            builder.RegisterType<GeoRegionApiService>().As<IGeoRegionApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenDataService>().As<IArtportalenDataService>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationService>().As<IFishDataObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<FileDownloadService>().As<IFileDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationService>().As<IKulObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<iNaturalistObservationService>().As<IiNaturalistObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationService>().As<IMvmObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationService>().As<INorsObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseDataService>().As<IObservationDatabaseDataService>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationService>().As<ISersObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationService>().As<ISharkObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationService>().As<IVirtualHerbariumObservationService>()
                .InstancePerLifetimeScope();

            // Service Clients
            builder.RegisterType<MvmService.SpeciesObservationChangeServiceClient>()
                .As<MvmService.ISpeciesObservationChangeService>().InstancePerLifetimeScope();

            // Add misc Services
            builder.RegisterType<ApiManagementUserService>().As<IApiManagementUserService>().InstancePerLifetimeScope();
            builder.RegisterType<ApplicationInsightsService>().As<IApplicationInsightsService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<TaxonAttributeService>().As<ITaxonAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonService>().As<ITaxonService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonServiceProxy>().As<ITaxonServiceProxy>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonListService>().As<ITaxonListService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();

            // Add managers
            builder.RegisterType<DiffusionManager>().As<IDiffusionManager>().InstancePerLifetimeScope();
            builder.RegisterType<InstanceManager>().As<IInstanceManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessManager>().As<IProcessManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTimeManager>().As<IProcessTimeManager>().InstancePerLifetimeScope();
            builder.RegisterType<ValidationManager>().As<IValidationManager>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ActivateInstanceJob>().As<IActivateInstanceJob>().InstancePerLifetimeScope();
            builder.RegisterType<AreasHarvestJob>().As<IAreasHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ApiUsageStatisticsHarvestJob>().As<IApiUsageStatisticsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ChecklistsHarvestJob>().As<IChecklistsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveHarvestJob>().As<IDwcArchiveHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<VocabulariesImportJob>().As<IVocabulariesImportJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessChecklistsJob>().As<IProcessChecklistsJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessObservationsJob>().As<IProcessObservationsJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTaxaJob>().As<IProcessTaxaJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectsHarvestJob>().As<IProjectsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonListsHarvestJob>().As<ITaxonListsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationsHarvestJob>().As<IObservationsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<CreateDwcaDataValidationReportJob>().As<ICreateDwcaDataValidationReportJob>().InstancePerLifetimeScope();
            builder.RegisterType<DataValidationReportJob>().As<IDataValidationReportJob>().InstancePerLifetimeScope();
        }
    }
}