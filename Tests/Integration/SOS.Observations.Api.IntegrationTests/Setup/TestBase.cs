using MongoDB.Bson.Serialization.Conventions;
using System.Globalization;

namespace SOS.Observations.Api.IntegrationTests.Setup;

/// <summary>
/// Provides fixture access, JSON serialization and common test settings.
/// Tests that write Elasticsearch data use <see cref="ElasticsearchTestBase"/>.
/// </summary>
public class TestBase
{
    protected TestFixture TestFixture { get; }
    protected ITestOutputHelper Output { get; }
    protected IProcessFixture ProcessFixture => TestFixture.ProcessFixture;

    protected readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    public TestBase(TestFixture testFixture, ITestOutputHelper output)
    {
        TestFixture = testFixture;
        Output = output;

        var culture = new CultureInfo("sv-SE");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        ConventionRegistry.Register(
            "MongoDB Solution Conventions",
            new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
            },
            t => true);
    }
}
