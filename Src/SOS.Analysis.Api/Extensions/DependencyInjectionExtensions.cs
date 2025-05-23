using Asp.Versioning.ApiExplorer;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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
using Swashbuckle.AspNetCore.SwaggerGen;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Validators.Interfaces;
using SOS.Shared.Api.Validators;
using System.Reflection;
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

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                       $"InternalSosObservations{description.GroupName}",
                       new OpenApiInfo()
                       {
                           Title = $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}",
                           Version = description.ApiVersion.ToString(),
                           Description = "Species Observation System (SOS) - Observations API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                       });
                options.SwaggerDoc(
                    $"PublicSosObservations{description.GroupName}",
                    new OpenApiInfo()
                    {
                        Title = $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}",
                        Version = description.ApiVersion.ToString(),
                        Description = "Species Observation System (SOS) - Observations API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                    });
                options.SwaggerDoc(
                       $"AzureInternalSosObservations{description.GroupName}",
                       new OpenApiInfo()
                       {
                           Title = $"SOS Observations API (Internal - Azure) {description.GroupName.ToUpperInvariant()}",
                           Version = description.ApiVersion.ToString(),
                           Description = "Species Observation System (SOS) - Observations API. Internal - Azure API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                       });
                options.SwaggerDoc(
                    $"AzurePublicSosObservations{description.GroupName}",
                    new OpenApiInfo()
                    {
                        Title = $"SOS Observations API (Public - Azure) {description.GroupName.ToUpperInvariant()}",
                        Version = description.ApiVersion.ToString(),
                        Description = "Species Observation System (SOS) - Observations API. Public - Azure API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                    });
                options.CustomOperationIds(apiDesc =>
                {
                    apiDesc.TryGetMethodInfo(out MethodInfo methodInfo);
                    string controller = apiDesc.ActionDescriptor.RouteValues["controller"];
                    string methodName = methodInfo.Name;
                    return $"{controller}_{methodName}".Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);
                });
            }
        }
    }
}