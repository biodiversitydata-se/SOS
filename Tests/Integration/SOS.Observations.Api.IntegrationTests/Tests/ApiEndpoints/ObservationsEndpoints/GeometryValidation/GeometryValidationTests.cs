using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.GeometryValidation;

[Collection(TestCollection.Name)]
public class GeometryValidationTests : TestBase
{
    private const string DegeneratePolygonJson = """
        {
          "geographics": {
            "geometries": [
              {
                "coordinates": [
                  [
                    [16.98472499942703, 62.90148608256228],
                    [16.98472499942703, 62.90148608256228]
                  ]
                ],
                "type": "polygon"
              }
            ]
          }
        }
        """;

    public GeometryValidationTests(TestFixture testFixture, ITestOutputHelper output)
        : base(testFixture, output)
    {
    }

    [Theory]
    [InlineData("/observations/geogridaggregation")]
    [InlineData("/observations/metricgridaggregation")]
    [InlineData("/observations/taxonaggregation")]
    [InlineData("/observations/internal/geogridaggregation")]
    [InlineData("/observations/internal/geogridaggregationgeojson")]
    [InlineData("/observations/internal/geogridtaxaaggregation")]
    [InlineData("/observations/internal/metricgridaggregation")]
    [InlineData("/observations/internal/taxonaggregation")]
    [InlineData("/observations/internal/observedtaxa")]
    [InlineData("/observations/internal/taxonareaaggregation")]
    public async Task Endpoint_WithDegeneratePolygon_ReturnsBadRequest(string endpoint)
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        using var content = new StringContent(DegeneratePolygonJson, Encoding.UTF8, "application/json");

        // Act
        var response = await apiClient.PostAsync(endpoint, content, TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseBody.Should().Contain("Invalid geometry");
    }
}
