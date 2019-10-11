using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SOS.Core;
using SOS.Core.IoC;
using SOS.Core.IoC.Modules;
using SOS.Core.Jobs;
using SOS.Core.Repositories;
using SOS.Hangfire.JobServer.MyApplication.Common;

namespace SOS.Hangfire.JobServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = ConfigurationFactory.CreateConfiguration();
            var builder = new Autofac.ContainerBuilder();
            builder.RegisterModule<CoreModule>();

            var configurationSection = configuration.GetSection("ApplicationSettings").GetSection("MongoDbRepository");
            var repositorySettings = new RepositorySettings()
            {
                DatabaseName = configurationSection.GetValue<string>("DatabaseName"),
                JobsDatabaseName = configurationSection.GetValue<string>("JobsDatabaseName"),
                MongoDbConnectionString = configurationSection.GetValue<string>("InstanceUrl"),
            };
            SystemSettings.InitSettings(repositorySettings);
            builder.Register(r => repositorySettings).As<IRepositorySettings>().SingleInstance();

            GlobalConfiguration.Configuration.UseMongoStorage(
                repositorySettings.MongoDbConnectionString, 
                repositorySettings.JobsDatabaseName, 
                MongoStorageOptions);

            var hostBuilder = new HostBuilder()
                // Add configuration, logging, ...
                .ConfigureServices((hostContext, services) =>
                {
                    // Add your services for depedency injection.
                    
                });

            IContainer autofacContainer = builder.Build();
            GlobalConfiguration.Configuration.UseAutofacActivator(autofacContainer); // Hangfire

            using (var server = new BackgroundJobServer(new BackgroundJobServerOptions { WorkerCount = 5 }))
            {
                await hostBuilder.RunConsoleAsync();
            }
        }

        private static MongoStorageOptions MongoStorageOptions
        {
            get
            {
                var migrationOptions = new MongoMigrationOptions
                {
                    Strategy = MongoMigrationStrategy.Migrate,
                    BackupStrategy = MongoBackupStrategy.Collections
                };

                var storageOptions = new MongoStorageOptions { MigrationOptions = migrationOptions };
                return storageOptions;
            }
        }
    }
}
