using System;
using System.Linq;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NLog.Extensions.Logging;
using SOS.Import.Configuration;
using SOS.Import.Factories;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;

namespace SOS.Import
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Configuration
        /// </summary>
        public static IConfiguration Configuration;

        public static IServiceProvider ServiceProvider;

        private static void Main(params string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            try
            {
                app.Name = "SOS.Import";
                app.Description =
                    "This program is used to import data from different sources and save it to mongo db";

                var environment = app.Option(
                    "-e|--environment <environment>",
                    "The environment to update. Possible values are dev, prod or st",
                    CommandOptionType.SingleValue);

                var sources = app.Option(
                    "-s|--sources <sources>",
                    "Sources to import. Bitflag: 1 - Species Portal",
                    CommandOptionType.SingleValue);

                app.HelpOption("-? | -h | --help");

                app.OnExecute(() =>
                {
                    var env = environment.Value();
                    if (environment.HasValue() && new[] { "dev", "st", "prod" }.Contains(env) 
                        && sources.HasValue() && int.TryParse(sources.Value(), out var srs) && srs > 0)
                    {
                        // Initialize service collection
                        var serviceCollection = new ServiceCollection();
                        
                        // Configure services
                        ConfigureServices(serviceCollection, env);

                        // Initialize service provider
                        ServiceProvider = serviceCollection.BuildServiceProvider();

                        // Initialize nLog
                        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
                        loggerFactory.AddNLog(new NLogProviderOptions()
                        {
                            CaptureMessageTemplates = true,
                            CaptureMessageProperties = true
                        });

                        NLog.LogManager.LoadConfiguration($"NLog.{env}.config");

                        // Get updateservice
                        var importService = ServiceProvider.GetService<IImportService>();

                        Console.WriteLine("Startar import");

                        var result = importService.ImportAsync(srs).Result;

                        Console.WriteLine($"Körningen returnerade: { result }");

                        Environment.ExitCode = result ? 0 : 1;
                    }
                    else
                    {
                        app.ShowHelp();
                    }

                    return 0;
                });
                app.Execute(args);
            }
            catch
            {
                app.ShowHelp();
            }
        }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        private static void ConfigureServices(IServiceCollection services, string environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // Mongo Db
            var mongoConfigSection = Configuration.GetSection("MongoDbConfiguration");

            // Add configuration
            services.Configure<MongoDbConfiguration>(mongoConfigSection);

            var mongoConfiguration = mongoConfigSection.Get<MongoDbConfiguration>();
            var settings = new MongoClientSettings
            {
                Servers = mongoConfiguration.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                ReplicaSetName = mongoConfiguration.ReplicaSetName,
                UseTls = mongoConfiguration.UseTls,
                SslSettings = mongoConfiguration.UseTls ? new SslSettings
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

            services.AddSingleton(provider => Configuration);

            // Add logging factory
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
            services.AddScoped<ISightingVerbatimRepository, SightingVerbatimRepository>();

            // Add factories
            services.AddScoped<ISpeciesPortalSightingFactory, SpeciesPortalSightingFactory>();

            // Add Services
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<ISpeciesPortalDataService, SpeciesPortalDataService>();
        }
    }
}
