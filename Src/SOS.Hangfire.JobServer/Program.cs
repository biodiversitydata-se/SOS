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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            if (new[] {"local", "dev", "st", "prod"}.Contains(_env, StringComparer.CurrentCultureIgnoreCase))
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
                            .UseMongoStorage(hangfireDbConfiguration.GetMongoDbSettings(),
                                hangfireDbConfiguration.DatabaseName,
                                new MongoStorageOptions
                                {
                                    MigrationOptions = new MongoMigrationOptions
                                    {
                                        Strategy = MongoMigrationStrategy.Migrate,
                                        BackupStrategy = MongoBackupStrategy.Collections
                                    }
                                })
                    );
                    GlobalJobFilters.Filters.Add(
                        new HangfireJobExpirationTimeAttribute(hangfireDbConfiguration.JobExpirationDays));

                    // Add the processing server as IHostedService
                    services.AddHangfireServer();

                    //setup the elastic search configuration
                    var elasticConfiguration = hostContext.Configuration.GetSection("ProcessConfiguration")
                        .GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
                    var uris = elasticConfiguration.Hosts.Select(u => new Uri(u));
                    services.AddSingleton<IElasticClient>(
                        new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris))));
                    services.AddSingleton(elasticConfiguration);
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        _importConfiguration = hostContext.Configuration.GetSection(typeof(ImportConfiguration).Name)
                            .Get<ImportConfiguration>();
                        _processConfiguration = hostContext.Configuration.GetSection(typeof(ProcessConfiguration).Name)
                            .Get<ProcessConfiguration>();
                        _exportConfiguration = hostContext.Configuration.GetSection(typeof(ExportConfiguration).Name)
                            .Get<ExportConfiguration>();

                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule(new ImportModule {Configuration = _importConfiguration})
                                .RegisterModule(new ProcessModule {Configuration = _processConfiguration})
                                .RegisterModule(new ExportModule {Configuration = _exportConfiguration})
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
                $"[MongoDb].[Servers]: {string.Join(", ", _importConfiguration.VerbatimDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_importConfiguration.VerbatimDbConfiguration.DatabaseName}");
            sb.AppendLine($"[MongoDb].[BatchSize]: {_importConfiguration.VerbatimDbConfiguration.BatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Process settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[ProcessedDb].[Servers]: {string.Join(", ", _processConfiguration.ProcessedDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine(
                $"[ProcessedDb].[DatabaseName]: {_processConfiguration.ProcessedDbConfiguration.DatabaseName}");
            sb.AppendLine(
                $"[VerbatimDb].[Servers]: {string.Join(", ", _processConfiguration.VerbatimDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[VerbatimDb].[DatabaseName]: {_processConfiguration.VerbatimDbConfiguration.DatabaseName}");
            sb.AppendLine($"[VerbatimDb].[BatchSize]: {_processConfiguration.VerbatimDbConfiguration.BatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Export settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[BlobStorage].[ConnectionString]: {_exportConfiguration.BlobStorageConfiguration.ConnectionString}");
            sb.AppendLine(
                $"[MongoDb].[Servers]: {string.Join(", ", _exportConfiguration.ProcessedDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_exportConfiguration.ProcessedDbConfiguration.DatabaseName}");
            sb.AppendLine($"[FileDestination].[Path]: {_exportConfiguration.FileDestination.Path}");

            logger.LogInformation(sb.ToString());
        }
    }
}