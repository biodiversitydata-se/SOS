using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
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

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToUpper();

            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
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
                            .UseMongoStorage($"mongodb:// {(string.IsNullOrEmpty(mongoConfiguration.UserName) || string.IsNullOrEmpty(mongoConfiguration.Password) ? "" : $"{mongoConfiguration.UserName}:{mongoConfiguration.Password}@")} {string.Join(",", mongoConfiguration.Hosts.Select(h => $"{h.Name}:{h.Port}"))}?connect=replicaSet",
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
                        var importConfiguration = hostContext.Configuration.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
                        var processConfiguration = hostContext.Configuration.GetSection(typeof(ProcessConfiguration).Name).Get<ProcessConfiguration>();
                        var exportConfiguration  = hostContext.Configuration.GetSection(typeof(ExportConfiguration).Name).Get<ExportConfiguration>();

                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule<CoreModule>()
                                .RegisterModule(new ImportModule { Configuration = importConfiguration })
                                .RegisterModule(new ProcessModule { Configuration = processConfiguration })
                                .RegisterModule(new ExportModule { Configuration = exportConfiguration })
                        );
                    }
                )
                .UseNLog();
    }
}
