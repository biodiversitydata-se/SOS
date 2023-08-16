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
        // Here is where you can replace services with test-specific implementations.
        // For example, you can replace a conventional database with a test database 
        // that temporarily runs in a container for as long as the test runs.
        builder.ConfigureTestServices(services =>
        {
            //services.Replace(...);
        });
    }
}
