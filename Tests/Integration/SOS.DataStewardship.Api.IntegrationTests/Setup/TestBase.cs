using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class TestBase
{
    protected TestFixture TestFixture { get; private set; }
    protected ITestOutputHelper Output { get; private set; }
    protected ProcessFixture ProcessFixture => TestFixture.ProcessFixture;
    protected HttpClient ApiClient => TestFixture.ApiClient;
    protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = {
            new JsonStringEnumConverter(),
            new GeoShapeConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    public TestBase(TestFixture testFixture, ITestOutputHelper output)
    {
        TestFixture = testFixture;        
        Output = output;
    }
}
