using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Hosting;

namespace SOS.Hangfire.JobServer
{
    class Program
    {
        // SOS.Hangfire.UI
        // SOS.Hangfire.Api
        static async Task Main(string[] args)
        {
            GlobalConfiguration.Configuration.UseMongoStorage("mongodb://localhost", "sos-jobs-st", MongoStorageOptions);
            var hostBuilder = new HostBuilder()
                // Add configuration, logging, ...
                .ConfigureServices((hostContext, services) =>
                {
                    // Add your services for depedency injection.
                });

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
