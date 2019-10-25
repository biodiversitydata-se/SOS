using System.Linq;
using System.Security.Authentication;
using Autofac;
using MongoDB.Driver;
using SOS.Export.Factories;
using SOS.Export.Factories.Interfaces;
using SOS.Export.Jobs;
using SOS.Export.Jobs.Interfaces;
using SOS.Export.MongoDb;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;

namespace SOS.Export.IoC.Modules
{
    /// <summary>
    /// Export module
    /// </summary>
    public class ExportModule : Module
    {
        /// <summary>
        /// Module configuration
        /// </summary>
        public ExportConfiguration Configuration { get; set; }

        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Add configuration
            builder.RegisterInstance(Configuration.FileDestination).As<FileDestination>().SingleInstance();

            // Init mongodb
            var exportSettings = GetMongDbSettings(Configuration.MongoDbConfiguration);
            var exportClient = new ExportClient(exportSettings, Configuration.MongoDbConfiguration.DatabaseName, Configuration.MongoDbConfiguration.BatchSize);
            builder.RegisterInstance(exportClient).As<IExportClient>().SingleInstance();

            // Add factories
            builder.RegisterType<SightingFactory>().As<ISightingFactory>().InstancePerLifetimeScope();

            // Repositories mongo
            builder.RegisterType<ProcessedDarwinCoreRepository>().As<IProcessedDarwinCoreRepository>().InstancePerLifetimeScope();

            // Services
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();

            // Add jobs
            builder.RegisterType<ExportDarwinCoreJob>().As<IExportDarwinCoreJob>().InstancePerLifetimeScope();
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
