using NSubstitute;
using SOS.ContainerIntegrationTests.Stubs;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;
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
    public ServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Initializes a new instance of the RestApiWebApplicationFactory class.
    /// </summary>
    public ObservationsApiWebApplicationFactory()
    {        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Dev");
        Environment.SetEnvironmentVariable("DISABLE_HANGFIRE_INIT", "true");
        Environment.SetEnvironmentVariable("DISABLE_HEALTHCHECK_INIT", "true");        
    }

    /// <summary>
    /// Configures the WebHostBuilder for the test application.
    /// </summary>
    /// <param name="builder">The WebHostBuilder instance to be configured.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using var scope = ServiceProvider!.CreateScope();
        var observationDatasetRepository = scope.ServiceProvider.GetService<IDatasetRepository>();
        var observationEventRepository = scope.ServiceProvider.GetService<IEventRepository>();
        var processedObservationCoreRepository = scope.ServiceProvider.GetService<IProcessedObservationCoreRepository>();
        var processedObservationRepository = scope.ServiceProvider.GetService<IProcessedObservationRepository>();
        var processedTaxonRepository = scope.ServiceProvider.GetService<IProcessedTaxonRepository>();
        var processedChecklistRepository = scope.ServiceProvider.GetService<IProcessedChecklistRepository>();
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
            services.Replace(ServiceDescriptor.Scoped(x => observationDatasetRepository!));
            services.Replace(ServiceDescriptor.Scoped(x => observationEventRepository!));
            services.Replace(ServiceDescriptor.Scoped(x => processedObservationCoreRepository!));
            services.Replace(ServiceDescriptor.Scoped(x => processedObservationRepository!));
            services.Replace(ServiceDescriptor.Scoped(x => processedTaxonRepository!));
            services.Replace(ServiceDescriptor.Scoped(x => processedChecklistRepository!));
            services.Replace(ServiceDescriptor.Singleton(x => processClient!));
            services.Replace(ServiceDescriptor.Singleton(x => _apiConfiguration));            
            services.Replace(ServiceDescriptor.Singleton(x => Substitute.For<IBlobStorageService>()));
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
        SignalSearchTaxonListIds = new int[] { 1, 7, 8, 17, 18 },
        EnableResponseCompression = true,
        ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Fastest,
        TilesLimitInternal = 350000,
        TilesLimitPublic = 65535,
        CountFactor = 1.1
    };
}