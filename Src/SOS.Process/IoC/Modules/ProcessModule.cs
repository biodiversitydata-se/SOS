using Autofac;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Jobs.Process;
using SOS.Process.Database;
using SOS.Process.Database.Interfaces;
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
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.Kul;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Nors;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Processors.Shark;
using SOS.Process.Processors.Shark.Interfaces;
using SOS.Process.Processors.VirtualHerbarium;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.IoC.Modules
{
    public class ProcessModule : Module
    {
        public ProcessConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Vebatim Mongo Db
            var verbatimDbConfiguration = Configuration.VerbatimDbConfiguration;
            var verbatimSettings = verbatimDbConfiguration.GetMongoDbSettings();
            var verbatimClient = new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.BatchSize);
            builder.RegisterInstance(verbatimClient).As<IVerbatimClient>().SingleInstance();

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.ProcessedDbConfiguration;
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName, processedDbConfiguration.BatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();

            // Field mapping processing configuration
            if (Configuration.FieldMapping != null)
            {
                builder.RegisterInstance(Configuration.FieldMapping).As<FieldMappingConfiguration>().SingleInstance();
            }

            builder.RegisterInstance(Configuration).As<ProcessConfiguration>().SingleInstance();

            // Helpers
            builder.RegisterType<AreaNameMapper>().As<IAreaNameMapper>().SingleInstance();
            builder.RegisterType<AreaHelper>().As<IAreaHelper>().SingleInstance();
            builder.RegisterType<FieldMappingDiffHelper>().As<IFieldMappingDiffHelper>().SingleInstance();
            builder.RegisterType<FieldMappingResolverHelper>().As<IFieldMappingResolverHelper>().SingleInstance();
            builder.RegisterType<AreaDiffHelper>().As<IAreaDiffHelper>().SingleInstance();

            // Repositories source
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ArtportalenVerbatimRepository>().As<IArtportalenVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<DwcaVerbatimRepository>().As<IDwcaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingVerbatimRepository>().As<IFieldMappingVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationVerbatimRepository>().As<INorsObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationVerbatimRepository>().As<ISersObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationVerbatimRepository>().As<ISharkObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationVerbatimRepository>().As<IVirtualHerbariumObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonVerbatimRepository>().As<ITaxonVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories destination 
            builder.RegisterType<ProcessedObservationRepository>().As<IProcessedObservationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InvalidObservationRepository>().As<IInvalidObservationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedTaxonRepository>().As<IProcessedTaxonRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedFieldMappingRepository>().As<IProcessedFieldMappingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedAreaRepository>().As<IProcessedAreaRepository>().InstancePerLifetimeScope();

            // Add processors
            builder.RegisterType<ArtportalenObservationProcessor>().As<IArtportalenObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<DwcaObservationProcessor>().As<IDwcaObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<ClamPortalObservationProcessor>().As<IClamPortalObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<InstanceManager>().As<IInstanceManager>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationProcessor>().As<IKulObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<NorsObservationProcessor>().As<INorsObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SersObservationProcessor>().As<ISersObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<SharkObservationProcessor>().As<ISharkObservationProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<VirtualHerbariumObservationProcessor>().As<IVirtualHerbariumObservationProcessor>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ActivateInstanceJob>().As<IActivateInstanceJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyProviderDataJob>().As<ICopyProviderDataJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessJob>().As<IProcessJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTaxaJob>().As<IProcessTaxaJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyFieldMappingsJob>().As<ICopyFieldMappingsJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyAreasJob>().As<ICopyAreasJob>().InstancePerLifetimeScope();
        }
    }
}