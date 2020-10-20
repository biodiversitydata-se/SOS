using Autofac;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Managers;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Mappings;
using SOS.Process.Mappings.Interfaces;
using SOS.Process.Processors.Artportalen;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.ClamPortal;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Processors.FishData;
using SOS.Process.Processors.FishData.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Mvm;
using SOS.Process.Processors.Mvm.Interfaces;
using SOS.Process.Processors.Nors;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Processors.Shark;
using SOS.Process.Processors.Shark.Interfaces;
using SOS.Process.Processors.Taxon;
using SOS.Process.Processors.Taxon.Interfaces;
using SOS.Process.Processors.VirtualHerbarium;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;
using SOS.Process.Services;
using SOS.Process.Services.Interfaces;
using DataProviderRepository = SOS.Lib.Repositories.Processed.DataProviderRepository;
using IDataProviderRepository = SOS.Lib.Repositories.Processed.Interfaces.IDataProviderRepository;


namespace SOS.Process.IoC.Modules
{
    public class ProcessModule : Module
    {
        public (ProcessConfiguration ProcessConfiguration, MongoDbConfiguration VerbatimDbConfiguration, MongoDbConfiguration ProcessDbConfiguration) Configurations { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Field mapping processing configuration
            if (Configurations.ProcessConfiguration.FieldMapping != null)
            {
                builder.RegisterInstance(Configurations.ProcessConfiguration.FieldMapping).As<FieldMappingConfiguration>().SingleInstance();
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

            // Helpers, single instance since static data
            builder.RegisterType<AreaNameMapper>().As<IAreaNameMapper>().SingleInstance();
            builder.RegisterType<AreaHelper>().As<IAreaHelper>().SingleInstance();
            builder.RegisterType<FieldMappingDiffHelper>().As<IFieldMappingDiffHelper>().SingleInstance();
            builder.RegisterType<FieldMappingResolverHelper>().As<IFieldMappingResolverHelper>().SingleInstance();

            // Repositories source
            builder.RegisterType<ArtportalenVerbatimRepository>().As<IArtportalenVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DarwinCoreArchiveVerbatimRepository>().As<IDarwinCoreArchiveVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationVerbatimRepository>().As<IFishDataObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationVerbatimRepository>().As<IMvmObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationVerbatimRepository>().As<INorsObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SersObservationVerbatimRepository>().As<ISersObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationVerbatimRepository>().As<ISharkObservationVerbatimRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationVerbatimRepository>()
                .As<IVirtualHerbariumObservationVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories destination 
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<InvalidObservationRepository>().As<IInvalidObservationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingRepository>().As<IFieldMappingRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderRepository>().As<IDataProviderRepository>().InstancePerLifetimeScope();

            // Add processors
            builder.RegisterType<ArtportalenObservationProcessor>().As<IArtportalenObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DwcaObservationProcessor>().As<IDwcaObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalObservationProcessor>().As<IClamPortalObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<FishDataObservationProcessor>().As<IFishDataObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationProcessor>().As<IKulObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<MvmObservationProcessor>().As<IMvmObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationProcessor>().As<INorsObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationProcessor>().As<ISersObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationProcessor>().As<ISharkObservationProcessor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationProcessor>().As<IVirtualHerbariumObservationProcessor>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TaxonProcessor>().As<ITaxonProcessor>()
                .InstancePerLifetimeScope();

            // Add managers
            builder.RegisterType<InstanceManager>().As<IInstanceManager>().InstancePerLifetimeScope();
            builder.RegisterType<DataProviderManager>().As<IDataProviderManager>().InstancePerLifetimeScope();
            builder.RegisterType<ValidationManager>().As<IValidationManager>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ActivateInstanceJob>().As<IActivateInstanceJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyProviderDataJob>().As<ICopyProviderDataJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessJob>().As<IProcessJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTaxaJob>().As<IProcessTaxaJob>().InstancePerLifetimeScope();

            // Add services
            builder.RegisterType<TaxonAttributeService>().As<ITaxonAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonService>().As<ITaxonService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonServiceProxy>().As<ITaxonServiceProxy>().InstancePerLifetimeScope();
        }
    }
}