using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SOS.Administration.Gui.Clients;
using SOS.Lib.Helpers;
using System.Linq;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SOS.Administration.Gui
{
    public class Program
    {
        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool isDevelopment = new[] { "local", "k8s", "dev", "st" }.Contains(env, StringComparer.InvariantCultureIgnoreCase);
            bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env, StringComparer.InvariantCultureIgnoreCase);
            var useLocalObservationApi = Environment.GetEnvironmentVariable("USE_LOCAL_OBSERVATION_API")?.ToLower() == "true";
            // we set up Log.Logger here in order to be able to log if something goes wrong in the startup process
            Log.Logger = SeriLogHelper.CreateLogger(isLocalDevelopment);
            Log.Logger.Debug("Starting Service");

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.AddServiceDefaults();
                SeriLogHelper.ConfigureSerilog(builder);
                var observationApiUrl = NetAspireHelper.GetServiceEndpoint("sos-observations-api", "http");
                string aspnetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(LogLevel.Trace);

                if (isDevelopment)
                {
                    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                }

                Settings.Init(builder.Configuration); // or fail early!
                var startup = new Startup(isDevelopment);
                if (useLocalObservationApi)
                {
                    Settings.ApiTestConfiguration.ApiUrl = observationApiUrl + "/";
                }
                startup.ConfigureServices(builder.Services);
                builder.Services.AddHttpClient<SosObservationsApiClient>(client =>
                {
                    client.BaseAddress = useLocalObservationApi ? new("https+http://sos-observations-api") : new(Settings.ApiTestConfiguration.ApiUrl);
                });

                WebApplication app = builder.Build();
                app.MapDefaultEndpoints();
                startup.Configure(app, builder.Environment);                
                await app.RunAsync(aspnetCoreUrls ?? "http://*:5000");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }
    }
}