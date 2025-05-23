using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using SOS.Analysis.Api.ApplicationInsights;
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
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Validators.Interfaces;
using SOS.Shared.Api.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Analysis.Api.Repositories;
using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch.Cluster;

namespace SOS.Analysis.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        // Add application insights.
        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = Settings.ApplicationInsights.ConnectionString;
        });        
        services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
        services.AddSingleton(Settings.ApplicationInsights!);
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

        // Elasticsearch configuration
        var elasticConfiguration = Settings.SearchDbConfiguration;
        services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));
        services.AddSingleton(elasticConfiguration!);

        // Processed Mongo Db
        var processedDbConfiguration = Settings.ProcessDbConfiguration;
        if (processedDbConfiguration == null)
        {
            throw new Exception("Failed to get ProcessDbConfiguration");
        }
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
            processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

        // Add configuration
        services.AddSingleton(Settings.AnalysisConfiguration!);
        services.AddSingleton(Settings.InputValaidationConfiguration!);
        services.AddSingleton(Settings.UserServiceConfiguration!);
        services.AddSingleton(Settings.AreaConfiguration!);
        services.AddSingleton(Settings.CryptoConfiguration!);

        // Add security
        services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

        // Add Caches
        services.AddSingleton<IAreaCache, AreaCache>();
        services.AddSingleton<IDataProviderCache, DataProviderCache>();
        services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
        var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
        services.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);
        services.AddSingleton<SortableFieldsCache>();

        // Add managers            
        services.AddScoped<IAnalysisManager, AnalysisManager>();
        services.AddScoped<IFilterManager, FilterManager>();
        services.AddSingleton<ITaxonManager, TaxonManager>();

        // Add repositories
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IDataProviderRepository, DataProviderRepository>();
        services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
        services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
        services.AddScoped<ITaxonRepository, TaxonRepository>();
        services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        services.AddScoped<IUserExportRepository, UserExportRepository>();

        // Add services
        services.AddSingleton<IHttpClientService, HttpClientService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IFileService, FileService>();

        // Add Utilites
        services.AddSingleton<ISearchFilterUtility, SearchFilterUtility>();

        // Add Validators
        services.AddScoped<IInputValidator, InputValidator>();     

        return services;
    }  
}