using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
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
using SOS.Lib.Configuration.Shared;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories;
using SOS.Search.Service.Repositories.Interfaces;
using SOS.Search.Service.Swagger;

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
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    configuration
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                       // .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .AddNLog(configFileName: $"nlog.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    
                    // Add Mvc Core services
                    services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                        .AddApiExplorer()
                        .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                            });

                    services.Configure<MongoDbConfiguration>(hostContext.Configuration.GetSection("MongoDbConfiguration"));

                    // Add CORS
                    services.AddCors();

                    // Add options
                    services.AddOptions();

                    // Mongo Db
                    var config =
                        hostContext.Configuration.GetSection("MongoDbConfiguration").Get<MongoDbConfiguration>();

                    // Setup db
                    services.AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(config.GetMongoDbSettings()));

                    // Add factories
                    services.AddScoped<ISightingFactory, SightingFactory>();
                    services.AddScoped<IProcessInfoFactory, ProcessInfoFactory>();
                    services.AddSingleton<ITaxonFactory, TaxonFactory>();
                    services.AddSingleton<IFieldMappingFactory, FieldMappingFactory>();

                    // Repositories mongo
                    services.AddScoped<IProcessedSightingRepository, ProcessedSightingRepository>();
                    services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
                    services.AddScoped<IProcessedTaxonRepository, ProcessedTaxonRepository>();
                    services.AddScoped<IProcessedFieldMappingRepository, ProcessedFieldMappingRepository>();

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

                            options.SchemaFilter<SwaggerIgnoreFilter>();
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
