﻿using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using SOS.ElasticSearch.Proxy.ApplicationInsights;
using SOS.ElasticSearch.Proxy.Configuration;
using SOS.ElasticSearch.Proxy.Middleware;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SOS.ElasticSearch.Proxy
{
    public class Startup
    {
        private bool _isDevelopment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            _isDevelopment = environment.Equals("local");
            if (_isDevelopment)
            {
                // If Development mode, add secrets stored on developer machine 
                // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                // In production you should store the secret values as environment variables.
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Use Swedish culture info.
            var culture = new CultureInfo("sv-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            // services.AddMemoryCache();

            // Add Mvc Core services
            services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                .AddApiExplorer()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddMvc();

            // Add application insights.
            services.AddApplicationInsightsTelemetry(Configuration);
            // Application insights custom
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            var applicationInsightsConfiguration = Configuration.GetSection("ApplicationInsights").Get<Lib.Configuration.Shared.ApplicationInsights>();
            if (applicationInsightsConfiguration == null)
            {
                throw new Exception("Failed to load Application Insights Configuration");
            }
            services.AddSingleton(applicationInsightsConfiguration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            //setup the elastic search configuration
            var elasticConfiguration = Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            if (elasticConfiguration == null)
            {
                throw new Exception("Failed to load Elastic Configuration");
            }
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            if (processedDbConfiguration == null)
            {
                throw new Exception("Failed to load Process Db Configuration");
            }
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            var proxyConfiguration = Configuration.GetSection("ProxyConfiguration").Get<ProxyConfiguration>();
            if (proxyConfiguration == null)
            {
                throw new Exception("Failed to load Proxy Configuration");
            }
            services.AddSingleton(proxyConfiguration);

            // Add Caches
            services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, ClusterHealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, ClusterHealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
            services.AddSingleton<IClassCache<ConcurrentDictionary<string, ClusterHealthResponse>>>(clusterHealthCache);

            // Add repositories
            services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
            services.AddScoped<ITaxonRepository, TaxonRepository>();
            services.AddScoped<ITaxonListRepository, TaxonListRepository>();

            // Add managers
            services.AddSingleton<ITaxonManager, TaxonManager>();

            // Add caches
            services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(
            IApplicationBuilder app)
        {
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/caches"),
                    builder => builder
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    }
                )
            );

            app.UseMiddleware<RequestMiddleware>();
        }
    }
}
