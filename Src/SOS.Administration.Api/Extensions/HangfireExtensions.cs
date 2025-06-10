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
    public static IServiceCollection SetupHangfire(this IServiceCollection services, bool useLocalHangfire, string hangfireDbConnectionString)
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