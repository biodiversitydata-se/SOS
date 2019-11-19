﻿using Autofac;
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
using SOS.Process.Services;
using SOS.Process.Services.Interfaces;

namespace SOS.Process.IoC.Modules
{
    public class ProcessModule : Module
    {
        public ProcessConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Configuration
            builder.RegisterInstance(Configuration.Runtime).As<RunTimeConfiguration>().SingleInstance();

            // Vebatim Mongo Db
            var verbatimDbConfiguration = Configuration.VerbatimDbConfiguration;
            var verbatimSettings = verbatimDbConfiguration.GetMongoDbSettings();
            var verbatimClient = new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.BatchSize);
            builder.RegisterInstance(verbatimClient).As<IVerbatimClient>().SingleInstance();

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.ProcessedDbConfiguration;
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.BatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();

            // Helpers
            builder.RegisterType<AreaNameMapper>().As<IAreaNameMapper>().SingleInstance();
            builder.RegisterType<AreaHelper>().As<IAreaHelper>().SingleInstance();

            // Repositories source
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<KulObservationVerbatimRepository>().As<IKulObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalVerbatimRepository>().As<ISpeciesPortalVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TaxonVerbatimRepository>().As<ITaxonVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TreeObservationVerbatimRepository>().As<ITreeObservationVerbatimRepository>().InstancePerLifetimeScope();
            
            // Repositories destination
            builder.RegisterType<ProcessedRepository>().As<IProcessedRepository>().InstancePerLifetimeScope();
          
            // Add factories
            builder.RegisterType<ClamTreePortalProcessFactory>().As<IClamTreePortalProcessFactory>().InstancePerLifetimeScope();
            builder.RegisterType<KulProcessFactory>().As<IKulProcessFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalProcessFactory>().As<ISpeciesPortalProcessFactory>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<RuntimeService>().As<IRuntimeService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ProcessJob>().As<IProcessJob>().InstancePerLifetimeScope();
        }
    }
}
