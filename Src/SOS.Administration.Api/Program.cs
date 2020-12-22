using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using SOS.Import.IoC.Modules;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Process.IoC.Modules;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SOS.Administration.Api
{
    /// <summary>
    /// </summary>
    public class Program
    {
        private static MongoDbConfiguration _verbatimDbConfiguration;
        private static ImportConfiguration _importConfiguration;
        private static MongoDbConfiguration _processDbConfiguration;


        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var logger = NLogBuilder.ConfigureNLog($"NLog.{env}.config").GetCurrentClassLogger();

            logger.Debug("Starting Service");
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        /// <summary>
        ///     Create a host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    LogManager.ReconfigExistingLoggers();
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        if (hostContext.HostingEnvironment.EnvironmentName.Equals("local",
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            // IIS Express don't get values from secret storage.  This workaround fix it
                            foreach (var prop in hostContext.Properties)
                            {
                                if (prop.Value.GetType().Name.Equals("Startup"))
                                {
                                    var startUp = (Startup)prop.Value;
                                    hostContext.Configuration = startUp.Configuration;
                                }
                            }
                        }
                        
                        /* Get values from secrets storage  */
                        _verbatimDbConfiguration = hostContext.Configuration.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
                        _processDbConfiguration = hostContext.Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
                        _importConfiguration = hostContext.Configuration.GetSection(nameof(ImportConfiguration)).Get<ImportConfiguration>();
                        
                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule(new ImportModule { Configurations = (_importConfiguration, _verbatimDbConfiguration, _processDbConfiguration) })
                                .RegisterModule(new ProcessModule { Configurations = (new ProcessConfiguration(), _verbatimDbConfiguration, _processDbConfiguration) })
                        );
                    }
                )
                .UseNLog();
        }
    }
}