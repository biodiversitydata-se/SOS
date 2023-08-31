﻿using Microsoft.ApplicationInsights.Extensibility;
using SOS.ContainerIntegrationTests.Extensions;
using SOS.ContainerIntegrationTests.Setup.ContainerDbFixtures;
using SOS.ContainerIntegrationTests.Setup.LiveDbFixtures;

namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Represents a fixture for integration tests, implementing the <see cref="IAsyncLifetime"/> interface.
/// This fixture provides the necessary setup and teardown logic for integration tests using the Observations API.
/// </summary>
public class TestFixture : IAsyncLifetime
{
    /// <summary>
    /// The <see cref="ObservationsApiWebApplicationFactory"/> used to create the API client.
    /// </summary>
    public ObservationsApiWebApplicationFactory ApiFactory { get; set; }

    public TestContainersFixture TestContainerFixture { get; private set; }
    public ServiceProvider? ServiceProvider { get; private set; }
    public ProcessFixture? ProcessFixture { get; private set; }
    public HarvestFixture? HarvestFixture { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFixture"/> class.    
    /// </summary>
    public TestFixture()
    {
        ApiFactory = new ObservationsApiWebApplicationFactory();
        TestContainerFixture = new TestContainersFixture();
        TelemetryConfiguration.Active.DisableTelemetry = true;
    }

    /// <summary>
    /// Creates and returns a new <see cref="HttpClient"/> instance that is ready to interact with the REST API.
    /// </summary>
    /// <returns>A new <see cref="HttpClient"/> instance.</returns>
    public HttpClient CreateApiClient()
    {                
        return ApiFactory.CreateClient();        
    }

    public HttpClient CreateApiClientWithReplacedService<TService>(TService service) where TService : class
    {
        var apiClient = ApiFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton(x => service));
            });
        }).CreateClient();
        return apiClient;
    }

    /// <summary>
    /// Performs asynchronous setup logic before running the integration tests.
    /// For example, this method could be used to initialize temporary databases running in containers.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {        
        await TestContainerFixture.InitializeAsync();
        ServiceProvider = RegisterServices();
        ApiFactory.ServiceProvider = ServiceProvider;
        using var scope = ServiceProvider.CreateScope();
        ProcessFixture = scope.ServiceProvider.GetService<ProcessFixture>();
        await ProcessFixture!.InitializeElasticsearchIndices();
        HarvestFixture = scope.ServiceProvider.GetService<HarvestFixture>();
    }

    /// <summary>
    /// Performs asynchronous cleanup logic after running the integration tests.    
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await ApiFactory.DisposeAsync();
    }

    private ServiceProvider RegisterServices()
    {
        var processFixtureServices = ProcessFixture.GetServiceCollection();
        var harvestFixtureServices = HarvestFixture.GetServiceCollection();
        var testContainersServices = TestContainerFixture.GetServiceCollection();
        var serviceProvider = ServiceProviderExtensions.RegisterServices(processFixtureServices, harvestFixtureServices, testContainersServices);
        return serviceProvider;
    }
}
