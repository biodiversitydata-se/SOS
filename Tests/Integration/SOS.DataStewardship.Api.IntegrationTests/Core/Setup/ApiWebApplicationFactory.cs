using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup;

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
        var observationDatasetRepository = scope.ServiceProvider.GetService<IDatasetRepository>();
        var observationEventRepository = scope.ServiceProvider.GetService<IEventRepository>();
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
            services.Replace(ServiceDescriptor.Scoped<IDatasetRepository>(x => observationDatasetRepository));
            services.Replace(ServiceDescriptor.Scoped<IEventRepository>(x => observationEventRepository));
            services.Replace(ServiceDescriptor.Scoped<IProcessedObservationCoreRepository>(x => processedObservationCoreRepository));
            services.Replace(ServiceDescriptor.Singleton<IProcessClient>(x => processClient));
        });
    }
}