using System;
using System.Linq;
using System.Security.Authentication;
using Autofac;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SOS.Import.Configuration;
using SOS.Import.Factories;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs;
using SOS.Import.Jobs.Interfaces;
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
        protected override void Load(ContainerBuilder builder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            // Add configuration
            var importConfiguration = configuration.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
            builder.RegisterInstance(importConfiguration.MongoDbConfiguration).As<MongoDbConfiguration>().SingleInstance();
            builder.RegisterInstance(importConfiguration.ConnectionStrings).As<ConnectionStrings>().SingleInstance();

            // Init mongodb
            SetUpMongoDb(builder, importConfiguration.MongoDbConfiguration);

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

        private void SetUpMongoDb(ContainerBuilder builder, MongoDbConfiguration mongoDBConfiguration)
        {
            var settings = new MongoClientSettings
            {
                Servers = mongoDBConfiguration.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                ReplicaSetName = mongoDBConfiguration.ReplicaSetName,
                UseTls = mongoDBConfiguration.UseTls,
                SslSettings = mongoDBConfiguration.UseTls ? new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                } : null
            };

            if (!string.IsNullOrEmpty(mongoDBConfiguration.DatabaseName) &&
                !string.IsNullOrEmpty(mongoDBConfiguration.Password))
            {
                var identity = new MongoInternalIdentity(mongoDBConfiguration.DatabaseName, mongoDBConfiguration.UserName);
                var evidence = new PasswordEvidence(mongoDBConfiguration.Password);

                settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);
            }
            // Setup Mongo Db
            var mongoClient = new MongoClient(settings);
            builder.RegisterInstance(mongoClient).As<IMongoClient>().SingleInstance();
        }
    }
}
