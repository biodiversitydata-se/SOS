namespace SOS.UserStatistics.Extensions;

internal static class DependencyInjectionExtensions
{
    internal static WebApplicationBuilder SetupDependencies(this WebApplicationBuilder webApplicationBuilder)
    {
        var elasticConfiguration = webApplicationBuilder.Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        webApplicationBuilder.Services.AddSingleton(elasticConfiguration);
        webApplicationBuilder.Services.AddSingleton<IElasticClientManager, ElasticClientManager>(elasticClientManager => new ElasticClientManager(elasticConfiguration));

        var processedDbConfiguration = webApplicationBuilder.Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        webApplicationBuilder.Services.AddSingleton<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));
        webApplicationBuilder.Services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();

        // Add security
        webApplicationBuilder.Services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

        // Managers
        webApplicationBuilder.Services.AddScoped<ITaxonManager, TaxonManager>();
        webApplicationBuilder.Services.AddScoped<IUserStatisticsManager, UserStatisticsManager>();

        // Repositories
        webApplicationBuilder.Services.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonRepository, TaxonRepository>();
        webApplicationBuilder.Services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        webApplicationBuilder.Services.AddScoped<IUserObservationRepository, UserStatisticsObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IProcessedObservationRepository, UserStatisticsProcessedObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IUserStatisticsObservationRepository, UserStatisticsObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IUserStatisticsProcessedObservationRepository, UserStatisticsProcessedObservationRepository>();
        return webApplicationBuilder;
    }
}
