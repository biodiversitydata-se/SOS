using Microsoft.ApplicationInsights.Extensibility;
using SOS.DataStewardship.Api.ApplicationInsights;
using SOS.DataStewardship.Api.Managers;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Services;

namespace SOS.DataStewardship.Api.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static WebApplicationBuilder SetupDependencies(this WebApplicationBuilder webApplicationBuilder)
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
        webApplicationBuilder.Services.AddSingleton<IDataProviderCache, DataProviderCache>();        

        // Security
        webApplicationBuilder.Services.AddSingleton<IAuthorizationProvider, CurrentUserAuthorization>();

        // Managers
        webApplicationBuilder.Services.AddScoped<ITaxonManager, TaxonManager>();
        webApplicationBuilder.Services.AddScoped<IDataStewardshipManager, DataStewardshipManager>();
        webApplicationBuilder.Services.AddScoped<IFilterManager, FilterManager>();

        // Repositories
        webApplicationBuilder.Services.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonRepository, TaxonRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        webApplicationBuilder.Services.AddScoped<IObservationDatasetRepository, ObservationDatasetRepository>();
        webApplicationBuilder.Services.AddScoped<IObservationEventRepository, ObservationEventRepository>();
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
        webApplicationBuilder.Services.AddSingleton(webApplicationBuilder.Configuration.GetSection("ApplicationInsights").Get<Lib.Configuration.Shared.ApplicationInsights>());
        webApplicationBuilder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        webApplicationBuilder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

        return webApplicationBuilder;
    }
}
