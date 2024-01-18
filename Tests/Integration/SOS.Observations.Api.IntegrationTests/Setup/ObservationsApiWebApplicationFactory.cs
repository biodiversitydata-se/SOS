using NSubstitute;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Shared.Api.Configuration;

namespace SOS.Observations.Api.IntegrationTests.Setup;

/// <summary>
/// Represents a custom WebApplicationFactory used for testing the Observations API.
/// </summary>
/// <remarks>
/// This class allows you to replace services, such as a conventional database,
/// with a test database that temporarily runs in a container for the duration of the test run.
/// </remarks>
public class ObservationsApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Initializes a new instance of the RestApiWebApplicationFactory class.
    /// </summary>
    public ObservationsApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Dev");
        Environment.SetEnvironmentVariable("DISABLE_HANGFIRE_INIT", "true");
        Environment.SetEnvironmentVariable("DISABLE_HEALTHCHECK_INIT", "true");
        Environment.SetEnvironmentVariable("DISABLE_CACHED_TAXON_SUM_INIT", "true");
    }

    /// <summary>
    /// Configures the WebHostBuilder for the test application.
    /// </summary>
    /// <param name="builder">The WebHostBuilder instance to be configured.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using var scope = ServiceProvider!.CreateScope();
        var processClient = scope.ServiceProvider.GetService<IProcessClient>();
        var elasticSearchConfiguration = scope.ServiceProvider.GetService<ElasticSearchConfiguration>();
        var elasticClientManager = scope.ServiceProvider.GetService<IElasticClientManager>();

        builder.ConfigureTestServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton(x => processClient!)); // Replace MongoDB 
            services.Replace(ServiceDescriptor.Singleton(x => elasticClientManager!)); // Replace Elasticsearch
            services.Replace(ServiceDescriptor.Singleton(x => elasticSearchConfiguration!));
            services.Replace(ServiceDescriptor.Singleton(x => _apiConfiguration));
            services.Replace(ServiceDescriptor.Singleton(x => _inputValidationConfiguration));
            services.Replace(ServiceDescriptor.Singleton(x => Substitute.For<IBlobStorageService>()));

            services.Configure<AuthenticationOptions>(options =>
            {
                options.SchemeMap.Clear();
                ((IList<AuthenticationSchemeBuilder>)options.Schemes).Clear();
            });

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
        });
    }

    // Replace api configuration and set ExportPath=Path.GetTempPath() in order to exports to work on Linux.
    private ObservationApiConfiguration _apiConfiguration = new ObservationApiConfiguration()
    {
        ProtectedScope = "SOS.Observations.Protected",
        DefaultUserExportLimit = 5,
        DownloadExportObservationsLimit = 50000,
        OrderExportObservationsLimit = 2000000,
        ExportPath = Path.GetTempPath(),
        EnableResponseCompression = true,
        ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Fastest
    };

    private InputValaidationConfiguration _inputValidationConfiguration = new InputValaidationConfiguration()
    {
        SignalSearchTaxonListIds = new int[] { 1, 7, 8, 17, 18 },
        TilesLimitInternal = 350000,
        TilesLimitPublic = 65535,
        CountFactor = 1.1
    };
}