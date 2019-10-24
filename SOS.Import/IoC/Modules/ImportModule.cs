using System.Linq;
using System.Security.Authentication;
using Autofac;
using MongoDB.Driver;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;
using SOS.Import.Factories;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Jobs.Interfaces;
using SOS.Import.MongoDb;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.IoC.Modules
{
    public class ImportModule : Module
    {
        public ImportConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configuration.ConnectionStrings).As<ConnectionStrings>().SingleInstance();

            // Init mongodb
            var importSettings = GetMongDbSettings(Configuration.MongoDbConfiguration);
            var importClient = new ImportClient(importSettings, Configuration.MongoDbConfiguration.DatabaseName, Configuration.MongoDbConfiguration.BatchSize);
            builder.RegisterInstance(importClient).As<IImportClient>().SingleInstance();
            
            // Repositories source
            builder.RegisterType<AreaRepository>().As<IAreaRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MetadataRepository>().As<IMetadataRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectRepository>().As<IProjectRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingRepository>().As<ISightingRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SiteRepository>().As<ISiteRepository>().InstancePerLifetimeScope();

            // Repositories destination
            builder.RegisterType<AreaVerbatimRepository>().As<IAreaVerbatimRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SightingVerbatimRepository>().As<ISightingVerbatimRepository>().InstancePerLifetimeScope();

            // Add factories
            builder.RegisterType<SpeciesPortalSightingFactory>().As<ISpeciesPortalSightingFactory>().InstancePerLifetimeScope();

            // Add Services
            builder.RegisterType<SpeciesPortalDataService>().As<ISpeciesPortalDataService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<SpeciesPortalHarvestJob>().As<ISpeciesPortalHarvestJob>().InstancePerLifetimeScope();
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
