﻿using Serilog.Formatting.Compact;
using Serilog;
using Serilog.Filters;

namespace SOS.Analysis.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Program
    {
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
                // ConfigureHostBuilder
                Log.Logger.Information("Init SOS Analysis API");
                var hostBuilder = CreateHostBuilder(args);                
                hostBuilder.Build().Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to start SOS Analysis API");
                throw;
            }            
        }        

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(Log.Logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:5000");
                });
    }
}