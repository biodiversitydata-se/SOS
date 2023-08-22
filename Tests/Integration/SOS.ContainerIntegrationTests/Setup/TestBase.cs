using SOS.Lib.JsonConverters;

namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Base class for integration tests providing common functionalities and settings.
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets the <see cref="Setup.TestFixture"/> instance used for setting up the integration test environment.
    /// </summary>
    protected TestFixture TestFixture { get; private set; }

    /// <summary>
    /// Gets the <see cref="ITestOutputHelper"/> instance used for test logging and output in the test runner.
    /// </summary>
    protected ITestOutputHelper Output { get; private set; }

    protected ProcessFixture ProcessFixture => TestFixture.ProcessFixture!;

    /// <summary>
    /// Standard JSON serializer options used in the integration tests for serialization/deserialization operations.
    /// </summary>
    protected readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        Converters = 
        { 
            new JsonStringEnumConverter(),
            new GeoShapeConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="testFixture">The <see cref="Setup.TestFixture"/> instance for setting up the integration test environment.</param>
    /// <param name="output">The <see cref="ITestOutputHelper"/> instance for test logging and output in the test runner.</param>
    public TestBase(TestFixture testFixture, ITestOutputHelper output)
    {
        TestFixture = testFixture;
        Output = output;
    }
}