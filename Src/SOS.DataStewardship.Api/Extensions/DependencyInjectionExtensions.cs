﻿using Microsoft.ApplicationInsights.Extensibility;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Services;
using SOS.DataStewardship.Api.Application.Infrastructure.ApplicationInsights;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Application.Managers;
using Autofac.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace SOS.DataStewardship.Api.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static MongoDbConfiguration SetupDependencies(this WebApplicationBuilder webApplicationBuilder)
    {
        // Configuration
        var elasticConfiguration = webApplicationBuilder.Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        webApplicationBuilder.Services.AddSingleton(elasticConfiguration);
        webApplicationBuilder.Services.AddSingleton<IElasticClientManager, ElasticClientManager>(elasticClientManager => new ElasticClientManager(elasticConfiguration));

        var processedDbConfiguration = webApplicationBuilder.Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        webApplicationBuilder.Services.AddSingleton<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));        
        
        webApplicationBuilder.Services.AddSingleton(webApplicationBuilder.Configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>());

        // Cache
        webApplicationBuilder.Services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
        webApplicationBuilder.Services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        webApplicationBuilder.Services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        webApplicationBuilder.Services.AddSingleton<IAreaCache, AreaCache>();
        webApplicationBuilder.Services.AddSingleton(new AreaConfiguration());
        webApplicationBuilder.Services.AddSingleton<IDataProviderCache, DataProviderCache>();
        var clusterHealthCache = new ClassCache<Dictionary<string, ClusterHealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, ClusterHealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
        webApplicationBuilder.Services.AddSingleton<IClassCache<Dictionary<string, ClusterHealthResponse>>>(clusterHealthCache);

        // Security
        webApplicationBuilder.Services.AddSingleton<IAuthorizationProvider, CurrentUserAuthorization>();

        // Managers
        webApplicationBuilder.Services.AddSingleton<ITaxonManager, TaxonManager>();
        webApplicationBuilder.Services.AddScoped<IDataStewardshipManager, DataStewardshipManager>();
        webApplicationBuilder.Services.AddScoped<IFilterManager, FilterManager>();

        // Repositories
        webApplicationBuilder.Services.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonRepository, TaxonRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        webApplicationBuilder.Services.AddScoped<IDatasetRepository, DatasetRepository>();
        webApplicationBuilder.Services.AddScoped<IEventRepository, EventRepository>();
        webApplicationBuilder.Services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
        webApplicationBuilder.Services.AddSingleton<IAreaRepository, AreaRepository>();
        webApplicationBuilder.Services.AddSingleton<IDataProviderRepository, DataProviderRepository>();

        // Services
        webApplicationBuilder.Services.AddSingleton<IUserService, UserService>();
        webApplicationBuilder.Services.AddSingleton<IHttpClientService, HttpClientService>();

        // Add application insights.
        webApplicationBuilder.Services.AddApplicationInsightsTelemetry(webApplicationBuilder.Configuration);
        // Application insights custom
        webApplicationBuilder.Services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
        webApplicationBuilder.Services.AddSingleton(webApplicationBuilder.Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>());
        webApplicationBuilder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        webApplicationBuilder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

        return processedDbConfiguration;
    }
}
