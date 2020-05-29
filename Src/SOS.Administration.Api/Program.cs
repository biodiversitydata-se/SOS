using System;
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
using SOS.Process.IoC.Modules;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SOS.Administration.Api
{
    /// <summary>
    /// </summary>
    public class Program
    {
        private static ImportConfiguration _importConfiguration;
        private static ProcessConfiguration _processConfiguration;

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
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseServiceProviderFactory(hostContext =>
                    {
                        _importConfiguration = hostContext.Configuration.GetSection(typeof(ImportConfiguration).Name)
                            .Get<ImportConfiguration>();
                        _processConfiguration = hostContext.Configuration.GetSection(typeof(ProcessConfiguration).Name)
                            .Get<ProcessConfiguration>();

                        return new AutofacServiceProviderFactory(builder =>
                            builder
                                .RegisterModule(new ImportModule {Configuration = _importConfiguration})
                                .RegisterModule(new ProcessModule {Configuration = _processConfiguration})
                        );
                    }
                )
                .UseNLog();
        }
    }
}