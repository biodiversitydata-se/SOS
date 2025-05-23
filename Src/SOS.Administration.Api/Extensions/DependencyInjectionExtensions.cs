using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SOS.Administration.Api.Managers;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

namespace SOS.Administration.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services)
    {        
        services.AddSingleton(Settings.SosApiConfiguration);        
        services.AddSingleton(Settings.ImportConfiguration.GeoRegionApiConfiguration);
        services.AddScoped<DiagnosticsManager>();
        services.AddScoped<ICacheManager, CacheManager>();
        services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();
        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();        
        services.AddSingleton(new ApiManagementServiceConfiguration());
        services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();
        services.AddScoped<ITaxonRepository, TaxonRepository>();

        return services;
    }
}