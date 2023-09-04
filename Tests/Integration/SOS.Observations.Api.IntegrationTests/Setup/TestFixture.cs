using Microsoft.ApplicationInsights.Extensibility;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;
using SOS.Observations.Api.IntegrationTests.Setup.LiveDbFixtures;

namespace SOS.Observations.Api.IntegrationTests.Setup;

/// <summary>
/// Represents a fixture for integration tests, implementing the <see cref="IAsyncLifetime"/> interface.
/// This fixture provides the necessary setup and teardown logic for integration tests using the Observations API.
/// </summary>
public class TestFixture : IAsyncLifetime
{
    private ProcessFixtureDbMode _processFixtureDbMode = ProcessFixtureDbMode.ContainerDb;

    /// <summary>
    /// The <see cref="ObservationsApiWebApplicationFactory"/> used to create the API client.
    /// </summary>
    public ObservationsApiWebApplicationFactory ApiFactory { get; set; }

    public TestContainersFixture TestContainerFixture { get; private set; }
    public ServiceProvider? ServiceProvider { get; private set; }
    public HarvestFixture? HarvestFixture { get; private set; }
    public IProcessFixture? ProcessFixture { get; private set; }

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

    public async Task InitializeAsync()
    {
        ServiceCollection[] services = null!;
        if (_processFixtureDbMode == ProcessFixtureDbMode.ContainerDb)
        {
            await TestContainerFixture.InitializeAsync();
            services = GetContainerModeServices();
        }
        else if (_processFixtureDbMode == ProcessFixtureDbMode.LiveDb)
        {
            services = GetLiveDbModeServices();
        }

        ServiceProvider = ServiceProviderExtensions.RegisterServices(services);
        ApiFactory.ServiceProvider = ServiceProvider;
        using var scope = ServiceProvider.CreateScope();
        ProcessFixture = scope.ServiceProvider.GetService<IProcessFixture>();
        await ProcessFixture!.InitializeElasticsearchIndices();
        HarvestFixture = scope.ServiceProvider.GetService<HarvestFixture>();
    }

    private ServiceCollection[] GetContainerModeServices()
    {
        return new ServiceCollection[]
        {
            ContainerDbFixtures.ProcessFixture.GetServiceCollection(),
            HarvestFixture.GetServiceCollection(),
            TestContainerFixture.GetServiceCollection()
        };
    }

    private ServiceCollection[] GetLiveDbModeServices()
    {
        return new ServiceCollection[]
        {
            LiveDbProcessFixture.GetServiceCollection(),
            HarvestFixture.GetServiceCollection()
        };
    }

    /// <summary>
    /// Performs asynchronous cleanup logic after running the integration tests.    
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await ApiFactory.DisposeAsync();
    }

    internal void ResetTaxonSumAggregationCache()
    {
        IClassCache<Dictionary<int, TaxonSumAggregationItem>>? cache = ApiFactory.Services.GetService<IClassCache<Dictionary<int, TaxonSumAggregationItem>>>()!;
        var c = cache.Get();
        if (c != null)
        {
            cache.Set(null!);
        }
    }

    private enum ProcessFixtureDbMode
    {
        /// <summary>
        /// Run Elasticsearch and MongoDB in container
        /// </summary>
        ContainerDb = 0,

        /// <summary>
        /// Run Elasticsearch and MongoDB in a existing on premise db
        /// </summary>
        LiveDb = 1
    }
}