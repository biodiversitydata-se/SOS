using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;

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
                    .UseRecommendedSerializerSettings(m =>
                    {
                        m.Converters.Add(new NetTopologySuite.IO.Converters.GeometryConverter());
                        m.Converters.Add(new StringEnumConverter());
                    })
                    .UseMongoStorage(new MongoClient(mongoClientSettings),
                        mongoConfiguration.DatabaseName,
                        new MongoStorageOptions
                        {
                            CheckQueuedJobsStrategy = (mongoConfiguration?.Hosts?.Length ?? 0) < 2 ? CheckQueuedJobsStrategy.TailNotificationsCollection : CheckQueuedJobsStrategy.Watch,
                            JobExpirationCheckInterval = TimeSpan.FromMinutes(10),
                            QueuePollInterval = TimeSpan.FromSeconds(10), // Deafault 15
                            MigrationOptions = new MongoMigrationOptions
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