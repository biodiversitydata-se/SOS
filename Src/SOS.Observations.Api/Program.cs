using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SOS.Observations.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Program
    {
        private static string _env;
        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var logger = NLogBuilder.ConfigureNLog($"NLog.{_env}.config").GetCurrentClassLogger();

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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
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
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    LogManager.ReconfigExistingLoggers();
                })
                .UseNLog();
        }
    }
}