using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;
using SOS.Lib.Extensions;

namespace SOS.Analysis.Api.Extensions;

public static class HangfireExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection SetupHangfire(bool useLocalHangfire, string? hangfireDbConnectionString)
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
