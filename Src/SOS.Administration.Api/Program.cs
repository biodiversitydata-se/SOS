using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using SOS.Administration.Api.IoC;
using SOS.Harvest.IoC.Modules;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using System;
using System.IO;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SOS.Administration.Api
{
    /// <summary>
    /// </summary>
    public class Program
    {
        private static MongoDbConfiguration _verbatimDbConfiguration;
        private static ImportConfiguration _importConfiguration;
        private static ProcessConfiguration _processConfiguration;
        private static MongoDbConfiguration _processDbConfiguration;
        private static ApplicationInsightsConfiguration _applicationInsightsConfiguration;
        private static SosApiConfiguration _sosApiConfiguration;
        private static UserServiceConfiguration _userServiceConfiguration;        

        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings(environment: env).GetCurrentClassLogger();

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
                NLog.LogManager.Shutdown();
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
                        .UseStartup<Startup>().UseUrls("http://*:5005");
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    NLog.LogManager.ReconfigExistingLoggers();
                })
                .UseServiceProviderFactory(hostContext =>
                {
                    var environment = hostContext.HostingEnvironment.EnvironmentName.ToLower();
                    bool isDevelopment = environment.Equals("local") || environment.Equals("dev") || environment.Equals("st");
                    if (isDevelopment)
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
                    _verbatimDbConfiguration = Settings.VerbatimDbConfiguration;
                    _processDbConfiguration = Settings.ProcessDbConfiguration;
                    _importConfiguration = Settings.ImportConfiguration;
                    _processConfiguration = Settings.ProcessConfiguration;
                    _applicationInsightsConfiguration = Settings.ApplicationInsightsConfiguration;
                    _sosApiConfiguration = Settings.SosApiConfiguration;
                    _userServiceConfiguration = Settings.UserServiceConfiguration;

                    return new AutofacServiceProviderFactory(builder =>
                        builder
                            .RegisterModule(new HarvestModule { Configurations = (_importConfiguration, null, _verbatimDbConfiguration, _processConfiguration, _processDbConfiguration, _applicationInsightsConfiguration, _sosApiConfiguration, _userServiceConfiguration, null) })
                            .RegisterModule<AdministrationModule>()
                    );
                }
                )
                .UseNLog();
        }
    }
}