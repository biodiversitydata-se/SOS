using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SOS.Core.IoC.Modules;
using SOS.Export.IoC.Modules;
using SOS.Import.IoC.Modules;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Process.IoC.Modules;

namespace SOS.Hangfire.JobServer
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        private static string _env;
        private static ImportConfiguration _importConfiguration;
        private static ProcessConfiguration _processConfiguration;
        private static ExportConfiguration _exportConfiguration;

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            _env = args?.Any() ?? false ? args[0].ToLower() : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();

            if (new[] { "local", "dev", "st", "prod" }.Contains(_env, StringComparer.CurrentCultureIgnoreCase))
            {
                var host = CreateHostBuilder(args).Build();
                LogStartupSettings(host.Services.GetService<ILogger<Program>>());
                await host.RunAsync();
            }
        }

        /// <summary>
        /// Create a host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{_env}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                   
                    // If Development mode, add secrets stored on developer machine 
                    // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                    // In production you should store the secret values as environment variables.
                    if (_env == "DEV" || _env == "LOCAL")
                    {
                        configuration.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .AddNLog(configFileName: $"nlog.{_env}.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var mongoConfiguration = hostContext.Configuration.GetSection("ApplicationSettings").GetSection("MongoDbRepository").Get<MongoDbConfiguration>();
                    
                    services.AddHangfire(configuration =>
                            configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseMongoStorage(mongoConfiguration.GetMongoDbSettings(),
                                mongoConfiguration.DatabaseName,
                                new MongoStorageOptions
                                {
                                    MigrationOptions = new MongoMigrationOptions
                                    {
                                        Strategy = MongoMigrationStrategy.Migrate,
                                        BackupStrategy = MongoBackupStrategy.Collections
                                    }
                                })
                    );

                    // Add the processing server as IHostedService
                    services.AddHangfireServer();
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        _importConfiguration = hostContext.Configuration.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
                        _processConfiguration = hostContext.Configuration.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();
                        _exportConfiguration  = hostContext.Configuration.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();

                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule<CoreModule>()
                                .RegisterModule(new ImportModule { Configuration = _importConfiguration })
                                .RegisterModule(new ProcessModule { Configuration = _processConfiguration })
                                .RegisterModule(new ExportModule { Configuration = _exportConfiguration })
                        );
                    }
                )
                .UseNLog();

        private static void LogStartupSettings(ILogger<Program> logger)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Hangfire JobServer Started with the following settings:");
            sb.AppendLine("Import settings:");
            sb.AppendLine("================");
            sb.AppendLine($"[KULSettings].[StartHarvestYear]: { _importConfiguration.KulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine($"[KULSettings].[MaxNumberOfSightingsHarvested]: { _importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[SpeciesPortalSettings].[MaxNumberOfSightingsHarvested]: {_importConfiguration.SpeciesPortalConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[SpeciesPortalSettings].[ChunkSize]: {_importConfiguration.SpeciesPortalConfiguration.ChunkSize}");
            sb.AppendLine($"[SpeciesPortalSettings].[ConnectionString]: {_importConfiguration.SpeciesPortalConfiguration.ConnectionString}");
            sb.AppendLine($"[ClamService].[Address]: {_importConfiguration.ClamServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[TaxonAttributeService].[Address]: {_importConfiguration.TaxonAttributeServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[TaxonService].[Address]: {_importConfiguration.TaxonServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[MongoDb].[Servers]: { string.Join(", ", _importConfiguration.MongoDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_importConfiguration.MongoDbConfiguration.DatabaseName}");
            sb.AppendLine($"[MongoDb].[BatchSize]: {_importConfiguration.MongoDbConfiguration.BatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Process settings:");
            sb.AppendLine("================");
            sb.AppendLine($"[ProcessedDb].[Servers]: { string.Join(", ", _processConfiguration.ProcessedDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[ProcessedDb].[DatabaseName]: { _processConfiguration.ProcessedDbConfiguration.DatabaseName}");
            sb.AppendLine($"[VerbatimDb].[Servers]: { string.Join(", ", _processConfiguration.VerbatimDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[VerbatimDb].[DatabaseName]: { _processConfiguration.VerbatimDbConfiguration.DatabaseName}");
            sb.AppendLine($"[VerbatimDb].[BatchSize]: { _processConfiguration.VerbatimDbConfiguration.BatchSize}");
            sb.AppendLine("");

            sb.AppendLine("Export settings:");
            sb.AppendLine("================");
            sb.AppendLine($"[BlobStorage].[ConnectionString]: { _exportConfiguration.BlobStorageConfiguration.ConnectionString}");
            sb.AppendLine($"[MongoDb].[Servers]: { string.Join(", ", _exportConfiguration.MongoDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: { _exportConfiguration.MongoDbConfiguration.DatabaseName}");
            sb.AppendLine($"[FileDestination].[Path]: { _exportConfiguration.FileDestination.Path}");

            logger.LogInformation(sb.ToString());
        }
    }
}