using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using SOS.Administration.Api.IoC;
using SOS.Harvest.IoC.Modules;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

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
            bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);

            // we set up Log.Logger here in order to be able to log if something goes wrong in the startup process
            Log.Logger = isLocalDevelopment ?
                    new LoggerConfiguration() // human readable in the terminal when developing, not all json
                        .MinimumLevel.Debug()
                        .Enrich.FromLogContext()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties}{NewLine}{Exception}")
                    .CreateLogger()
                :
                    new LoggerConfiguration() // compact json when running in the clusters for that sweet sweet structured logging
                        .MinimumLevel.Information()
                        .WriteTo.Console(new RenderedCompactJsonFormatter())
                        .Enrich.FromLogContext()
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                        .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
                    .CreateLogger();
            
            try
            {
                Log.Logger.Debug("Init SOS Admin API");
                var hostBuilder = CreateHostBuilder(args);
                var swedishCulture = new CultureInfo("sv-SE");
                CultureInfo.DefaultThreadCurrentCulture = swedishCulture;
                CultureInfo.DefaultThreadCurrentUICulture = swedishCulture;
                var app = hostBuilder.Build();
                app.Run();
            }
            catch (Exception ex)
            {                
                Log.Logger.Error(ex, "Stopped program because of exception");
                throw;
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
                .UseSerilog(Log.Logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>().UseUrls("http://*:5005");
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
                });                
        }
    }
}