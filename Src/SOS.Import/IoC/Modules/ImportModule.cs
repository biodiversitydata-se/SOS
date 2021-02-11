using System.Text;
using Autofac;
using SOS.Import.Containers;
using SOS.Import.Containers.Interfaces;
using SOS.Import.DarwinCore;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Import.Factories.Validation;
using SOS.Import.Factories.Vocabularies;
using SOS.Import.Harvesters;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Managers;
using SOS.Import.Managers.Interfaces;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Repositories.Source.ObservationsDatabase;
using SOS.Import.Repositories.Source.ObservationsDatabase.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.IoC.Modules
{
    public class ImportModule : Module
    {
        public (ImportConfiguration ImportConfiguration, 
            MongoDbConfiguration VerbatimDbConfiguration, 
            MongoDbConfiguration ProcessDbConfiguration) Configurations { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            // Add configuration
            if (Configurations.ImportConfiguration.ArtportalenConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ArtportalenConfiguration).As<ArtportalenConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.DwcaConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.DwcaConfiguration).As<DwcaConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.ClamServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.ClamServiceConfiguration).As<ClamServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.FishDataServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.FishDataServiceConfiguration).As<FishDataServiceConfiguration>()
                    .SingleInstance();
            if (Configurations.ImportConfiguration.KulServiceConfiguration != null)
                builder.RegisterInstance(Configurations.ImportConfiguration.KulServiceConfiguration).As<KulServiceConfiguration>()
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

            // Vebatim Mongo Db
            var verbatimSettings = Configurations.VerbatimDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IVerbatimClient>(new VerbatimClient(verbatimSettings, Configurations.VerbatimDbConfiguration.DatabaseName,
                Configurations.VerbatimDbConfiguration.ReadBatchSize, Configurations.VerbatimDbConfiguration.WriteBatchSize)).SingleInstance();

            // Processed Mongo Db
            var processedSettings = Configurations.ProcessDbConfiguration.GetMongoDbSettings();
            builder.RegisterInstance<IProcessClient>(new ProcessClient(processedSettings, Configurations.ProcessDbConfiguration.DatabaseName,
                Configurations.ProcessDbConfiguration.ReadBatchSize, Configurations.ProcessDbConfiguration.WriteBatchSize)).SingleInstance();

            // Darwin Core
            builder.RegisterType<DwcArchiveReader>().As<IDwcArchiveReader>().InstancePerLifetimeScope();

            // Containers, single instance for best performance (re-init on full harvest)
            builder.RegisterType<ArtportalenMetadataContainer>().As<IArtportalenMetadataContainer>().SingleInstance();

            // Managers
            builder.RegisterType<ReportManager>().As<IReportManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerLifetimeScope();
            builder.RegisterType<DwcaDataValidationReportManager>().As<IDwcaDataValidationReportManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataValidationReportManager>().As<IDataValidationReportManager>().InstancePerLifetimeScope();

            // Repositories source
            builder.RegisterType<Repositories.Source.Artportalen.AreaRepository>().As<Repositories.Source.Artportalen.Interfaces.IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MetadataRepository>().As<IMetadataRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseRepository>().As<IObservationDatabaseRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRepository>().As<ISightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SiteRepository>().As<ISiteRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationRepository>().As<IOrganizationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PersonRepository>().As<IPersonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRelationRepository>().As<ISightingRelationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SpeciesCollectionItemRepository>().As<ISpeciesCollectionItemRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveVerbatimRepository>().As<IDarwinCoreArchiveVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveEventRepository>().As<IDarwinCoreArchiveEventRepository>()
                .InstancePerLifetimeScope();

            // Repositories destination
            builder.RegisterType<ArtportalenVerbatimRepository>().As<IArtportalenVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<Lib.Repositories.Resource.AreaRepository>().As<Lib.Repositories.Resource.Interfaces.IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ReportRepository>().As<IReportRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VocabularyRepository>().As<IVocabularyRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationVerbatimRepository>().As<IFishDataObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationVerbatimRepository>().As<IMvmObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationVerbatimRepository>().As<INorsObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseVerbatimRepository>().As<IObservationDatabaseVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessedPublicObservationRepository>().As<IProcessedPublicObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SersObservationVerbatimRepository>().As<ISersObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationVerbatimRepository>().As<ISharkObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationVerbatimRepository>()
                .As<IVirtualHerbariumObservationVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories resource
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().InstancePerLifetimeScope();

            // Add harvesters
            builder.RegisterType<AreaHarvester>().As<IAreaHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenObservationHarvester>().As<IArtportalenObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalObservationHarvester>().As<IClamPortalObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcObservationHarvester>().As<IDwcObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<VocabularyHarvester>().As<IVocabularyHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationHarvester>().As<IFishDataObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationHarvester>().As<IKulObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationHarvester>().As<IMvmObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationHarvester>().As<INorsObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationDatabaseHarvester>().As<IObservationDatabaseHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationHarvester>().As<ISersObservationHarvester>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationHarvester>().As<ISharkObservationHarvester>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationHarvester>().As<IVirtualHerbariumObservationHarvester>()
                .InstancePerLifetimeScope();

            // Add factories
            builder.RegisterType<ActivityVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<GenderVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LifeStageVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ReproductiveConditionVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BehaviorVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BiotopeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SubstrateVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ValidationStatusVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<InstitutionVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UnitVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BasisOfRecordVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ContinentVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TypeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CountryVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AccessRightsVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OccurrenceStatusVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EstablishmentMeansVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AreaTypeVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DiscoveryMethodVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DeterminationMethodVocabularyFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DwcaDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<MvmDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<KulDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<NorsDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SersDataValidationReportFactory>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumValidationReportFactory>().InstancePerLifetimeScope();

            // Add Services
            builder.RegisterType<ArtportalenDataService>().As<IArtportalenDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationService>().As<IClamObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationService>().As<IFishDataObservationService>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientService>().As<IHttpClientService>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationService>().As<IKulObservationService>().InstancePerLifetimeScope();
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

            // Add jobs
            builder.RegisterType<AreasHarvestJob>().As<IAreasHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<DwcArchiveHarvestJob>().As<IDwcArchiveHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<VocabulariesImportJob>().As<IVocabulariesImportJob>().InstancePerLifetimeScope();
            builder.RegisterType<ObservationsHarvestJob>().As<IObservationsHarvestJob>().InstancePerLifetimeScope();
            builder.RegisterType<CreateDwcaDataValidationReportJob>().As<ICreateDwcaDataValidationReportJob>().InstancePerLifetimeScope();
            builder.RegisterType<DataValidationReportJob>().As<IDataValidationReportJob>().InstancePerLifetimeScope();
        }
    }
}