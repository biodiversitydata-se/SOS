﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SOS.ContainerIntegrationTests.Stubs;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Represents a custom WebApplicationFactory used for testing the Observations API.
/// </summary>
/// <remarks>
/// This class allows you to replace services, such as a conventional database,
/// with a test database that temporarily runs in a container for the duration of the test run.
/// </remarks>
public class ObservationsApiWebApplicationFactory : WebApplicationFactory<Observations.Api.Program>
{
    public ServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Initializes a new instance of the RestApiWebApplicationFactory class.
    /// </summary>
    public ObservationsApiWebApplicationFactory()
    {        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Dev");
    }

    /// <summary>
    /// Configures the WebHostBuilder for the test application.
    /// </summary>
    /// <param name="builder">The WebHostBuilder instance to be configured.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using var scope = ServiceProvider.CreateScope();
        var observationDatasetRepository = scope.ServiceProvider.GetService<IDatasetRepository>();
        var observationEventRepository = scope.ServiceProvider.GetService<IEventRepository>();
        var processedObservationCoreRepository = scope.ServiceProvider.GetService<IProcessedObservationCoreRepository>();
        var processedObservationRepository = scope.ServiceProvider.GetService<IProcessedObservationRepository>();
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
            services.Replace(ServiceDescriptor.Scoped<IProcessedObservationRepository>(x => processedObservationRepository));
            services.Replace(ServiceDescriptor.Singleton<IProcessClient>(x => processClient));
        });
    }
}
