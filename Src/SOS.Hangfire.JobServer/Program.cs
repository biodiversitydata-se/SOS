using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Elasticsearch.Net;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nest;
using NLog.Web;
using SOS.Export.IoC.Modules;
using SOS.Hangfire.JobServer.Configuration;
using SOS.Import.IoC.Modules;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Process.IoC.Modules;

namespace SOS.Hangfire.JobServer
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Program
    {
        private static string _env;
        private static MongoDbConfiguration _verbatimDbConfiguration;
        private static MongoDbConfiguration _processDbConfiguration;
        private static ElasticSearchConfiguration _searchDbConfiguration;
        private static ImportConfiguration _importConfiguration;
        private static ProcessConfiguration _processConfiguration;
        private static ExportConfiguration _exportConfiguration;

        /// <summary>
        ///     Application entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            _env = args?.Any() ?? false
                ? args[0].ToLower()
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();

            if (new[] { "local", "dev", "st", "prod" }.Contains(_env, StringComparer.CurrentCultureIgnoreCase))
            {
                var host = CreateHostBuilder(args).Build();
                LogStartupSettings(host.Services.GetService<ILogger<Program>>());
                await host.RunAsync();
            }
        }

        /// <summary>
        ///     Create a host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{_env}.json", false, true)
                        .AddEnvironmentVariables();

                    // If Development mode, add secrets stored on developer machine 
                    // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                    // In production you should store the secret values as environment variables.
                    configuration.AddUserSecrets<Program>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .AddNLog($"nlog.{_env}.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var hangfireDbConfiguration = hostContext.Configuration.GetSection("ApplicationSettings")
                        .GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();

                    services.AddHangfire(configuration =>
                        configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseMongoStorage(new MongoClient(hangfireDbConfiguration.GetMongoDbSettings()),
                                hangfireDbConfiguration.DatabaseName,
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

                    GlobalJobFilters.Filters.Add(
                        new HangfireJobExpirationTimeAttribute(hangfireDbConfiguration.JobExpirationDays));

                    // Add the processing server as IHostedService
                    services.AddHangfireServer();

                    // Get configuration
                    _verbatimDbConfiguration = hostContext.Configuration.GetSection("ApplicationSettings")
                        .GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
                    _processDbConfiguration = hostContext.Configuration.GetSection("ApplicationSettings")
                        .GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
                    _searchDbConfiguration = hostContext.Configuration.GetSection("ApplicationSettings")
                        .GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
                    _importConfiguration = hostContext.Configuration.GetSection(nameof(ImportConfiguration))
                        .Get<ImportConfiguration>();
                    _processConfiguration = hostContext.Configuration.GetSection(nameof(ProcessConfiguration))
                        .Get<ProcessConfiguration>();
                    _exportConfiguration = hostContext.Configuration.GetSection(nameof(ExportConfiguration))
                        .Get<ExportConfiguration>();

                    //setup the elastic search configuration
                    var uris = _searchDbConfiguration.Hosts.Select(u => new Uri(u));
                    services.AddSingleton<IElasticClient>(
                        new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris))));
                    services.AddSingleton(_searchDbConfiguration);
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule(new ImportModule { Configurations = (_importConfiguration, _verbatimDbConfiguration, _processDbConfiguration) })
                                .RegisterModule(new ProcessModule { Configurations = (_processConfiguration, _verbatimDbConfiguration, _processDbConfiguration) })
                                .RegisterModule(new ExportModule { Configurations = (_exportConfiguration, _processDbConfiguration) })
                        );
                    }
                )
                .UseNLog();
        }

        private static void LogStartupSettings(ILogger<Program> logger)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Hangfire JobServer Started with the following settings:");
            sb.AppendLine("Import settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[KULSettings].[StartHarvestYear]: {_importConfiguration.KulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine(
                $"[KULSettings].[MaxNumberOfSightingsHarvested]: {_importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine(
                $"[ArtportalenSettings].[MaxNumberOfSightingsHarvested]: {_importConfiguration.ArtportalenConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine(
                $"[ArtportalenSettings].[ChunkSize]: {_importConfiguration.ArtportalenConfiguration.ChunkSize}");
            sb.AppendLine(
                $"[ArtportalenSettings].[ConnectionString]: {_importConfiguration.ArtportalenConfiguration.ConnectionStringBackup}");
            sb.AppendLine($"[ClamService].[Address]: {_importConfiguration.ClamServiceConfiguration.BaseAddress}");
            sb.AppendLine(
                $"[TaxonAttributeService].[Address]: {_importConfiguration.TaxonAttributeServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[TaxonService].[Address]: {_importConfiguration.TaxonServiceConfiguration.BaseAddress}");
            sb.AppendLine(
                $"[MongoDb].[Servers]: {string.Join(", ", _verbatimDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_verbatimDbConfiguration.DatabaseName}");
            sb.AppendLine($"[MongoDb].[ReadBatchSize]: {_verbatimDbConfiguration.ReadBatchSize}");
            sb.AppendLine($"[MongoDb].[WriteBatchSize]: {_verbatimDbConfiguration.WriteBatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Process settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[ProcessedDb].[Servers]: {string.Join(", ", _processDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine(
                $"[ProcessedDb].[DatabaseName]: {_processDbConfiguration.DatabaseName}");
            sb.AppendLine(
                $"[VerbatimDb].[Servers]: {string.Join(", ", _processDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[VerbatimDb].[DatabaseName]: {_processDbConfiguration.DatabaseName}");
            sb.AppendLine($"[VerbatimDb].[ReadBatchSize]: {_processDbConfiguration.ReadBatchSize}");
            sb.AppendLine($"[VerbatimDb].[WriteBatchSize]: {_processDbConfiguration.WriteBatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Export settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[BlobStorage].[ConnectionString]: {_exportConfiguration.BlobStorageConfiguration.ConnectionString}");
            sb.AppendLine(
                $"[MongoDb].[Servers]: {string.Join(", ", _searchDbConfiguration.Hosts.Select(x => x))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_processDbConfiguration.DatabaseName}");
            sb.AppendLine($"[FileDestination].[Path]: {_exportConfiguration.FileDestination.Path}");

            logger.LogInformation(sb.ToString());
        }
    }
}