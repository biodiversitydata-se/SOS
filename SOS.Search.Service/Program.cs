using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NLog.Web;
using SOS.Search.Service.Configuration;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }

        /// <summary>
        /// Create a host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseNLog()
                .ConfigureKestrel((hostBuilder, serverOptions) =>
                {
                   /* serverOptions.Limits.MaxConcurrentConnections = 100;
                    serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
                    serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
                    serverOptions.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    serverOptions.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    serverOptions.Listen(IPAddress.Loopback, 5000);
                    serverOptions.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps("testCert.pfx", "testPassword");
                    });
                    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);*/
                })
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .AddNLog(configFileName: $"nlog.{hostingContext.HostingEnvironment.EnvironmentName}.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add Mvc Core services
                    services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                        .AddApiExplorer();

                    services.Configure<MongoDbConfiguration>(hostContext.Configuration.GetSection("MongoDbConfiguration"));

                    // Add CORS
                    services.AddCors();

                    // Add options
                    services.AddOptions();

                    // Mongo Db
                    var mongoConfiguration =
                        hostContext.Configuration.GetSection("MongoDbConfiguration").Get<MongoDbConfiguration>();

                    var settings = new MongoClientSettings
                    {
                        Servers = mongoConfiguration.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                        ReplicaSetName = mongoConfiguration.ReplicaSetName,
                        UseTls = mongoConfiguration.UseTls,
                        SslSettings = mongoConfiguration.UseTls
                            ? new SslSettings
                            {
                                EnabledSslProtocols = SslProtocols.Tls12
                            }
                            : null
                    };

                    if (!string.IsNullOrEmpty(mongoConfiguration.DatabaseName) &&
                        !string.IsNullOrEmpty(mongoConfiguration.Password))
                    {
                        var identity = new MongoInternalIdentity(mongoConfiguration.DatabaseName,
                            mongoConfiguration.UserName);
                        var evidence = new PasswordEvidence(mongoConfiguration.Password);

                        settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);
                    }

                    // Setup db
                    services.AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(settings));

                    // Add factories
                    services.AddScoped<ISightingFactory, SightingFactory>();

                    // Repositories mongo
                    services.AddScoped<IProcessedDarwinCoreRepository, ProcessedDarwinCoreRepository>();

                    // Configure swagger
                    services.AddSwaggerGen(
                        options =>
                        {
                            options.SwaggerDoc("v1",
                                new OpenApiInfo
                                {
                                    Title = "SOS.Search.Service",
                                    Version = "v1",
                                    Description = "Search sightings"

                                });
                            // Set the comments path for the Swagger JSON and UI.
                            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                           
                            options.IncludeXmlComments(xmlPath);
                        });

                })
                .Configure((hostBuilder, app) =>
                {
                    // Use CORS in the application
                    app.UseCors(x => x.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());

                    if (new [] { "dev", "local"}.Contains(hostBuilder.HostingEnvironment.EnvironmentName.ToLower()))
                    {
                        app.UseDeveloperExceptionPage();
                    }
                    else
                    {
                        app.UseHsts();
                    }

                    // app.UseAuthentication();
                    app.UseHttpsRedirection();
                    app.UseMvc();
                    app.UseStaticFiles();

                    // Use Swagger and SwaggerUI
                    // Enable middleware to serve generated Swagger as a JSON endpoint.
                    app.UseSwagger();

                    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                    // specifying the Swagger JSON endpoint.
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOS Search service");
                    });
                });
    }
}
