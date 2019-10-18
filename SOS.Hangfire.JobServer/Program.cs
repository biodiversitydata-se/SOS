﻿using System;
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
using SOS.Import.Configuration;
using SOS.Import.IoC.Modules;
using SOS.Process.Configuration;
using SOS.Process.IoC.Modules;
using MongoDbConfiguration = SOS.Hangfire.JobServer.Configuration.MongoDbConfiguration;

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
            _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var logger = NLogBuilder.ConfigureNLog($"nlog.{_env}.config").GetCurrentClassLogger();

            try
            {
                logger.Debug("Init main");
                await CreateHostBuilder(args)
                    .Build()
                    .RunAsync();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// Create a host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseEnvironment(_env)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{_env}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
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
                                new MongoStorageOptions { MigrationOptions = new MongoMigrationOptions
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

                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule<CoreModule>()
                                .RegisterModule(new ImportModule { Configuration = importConfiguration })
                                .RegisterModule(new ProcessModule { Configuration = processConfiguration })
                        );
                    }
                )
               /* .UseServiceProviderFactory(new AutofacServiceProviderFactory(builder =>
                    builder
                        .RegisterModule<CoreModule>()
                        .RegisterModule(new ImportModule{ Configuration = _importConfiguration })
                        .RegisterModule(new ProcessModule { Configuration = _processConfiguration })
                ))*/
                .UseNLog();
    }
}
