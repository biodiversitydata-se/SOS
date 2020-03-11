using Autofac;
using SOS.Lib.Configuration.Process;
using SOS.Process.Database;
using SOS.Process.Database.Interfaces;
using SOS.Process.Factories;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Mappings;
using SOS.Process.Mappings.Interfaces;
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

            // Repositories source
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<HarvestInfoRepository>().As<IHarvestInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalVerbatimRepository>().As<ISpeciesPortalVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonVerbatimRepository>().As<ITaxonVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FieldMappingVerbatimRepository>().As<IFieldMappingVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories destination 
            builder.RegisterType<ProcessedSightingRepository>().As<IProcessedSightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InadequateItemRepository>().As<IInadequateItemRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessInfoRepository>().As<IProcessInfoRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonProcessedRepository>().As<ITaxonProcessedRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessedFieldMappingRepository>().As<IProcessedFieldMappingRepository>().InstancePerLifetimeScope();

            // Add factories
            builder.RegisterType<ClamPortalProcessFactory>().As<IClamPortalProcessFactory>().InstancePerLifetimeScope();
            builder.RegisterType<InstanceFactory>().As<IInstanceFactory>().InstancePerLifetimeScope();
            builder.RegisterType<KulProcessFactory>().As<IKulProcessFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalProcessFactory>().As<ISpeciesPortalProcessFactory>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ActivateInstanceJob>().As<IActivateInstanceJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyProviderDataJob>().As<ICopyProviderDataJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessJob>().As<IProcessJob>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessTaxaJob>().As<IProcessTaxaJob>().InstancePerLifetimeScope();
            builder.RegisterType<CopyFieldMappingsJob>().As<ICopyFieldMappingsJob>().InstancePerLifetimeScope();
        }
    }
}
