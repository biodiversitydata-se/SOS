using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch.Cluster;
using SOS.ElasticSearch.Proxy.ApplicationInsights;

namespace SOS.ElasticSearch.Proxy.Extensions;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependencyInjectionServices(IConfigurationRoot configuration)
        {
            // Add application insights.
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = Settings.ApplicationInsightsConfiguration.ConnectionString;
            });
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            services.AddSingleton(Settings.ApplicationInsightsConfiguration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            //setup the elastic search configuration
            var elasticConfiguration = Settings.SearchDbConfiguration;
            if (elasticConfiguration == null)
            {
                throw new Exception("Failed to load Elastic Configuration");
            }
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));

            // Processed Mongo Db
            var processedDbConfiguration = Settings.ProcessDbConfiguration;
            if (processedDbConfiguration == null)
            {
                throw new Exception("Failed to load Process Db Configuration");
            }
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            var proxyConfiguration = Settings.ProxyConfiguration;
            if (proxyConfiguration == null)
            {
                throw new Exception("Failed to load Proxy Configuration");
            }
            services.AddSingleton(proxyConfiguration);

            // Add Caches
            services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            services.AddSingleton<IDataProviderCache, DataProviderCache>();
            var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
            services.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);

            // Add repositories
            services.AddScoped<IDataProviderRepository, DataProviderRepository>();
            services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
            services.AddScoped<ITaxonRepository, TaxonRepository>();
            services.AddScoped<ITaxonListRepository, TaxonListRepository>();

            // Add managers
            services.AddSingleton<ITaxonManager, TaxonManager>();

            // Add caches
            services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();

            return services;
        }
    }
}