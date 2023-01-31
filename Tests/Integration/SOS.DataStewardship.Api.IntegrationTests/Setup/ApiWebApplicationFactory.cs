using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Reflection;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ServiceProvider ServiceProvider { get; set; }
    
    public ApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using var scope = ServiceProvider.CreateScope();
        var observationDatasetRepository = scope.ServiceProvider.GetService<IObservationDatasetRepository>();
        var observationEventRepository = scope.ServiceProvider.GetService<IObservationEventRepository>();
        var processedObservationCoreRepository = scope.ServiceProvider.GetService<IProcessedObservationCoreRepository>();
        var processClient = scope.ServiceProvider.GetService<IProcessClient>();

        builder.ConfigureTestServices(services =>
        {
            services.Configure<AuthenticationOptions>(options =>
            {
                options.SchemeMap.Clear();
                ((IList<AuthenticationSchemeBuilder>)options.Schemes).Clear();
            });

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
            services.Replace(ServiceDescriptor.Scoped<IObservationDatasetRepository>(x => observationDatasetRepository));
            services.Replace(ServiceDescriptor.Scoped<IObservationEventRepository>(x => observationEventRepository));
            services.Replace(ServiceDescriptor.Scoped<IProcessedObservationCoreRepository>(x => processedObservationCoreRepository));
            services.Replace(ServiceDescriptor.Singleton<IProcessClient>(x => processClient));
        });
    }  
}