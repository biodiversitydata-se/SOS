using System;
using System.Linq;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NLog.Extensions.Logging;
using SOS.Process.Configuration;
using SOS.Process.Database;
using SOS.Process.Database.Interfaces;
using SOS.Process.Factories;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source;
using SOS.Process.Repositories.Source.Interfaces;
using SOS.Process.Services;
using SOS.Process.Services.Interfaces;

namespace SOS.Process
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
                app.Name = "SOS.Process";
                app.Description =
                    "This program is used to process verbatim data from different sources to Darwin Core";

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
                        var importService = ServiceProvider.GetService<IProcessService>();

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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

            // Add app settings configuration
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Vebatim Mongo Db
            var verbatimDbConfiguration = Configuration.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            var verbatimSettings = GetMongDbSettings(verbatimDbConfiguration);
            services.AddSingleton<IVerbatimClient, VerbatimClient>(x => new VerbatimClient(verbatimSettings, verbatimDbConfiguration.DatabaseName, verbatimDbConfiguration.BatchSize));

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.GetSection("ProcessedDbConfiguration").Get<MongoDbConfiguration>();
            var processedSettings = GetMongDbSettings(processedDbConfiguration);
            services.AddSingleton<IProcessClient, ProcessClient>(x => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName, processedDbConfiguration.BatchSize));

            // Add options
            services.AddOptions();

            services.AddSingleton(provider => Configuration);

            // Add logging factory
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(logger => logger.SetMinimumLevel(LogLevel.Trace));

            // Repositories source
            services.AddScoped<ISpeciesPortalVerbatimRepository, SpeciesPortalVerbatimRepository>();

            // Repositories destination
            services.AddScoped<IProcessedRepository, ProcessedRepository>();

            // Add factories
            services.AddScoped<ISpeciesPortalProcessFactory, SpeciesPortalProcessFactory>();

            // Add Services
            services.AddScoped<IProcessService, ProcessService>();
            services.AddScoped<ITaxonService, TaxonService>();
        }

        /// <summary>
        /// Get mongo db settings object
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static MongoClientSettings GetMongDbSettings(MongoDbConfiguration config)
        {
            MongoInternalIdentity identity = null;
            PasswordEvidence evidence = null;
            if (!(string.IsNullOrEmpty(config.DatabaseName) ||
                string.IsNullOrEmpty(config.UserName) ||
                string.IsNullOrEmpty(config.Password)))
            {
                identity = new MongoInternalIdentity(config.DatabaseName, config.UserName);
                evidence = new PasswordEvidence(config.Password);
            }

            return new MongoClientSettings
            {
                Servers = config.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                ReplicaSetName = config.ReplicaSetName,
                UseTls = config.UseTls,
                SslSettings = config.UseTls ? new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                } : null,
                Credential = identity != null && evidence != null ? new MongoCredential("SCRAM-SHA-1", identity, evidence) : null
            };
        }
    }
}
