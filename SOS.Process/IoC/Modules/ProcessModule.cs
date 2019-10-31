using System.Linq;
using System.Security.Authentication;
using Autofac;
using MongoDB.Driver;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Process.Database;
using SOS.Process.Database.Interfaces;
using SOS.Process.Factories;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Helpers;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs;
using SOS.Process.Jobs.Interfaces;
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
            // Add configuration
            builder.Register(r => Configuration.AppSettings).As<AppSettings>().SingleInstance();

            // Vebatim Mongo Db
            var verbatimDbConfiguration = Configuration.VerbatimDbConfiguration;
            var verbatimSettings = GetMongDbSettings(verbatimDbConfiguration);
            var verbatimClient = new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.BatchSize);
            builder.RegisterInstance(verbatimClient).As<IVerbatimClient>().SingleInstance();

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.ProcessedDbConfiguration;
            var processedSettings = GetMongDbSettings(processedDbConfiguration);
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName, processedDbConfiguration.BatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();

            // Helpers
            builder.RegisterType<AreaHelper>().As<IAreaHelper>().SingleInstance();

            // Repositories source
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClamObservationVerbatimRepository>().As<IClamObservationVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalVerbatimRepository>().As<ISpeciesPortalVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TreeObservationVerbatimRepository>().As<ITreeObservationVerbatimRepository>().InstancePerLifetimeScope();
            
            // Repositories destination
            builder.RegisterType<ProcessedRepository>().As<IProcessedRepository>().InstancePerLifetimeScope();
          
            // Add factories
            builder.RegisterType<ClamTreePortalProcessFactory>().As<IClamTreePortalProcessFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SpeciesPortalProcessFactory>().As<ISpeciesPortalProcessFactory>().InstancePerLifetimeScope();
            
            // Add Services
            builder.RegisterType<TaxonService>().As<ITaxonService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ProcessJob>().As<IProcessJob>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Get mongo db settings object
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static MongoClientSettings GetMongDbSettings(MongoDbConfiguration config)
        {
            MongoInternalIdentity identity = null;
            PasswordEvidence evidence = null;
            if (!(string.IsNullOrEmpty(config.DatabaseName) ||
                  string.IsNullOrEmpty(config.UserName) ||
                  string.IsNullOrEmpty(config.Password)))
            {
                identity = new MongoInternalIdentity(config.DatabaseName, config.UserName);
                evidence = new PasswordEvidence(config.Password);
            }

            return new MongoClientSettings
            {
                Servers = config.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                ReplicaSetName = config.ReplicaSetName,
                UseTls = config.UseTls,
                SslSettings = config.UseTls ? new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                } : null,
                Credential = identity != null && evidence != null ? new MongoCredential("SCRAM-SHA-1", identity, evidence) : null
            };
        }
    }
}
