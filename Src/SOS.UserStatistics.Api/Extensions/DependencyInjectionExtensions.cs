using Autofac.Core;
using Microsoft.Extensions.Configuration;
using SOS.Lib.Cache;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Resource;

namespace SOS.UserStatistics.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder SetupDependencies(this WebApplicationBuilder webApplicationBuilder)
    {
        var elasticConfiguration = webApplicationBuilder.Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        webApplicationBuilder.Services.AddSingleton(elasticConfiguration);
        webApplicationBuilder.Services.AddSingleton<IElasticClientManager, ElasticClientManager>(elasticClientManager => new ElasticClientManager(elasticConfiguration));

        var processedDbConfiguration = webApplicationBuilder.Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        webApplicationBuilder.Services.AddSingleton<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));
        webApplicationBuilder.Services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();

        // Managers
        webApplicationBuilder.Services.AddScoped<IUserStatisticsManager, UserStatisticsManager>();

        // Repositories
        webApplicationBuilder.Services.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        webApplicationBuilder.Services.AddScoped<IUserObservationRepository, UserStatisticsObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IProcessedObservationRepository, UserStatisticsProcessedObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IUserStatisticsObservationRepository, UserStatisticsObservationRepository>();
        webApplicationBuilder.Services.AddScoped<IUserStatisticsProcessedObservationRepository, UserStatisticsProcessedObservationRepository>();
        return webApplicationBuilder;
    }
}
