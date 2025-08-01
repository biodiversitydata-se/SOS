﻿using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;
using NLog.Web;
using SOS.Hangfire.JobServer.Configuration;
using SOS.Hangfire.JobServer.Extensions;
using SOS.Hangfire.JobServer.ServiceBus.Consumers;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Context;
using SOS.Lib.Managers;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Hangfire.JobServer
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Program
    {
        private static string _env;
        private static ApiManagementServiceConfiguration _apiManagementServiceConfiguration;
        private static CryptoConfiguration _cryptoConfiguration;        
        private static MongoDbConfiguration _verbatimDbConfiguration;
        private static MongoDbConfiguration _processDbConfiguration;
        private static ElasticSearchConfiguration _searchDbConfiguration;
        private static ImportConfiguration _importConfiguration;
        private static ProcessConfiguration _processConfiguration;
        private static ExportConfiguration _exportConfiguration;
        private static BlobStorageConfiguration _blobStorageConfiguration;
        private static DataCiteServiceConfiguration _dataCiteServiceConfiguration;
        private static ApplicationInsightsConfiguration _applicationInsightsConfiguration;
        private static SosApiConfiguration _sosApiConfiguration;
        private static UserServiceConfiguration _userServiceConfiguration;
        private static AreaConfiguration _areaConfiguration;
        private static HangfireDbConfiguration _hangfireDbConfiguration;
        private static bool _useLocalHangfire;
        private static string? _hangfireDbConnectionString = null;
        private static bool _excludeParishGeometries = false; // Exclude parish geometries when running tests in order to improve performance.

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
            _useLocalHangfire = GetEnvironmentBool(environmentVariable: "USE_LOCAL_HANGFIRE");
            _excludeParishGeometries = GetEnvironmentBool(environmentVariable: "EXCLUDE_PARISH_GEOMETRIES");
            Console.WriteLine("Starting up in environment:" + _env);


            if (new[] { "local", "dev", "st", "prod", "at" }.Contains(_env, StringComparer.InvariantCultureIgnoreCase))
            {
                var builder = CreateHostBuilder(args);

                var host = builder.Build();
                host.MapDefaultEndpoints();
                HangfireJobServerContext.Host = host;
                LogStartupSettings(host.Services.GetService<ILogger<Program>>());
                LogManager.Logger = host.Services.GetService<ILogger<LogManager>>();
                LogManager.LogInformation("LogManager created");
                host.MapGet("/", () => "Hangfire worker is running.");

                // Start the application
                string aspnetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
                await host.RunAsync();                
            }
        }

        /// <summary>
        ///     Create a host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();
            
            string env = args?.Any() ?? false
                ? args[0].ToLower()
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Logging.AddNLog($"NLog.{env}.config");

            // Use Swedish culture info.
            var culture = new CultureInfo("sv-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            var services = builder.Services;

            ConfigurationManager configuration = builder.Configuration;
            _apiManagementServiceConfiguration = configuration.GetSection("ApiManagementServiceConfiguration").Get<ApiManagementServiceConfiguration>();
            _cryptoConfiguration = configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>();
            _verbatimDbConfiguration = configuration.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            _processDbConfiguration = configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            _searchDbConfiguration = configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            _importConfiguration = configuration.GetSection(nameof(ImportConfiguration)).Get<ImportConfiguration>();
            _processConfiguration = configuration.GetSection(nameof(ProcessConfiguration)).Get<ProcessConfiguration>();
            _exportConfiguration = configuration.GetSection(nameof(ExportConfiguration)).Get<ExportConfiguration>();
            _blobStorageConfiguration = configuration.GetSection(nameof(BlobStorageConfiguration)).Get<BlobStorageConfiguration>();
            _dataCiteServiceConfiguration = configuration.GetSection(nameof(DataCiteServiceConfiguration)).Get<DataCiteServiceConfiguration>();
            _applicationInsightsConfiguration = configuration.GetSection(nameof(ApplicationInsightsConfiguration)).Get<ApplicationInsightsConfiguration>();
            _sosApiConfiguration = configuration.GetSection(nameof(SosApiConfiguration)).Get<SosApiConfiguration>();
            _userServiceConfiguration = configuration.GetSection(nameof(UserServiceConfiguration)).Get<UserServiceConfiguration>();
            _areaConfiguration = configuration.GetSection(nameof(AreaConfiguration)).Get<AreaConfiguration>();
            _hangfireDbConnectionString = builder.Configuration.GetConnectionString("hangfire-mongodb");
            var hangfireDbConfig = configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();
            var localHangfireDbConfiguration = configuration.GetSection("LocalHangfireDbConfiguration").Get<HangfireDbConfiguration>();
            _areaConfiguration.ExcludeParishGeometries = _excludeParishGeometries;

            _hangfireDbConfiguration = _useLocalHangfire
                ? localHangfireDbConfiguration
                : hangfireDbConfig;

            var mongoClientSettings = !string.IsNullOrEmpty(_hangfireDbConnectionString)
                ? MongoClientSettings.FromConnectionString(_hangfireDbConnectionString)
                : _hangfireDbConfiguration.GetMongoDbSettings();

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
                        _hangfireDbConfiguration.DatabaseName,
                        new MongoStorageOptions
                        {
                            CheckConnection = true,
                            CheckQueuedJobsStrategy = (_hangfireDbConfiguration?.Hosts?.Length ?? 0) < 2 ? CheckQueuedJobsStrategy.TailNotificationsCollection : CheckQueuedJobsStrategy.Watch,
                            CountersAggregateInterval = TimeSpan.FromMinutes(10), // Default 5
                                                                                  // ConnectionCheckTimeout = TimeSpan.FromSeconds(5),
                                                                                  //  DistributedLockLifetime = TimeSpan.FromSeconds(30),
                            JobExpirationCheckInterval = TimeSpan.FromMinutes(10),
                            // MigrationLockTimeout = TimeSpan.FromMinutes(1),
                            MigrationOptions = new MongoMigrationOptions
                            {
                                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                                BackupStrategy = new CollectionMongoBackupStrategy()
                            },
                            Prefix = "hangfire",
                            QueuePollInterval = TimeSpan.FromSeconds(10) // Deafult 15
                        })
            );
            GlobalJobFilters.Filters.Add(
                new HangfireJobExpirationTimeAttribute(_hangfireDbConfiguration.JobExpirationDays));
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            // Add the processing server as IHostedService
            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "high", "medium", "low", "default" };
            });

            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                            new IgnoreExtraElementsConvention(true),
                            new IgnoreIfNullConvention(true)
                },
                t => true);            

            var jobServerConfiguration = configuration.GetSection("JobServerConfiguration").Get<JobServerConfiguration>();
            if (jobServerConfiguration.EnableBusHarvest)
            {
                services.AddMassTransit(cfg =>
                {
                    cfg.AddConsumer<ArtportalenConsumer>();
                    cfg.UsingAzureServiceBus((context, cfg) =>
                    {
                        var busConfiguration = configuration.GetSection("BusConfiguration").Get<BusConfiguration>();
                        cfg.Host($"Endpoint={busConfiguration.Host};SharedAccessKeyName={busConfiguration.SharedAccessKeyName};SharedAccessKey={busConfiguration.SharedAccessKey}");
                        cfg.ReceiveEndpoint(busConfiguration.Queue, e =>
                        {
                            e.MaxConcurrentCalls = 1;
                            e.ConfigureConsumer<ArtportalenConsumer>(context);
                        });
                    });
                });
            }

            services.AddDependencyInjectionServices(configuration, _excludeParishGeometries);
   
            builder.Host.UseNLog();
            return builder;
        }

        private static void LogStartupSettings(ILogger<Program> logger)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Hangfire JobServer Started with the following settings:");

            sb.AppendLine("Hangfire settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[MongoDb].[Servers]: {string.Join(", ", _hangfireDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_hangfireDbConfiguration.DatabaseName}");
            sb.AppendLine("");

            sb.AppendLine("Import settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[ArtportalenSettings].[ConnectionStringBackup]: {_importConfiguration.ArtportalenConfiguration.ConnectionStringBackup}");
            sb.AppendLine(
                $"[ArtportalenSettings].[ConnectionStringLive]: {_importConfiguration.ArtportalenConfiguration.ConnectionStringLive}");
            sb.AppendLine(
                $"[ArtportalenSettings].[MaxNumberOfSightingsHarvested]: {_importConfiguration.ArtportalenConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine(
                $"[ArtportalenSettings].[ChunkSize]: {_importConfiguration.ArtportalenConfiguration.ChunkSize}");
            sb.AppendLine(
                $"[ArtportalenSettings].[HarvestStartDate]: {_importConfiguration.ArtportalenConfiguration.HarvestStartDate}");
            sb.AppendLine("");

            sb.AppendLine($"[ClamService].[Address]: {_importConfiguration.ClamServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[ClamService].[MaxNumberOfSightingsHarvested]: {_importConfiguration.ClamServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[ClamService].[MaxReturnedChangesInOnePage]: {_importConfiguration.ClamServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine("");

            sb.AppendLine($"[FishDataServiceConfiguration].[BaseAddress]: {_importConfiguration.FishDataServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[FishDataServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.FishDataServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[FishDataServiceConfiguration].[StartHarvestYear]: {_importConfiguration.FishDataServiceConfiguration.StartHarvestYear}");
            sb.AppendLine("");

            sb.AppendLine($"[KulServiceConfiguration].[BaseAddress]: {_importConfiguration.KulServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[KulServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.KulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[KulServiceConfiguration].[StartHarvestYear]: {_importConfiguration.KulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine("");

            sb.AppendLine($"[MvmServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.MvmServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[MvmServiceConfiguration].[MaxReturnedChangesInOnePage]: {_importConfiguration.MvmServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine("");

            sb.AppendLine($"[NorsServiceConfiguration].[BaseAddress]: {_importConfiguration.NorsServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[NorsServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.NorsServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[NorsServiceConfiguration].[StartHarvestYear]: {_importConfiguration.NorsServiceConfiguration.StartHarvestYear}");
            sb.AppendLine("");

            sb.AppendLine($"[SersServiceConfiguration].[BaseAddress]: {_importConfiguration.SersServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[SersServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.SersServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[SersServiceConfiguration].[StartHarvestYear]: {_importConfiguration.SersServiceConfiguration.StartHarvestYear}");
            sb.AppendLine("");

            sb.AppendLine($"[SharkServiceConfiguration].[BaseAddress]: {_importConfiguration.SharkServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[SharkServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.SharkServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[SharkServiceConfiguration].[MaxReturnedChangesInOnePage]: {_importConfiguration.SharkServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine("");

            sb.AppendLine($"[VirtualHerbariumServiceConfiguration].[BaseAddress]: {_importConfiguration.VirtualHerbariumServiceConfiguration.BaseAddress}");
            sb.AppendLine($"[VirtualHerbariumServiceConfiguration].[MaxNumberOfSightingsHarvested]: {_importConfiguration.VirtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"[VirtualHerbariumServiceConfiguration].[MaxReturnedChangesInOnePage]: {_importConfiguration.VirtualHerbariumServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine("");

            sb.AppendLine($"[TaxonListServiceConfiguration].[BaseAddress]: {_importConfiguration.TaxonListServiceConfiguration.BaseAddress}");
            sb.AppendLine("");

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
                $"[ProcessedDb].[Servers]: {string.Join(", ", _processDbConfiguration.Hosts.Select(x => x.Name))}");
            sb.AppendLine($"[ProcessedDb].[DatabaseName]: {_processDbConfiguration.DatabaseName}");
            sb.AppendLine($"[ProcessedDb].[ReadBatchSize]: {_processDbConfiguration.ReadBatchSize}");
            sb.AppendLine($"[ProcessedDb].[WriteBatchSize]: {_processDbConfiguration.WriteBatchSize}");
            sb.AppendLine("");
            sb.AppendLine($"[ProcessedDb].[NoOfThreads]: {_processConfiguration.NoOfThreads}");
            sb.AppendLine($"[ProcessedDb].[Diffusion]: {_processConfiguration.Diffusion}");
            sb.AppendLine($"[ProcessedDb].[Export_Container]: {_processConfiguration.Export_Container}");
            sb.AppendLine($"[ProcessedDb].[RunIncrementalAfterFull]: {_processConfiguration.RunIncrementalAfterFull}");
            sb.AppendLine("");

            sb.AppendLine("Export settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[BlobStorage].[ConnectionString]: {_blobStorageConfiguration.ConnectionString}");
            sb.AppendLine(
                $"[MongoDb].[Servers]: {string.Join(", ", _searchDbConfiguration.Clusters.Select(c => c.Hosts.Select(h => h)))}");
            sb.AppendLine($"[MongoDb].[DatabaseName]: {_processDbConfiguration.DatabaseName}");
            sb.AppendLine($"[FileDestination].[Path]: {_exportConfiguration.FileDestination.Path}");
            sb.AppendLine($"[DwcaFilesCreation].[FolderPath]: {_exportConfiguration.DwcaFilesCreationConfiguration.FolderPath}");
            sb.AppendLine($"[DwcaFilesCreation].[IsEnabled]: {_exportConfiguration.DwcaFilesCreationConfiguration.IsEnabled}");
            sb.AppendLine($"[ZendToConfiguration].[EmailSubject]: {_exportConfiguration.ZendToConfiguration.EmailSubject}");
            sb.AppendLine($"[ZendToConfiguration].[SenderName]: {_exportConfiguration.ZendToConfiguration.SenderName}");
            sb.AppendLine($"[ZendToConfiguration].[SenderOrganization]: {_exportConfiguration.ZendToConfiguration.SenderOrganization}");

            sb.AppendLine("");

            sb.AppendLine("DOI settings:");
            sb.AppendLine("================");
            sb.AppendLine(
                $"[DataCite].[BaseAddress]: {_dataCiteServiceConfiguration.BaseAddress}");
            sb.AppendLine(
                $"[DataCite].[DoiPrefix]: {_dataCiteServiceConfiguration.DoiPrefix}");


            logger.LogInformation(sb.ToString());
        }

        private static bool GetEnvironmentBool(string environmentVariable, bool defaultValue = false)
        {
            string value = Environment.GetEnvironmentVariable(environmentVariable);

            if (value != null)
            {
                if (bool.TryParse(value, out var boolValue))
                {
                    return boolValue;
                }

                return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}