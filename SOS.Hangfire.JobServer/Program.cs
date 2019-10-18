using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SOS.Core;
using SOS.Core.IoC.Modules;
using SOS.Core.Repositories;
using SOS.Hangfire.JobServer.Configuration;
using SOS.Hangfire.JobServer.MyApplication.Common;
using SOS.Import.IoC.Modules;
using SOS.Process.IoC.Modules;

namespace SOS.Hangfire.JobServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration configuration = ConfigurationFactory.CreateConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterModule<CoreModule>();
            builder.RegisterModule<ImportModule>();
            builder.RegisterModule<ProcessModule>();

            builder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            var mongoConfiguration = configuration.GetSection("ApplicationSettings").GetSection("MongoDbRepository").Get<MongoDbConfiguration>();
            
            var repositorySettings = new RepositorySettings()
            {
                JobsDatabaseName = mongoConfiguration.DatabaseName,
                MongoDbConnectionString = $"mongodb://{string.Join(",", mongoConfiguration.Hosts.Select(h => h.Name))}"
            };
            SystemSettings.InitSettings(repositorySettings);
            builder.Register(r => repositorySettings).As<IRepositorySettings>().SingleInstance();

            GlobalConfiguration.Configuration.UseMongoStorage(
                repositorySettings.MongoDbConnectionString, 
                repositorySettings.JobsDatabaseName, 
                MongoStorageOptions);

            var hostBuilder = new HostBuilder()
                // Add configuration, logging, ...
                .ConfigureServices((hostContext, services) => { })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                })
                .UseNLog();
            
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
