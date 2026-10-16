using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SOS.Lib.Extensions;

namespace SOS.Administration.Api.Extensions;

public static class HangfireExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection SetupHangfire(bool useLocalHangfire, string hangfireDbConnectionString)
        {
            var mongoConfiguration = useLocalHangfire
                ? Settings.LocalHangfireDbConfiguration
                : Settings.HangfireDbConfiguration;

            var mongoClientSettings = !string.IsNullOrEmpty(hangfireDbConnectionString)
                ? MongoClientSettings.FromConnectionString(hangfireDbConnectionString)
                : mongoConfiguration.GetMongoDbSettings();

            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseSosHangfireSerializerSettings()
                    .UseMongoStorage(new MongoClient(mongoClientSettings),
                        mongoConfiguration.DatabaseName,
                        new MongoStorageOptions
                        {
                            CheckQueuedJobsStrategy = (mongoConfiguration?.Hosts?.Length ?? 0) < 2 ? CheckQueuedJobsStrategy.TailNotificationsCollection : CheckQueuedJobsStrategy.Watch,
                            JobExpirationCheckInterval = TimeSpan.FromMinutes(10),
                            QueuePollInterval = TimeSpan.FromSeconds(10), // Deafault 15
                            MigrationOptions =
                            {
                                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                                BackupStrategy = new CollectionMongoBackupStrategy()
                            },                            
                            Prefix = "hangfire",
                            CheckConnection = true
                        })
            );

            return services;
        }
    }
}
