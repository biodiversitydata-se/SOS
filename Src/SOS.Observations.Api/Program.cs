using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SOS.Lib.Configuration.Shared;
using SOS.Observations.Api.IoC.Modules;

namespace SOS.Observations.Api
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        private static MongoDbConfiguration _mongoDbConfiguration;

        /// <summary>
        /// Main 
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
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// Create a host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseServiceProviderFactory(hostContext =>
                {
                    _mongoDbConfiguration = hostContext.Configuration.GetSection("ProcessedDbConfiguration").Get<MongoDbConfiguration>();

                    return new AutofacServiceProviderFactory(builder =>
                        builder.RegisterModule(new SearchModule { MongoDbConfiguration = _mongoDbConfiguration })
                    );
                }
                )
                .UseNLog();
    }
}
