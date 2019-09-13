using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using DataPopulateService.Repositories.DocumentDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using MongoDB.Driver;
using SOS.Batch.Import.AP.Configuration;
using SOS.Batch.Import.AP.Factories;
using SOS.Batch.Import.AP.Factories.Interfaces;
using SOS.Batch.Import.AP.Repositories.Destination.Interfaces;
using SOS.Batch.Import.AP.Repositories.Source;
using SOS.Batch.Import.AP.Repositories.Source.Interfaces;
using SOS.Batch.Import.AP.Services;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);

            try
            {
                var environment = app.Option(
                   "-e|--environment <environment>",
                   "The environment to update. Possible values are dev, prod or st",
                   CommandOptionType.SingleValue);

                app.Execute(args);

                if (environment.HasValue() && new[] { "dev", "st", "prod" }.Contains(environment.Value()))
                {
                    var host = new HostBuilder()
                        .UseEnvironment(environment.Value())
                        .UseNLog(new NLogAspNetCoreOptions
                        {
                            CaptureMessageProperties = true,
                            CaptureMessageTemplates = true
                        })
                        .ConfigureAppConfiguration((hostContext, configApp) => {
                            configApp.SetBasePath(Directory.GetCurrentDirectory());
                            configApp.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                            configApp.AddJsonFile($"appsettings.{environment.Value()}.json", optional: false, reloadOnChange: true);
                            configApp.AddCommandLine(args);
                        })
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            NLog.LogManager.LoadConfiguration($"NLog.{environment.Value()}.config");
                        })
                        .ConfigureServices((hostContext, services) => {
                            // Mongo Db
                            var mongoConfigSection = hostContext.Configuration.GetSection("MongoDbConfiguration");

                            // Add configuration
                            services.Configure<MongoDbConfiguration>(mongoConfigSection);

                            services.AddSingleton<IHostedService, AggregationService>();

                            var mongoConfiguration = mongoConfigSection.Get<MongoDbConfiguration>();
                            var settings = new MongoClientSettings
                            {
                                Servers = mongoConfiguration.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                                ReplicaSetName = mongoConfiguration.ReplicaSetName,
                                UseSsl = mongoConfiguration.UseSsl,
                                SslSettings = mongoConfiguration.UseSsl ? new SslSettings
                                {
                                    EnabledSslProtocols = SslProtocols.Tls12
                                } : null
                            };

                            if (!string.IsNullOrEmpty(mongoConfiguration.DatabaseName) &&
                                !string.IsNullOrEmpty(mongoConfiguration.Password))
                            {
                                var identity = new MongoInternalIdentity(mongoConfiguration.DatabaseName, mongoConfiguration.UserName);
                                var evidence = new PasswordEvidence(mongoConfiguration.Password);

                                settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);
                            }

                            // Setup Mongo Db
                            services.AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(settings));
                  
                            // Add options
                            services.AddOptions();

                            // Add logging
                            services.AddSingleton<ILoggerFactory, LoggerFactory>();
                            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                            services.AddLogging(logger => logger.SetMinimumLevel(LogLevel.Trace));

                            // Repositories source
                            services.AddScoped<IMetadataRepository, MetadataRepository>();
                            services.AddScoped<IProjectRepository, ProjectRepository>();
                            services.AddScoped<ISightingRepository, SightingRepository>();
                            services.AddScoped<ISiteRepository, SiteRepository>();
                            services.AddScoped<ITaxonRepository, TaxonRepository>();

                            // Repositories destination
                            services.AddScoped<ISightingAggregateRepository, SightingAggregateRepository>();

                            // Add factories
                            services.AddScoped<ISightingFactory, SightingFactory>();

                            // Add Services
                            services.AddScoped<ISpeciesPortalDataService, SpeciesPortalDataService>();
                        })
                        .Build();

                    await host.RunAsync();
                }
                else
                {
                    app.ShowHelp();
                }
            }
            catch (Exception e)
            {
                app.ShowHelp();
            }
            
        }
    }
}
