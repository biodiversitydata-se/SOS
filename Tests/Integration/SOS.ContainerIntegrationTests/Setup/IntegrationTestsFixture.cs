namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Represents a fixture for integration tests, implementing the <see cref="IAsyncLifetime"/> interface.
/// This fixture provides the necessary setup and teardown logic for integration tests using the Observations API.
/// </summary>
public class IntegrationTestsFixture : IAsyncLifetime
{
    /// <summary>
    /// The <see cref="ObservationsApiWebApplicationFactory"/> used to create the API client.
    /// </summary>
    public ObservationsApiWebApplicationFactory ApiFactory { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestsFixture"/> class.    
    /// </summary>
    public IntegrationTestsFixture()
    {
        ApiFactory = new ObservationsApiWebApplicationFactory();
    }

    /// <summary>
    /// Creates and returns a new <see cref="HttpClient"/> instance that is ready to interact with the REST API.
    /// </summary>
    /// <returns>A new <see cref="HttpClient"/> instance.</returns>
    public HttpClient CreateApiClient()
    {
        return ApiFactory.CreateClient();
    }

    /// <summary>
    /// Performs asynchronous setup logic before running the integration tests.
    /// For example, this method could be used to initialize temporary databases running in containers.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        // Initialize for example temporary databases running in containers.
    }

    /// <summary>
    /// Performs asynchronous cleanup logic after running the integration tests.    
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await ApiFactory.DisposeAsync();
    }
}
