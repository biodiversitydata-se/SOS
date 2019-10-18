using System;
using System.Linq;
using System.Security.Authentication;
using Autofac;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SOS.Process.Configuration;
using SOS.Process.Database;
using SOS.Process.Database.Interfaces;
using SOS.Process.Factories;
using SOS.Process.Factories.Interfaces;
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
        private static IServiceProvider ServiceProvider;

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
           /* var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection(nameof(ProcessConfiguration.AppSettings)));

            ServiceProvider = serviceCollection.BuildServiceProvider();
            builder.Register(context => ServiceProvider.GetService<IOptions<AppSettings>>());
            */
            var processConfiguration = configuration.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();

            builder.Register(r => processConfiguration.AppSettings).As<AppSettings>().SingleInstance();

            // Vebatim Mongo Db
            var verbatimDbConfiguration = processConfiguration.VerbatimDbConfiguration;
            var verbatimSettings = GetMongDbSettings(verbatimDbConfiguration);
            var verbatimClient = new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.BatchSize);
            builder.RegisterInstance(verbatimClient).As<IVerbatimClient>().SingleInstance();

            // Processed Mongo Db
            var processedDbConfiguration = processConfiguration.ProcessedDbConfiguration;
            var processedSettings = GetMongDbSettings(processedDbConfiguration);
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName, processedDbConfiguration.BatchSize);
            builder.RegisterInstance(processClient).As<IProcessClient>().SingleInstance();
            
            // Repositories source
            builder.RegisterType<SpeciesPortalVerbatimRepository>().As<ISpeciesPortalVerbatimRepository>().InstancePerLifetimeScope();

            // Repositories destination
            builder.RegisterType<ProcessedRepository>().As<IProcessedRepository>().InstancePerLifetimeScope();
          
            // Add factories
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
