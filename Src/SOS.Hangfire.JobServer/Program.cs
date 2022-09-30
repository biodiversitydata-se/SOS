using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;
using NLog.Web;
using SOS.Export.IoC.Modules;
using SOS.Hangfire.JobServer.Configuration;
using SOS.Harvest.IoC.Modules;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Hangfire.JobServer
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Program
    {
        private static string _env;
        private static ApiManagementServiceConfiguration _apiManagementServiceConfiguration;
        private static HangfireDbConfiguration _hangfireDbConfiguration;
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

            Console.WriteLine("Starting up in environment:"  + _env);
           

            if (new[] { "local", "dev", "st", "prod", "at" }.Contains(_env, StringComparer.CurrentCultureIgnoreCase))
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
                        .AddNLog($"NLog.{_env}.config");

                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();
                    services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
                    services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();

                    _hangfireDbConfiguration = hostContext.Configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();

                    services.AddHangfire(configuration =>
                        configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings(m =>
                            {
                                m.Converters.Add(new NewtonsoftGeoShapeConverter());
                                m.Converters.Add(new StringEnumConverter());
                            })
                            .UseMongoStorage(new MongoClient(_hangfireDbConfiguration.GetMongoDbSettings()),
                                _hangfireDbConfiguration.DatabaseName,
                                new MongoStorageOptions
                                {
                                    MigrationOptions = new MongoMigrationOptions
                                    {
                                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                                        BackupStrategy = new CollectionMongoBackupStrategy()
                                    },
                                    CheckQueuedJobsStrategy = _hangfireDbConfiguration.Hosts.Length == 1 ? CheckQueuedJobsStrategy.TailNotificationsCollection : CheckQueuedJobsStrategy.Watch,
                                    QueuePollInterval = TimeSpan.FromSeconds(1),
                                    Prefix = "hangfire",
                                    CheckConnection = true,
                                    JobExpirationCheckInterval = TimeSpan.FromMinutes(10)
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

                    // Get configuration
                    _apiManagementServiceConfiguration = hostContext.Configuration.GetSection("ApiManagementServiceConfiguration").Get<ApiManagementServiceConfiguration>();
                    _verbatimDbConfiguration = hostContext.Configuration.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
                    _processDbConfiguration = hostContext.Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
                    _searchDbConfiguration = hostContext.Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
                    _importConfiguration = hostContext.Configuration.GetSection(nameof(ImportConfiguration))
                        .Get<ImportConfiguration>();
                    _processConfiguration = hostContext.Configuration.GetSection(nameof(ProcessConfiguration))
                        .Get<ProcessConfiguration>();
                    _exportConfiguration = hostContext.Configuration.GetSection(nameof(ExportConfiguration))
                        .Get<ExportConfiguration>();
                    _blobStorageConfiguration = hostContext.Configuration.GetSection(nameof(BlobStorageConfiguration))
                        .Get<BlobStorageConfiguration>();
                    _dataCiteServiceConfiguration = hostContext.Configuration.GetSection(nameof(DataCiteServiceConfiguration))
                        .Get<DataCiteServiceConfiguration>();
                    _applicationInsightsConfiguration = hostContext.Configuration.GetSection(nameof(ApplicationInsightsConfiguration))
                        .Get<ApplicationInsightsConfiguration>();
                    _sosApiConfiguration = hostContext.Configuration.GetSection(nameof(SosApiConfiguration))
                        .Get<SosApiConfiguration>();
                    _userServiceConfiguration = hostContext.Configuration.GetSection(nameof(UserServiceConfiguration))
                        .Get<UserServiceConfiguration>();

                    services.AddSingleton(_searchDbConfiguration);
                    services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(_searchDbConfiguration));
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule(new HarvestModule { Configurations = (_importConfiguration, _apiManagementServiceConfiguration, _verbatimDbConfiguration, _processConfiguration, _processDbConfiguration, _applicationInsightsConfiguration, _sosApiConfiguration, _userServiceConfiguration) })
                                .RegisterModule(new ExportModule { Configurations = (_exportConfiguration, _processDbConfiguration, _blobStorageConfiguration, _dataCiteServiceConfiguration, _userServiceConfiguration) })
                        );
                    }
                )
                .UseNLog();            
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
    }
}