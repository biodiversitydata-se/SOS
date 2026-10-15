using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.GeometryValidation;

[Collection(TestCollection.Name)]
[Trait("Category", "ReadOnlyIntegrationTest")]
public class GeometryValidationTests(TestFixture fixture)
{
    private const string DegeneratePolygonJson = """
        {
          "Geographics": {
            "Geometries": [
              {
                "Coordinates": [
                  [
                    [16.98472499942703, 62.90148608256228],
                    [16.98472499942703, 62.90148608256228]
                  ]
                ],
                "Type": "polygon"
              }
            ]
          }
        }
        """;

    [Fact]
    public async Task InternalSearch_WithPascalCaseDegeneratePolygon_PreservesBadRequestMessageAndPath()
    {
        using var apiClient = fixture.CreateApiClient();
        using var content = new StringContent(DegeneratePolygonJson, Encoding.UTF8, "application/json");

        using var response = await apiClient.PostAsync("/observations/internal/search", content, TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result!.Errors.Should().ContainKey("$.geographics.geometries[0]");
        result.Errors["$.geographics.geometries[0]"].Should().Contain("Invalid geometry.");
    }

    [Fact]
    public async Task Analysis_WithPascalCaseDegeneratePolygon_PreservesBadRequestMessageAndPath()
    {
        using var apiClient = fixture.CreateAnalysisApiClient();
        using var content = new StringContent(DegeneratePolygonJson, Encoding.UTF8, "application/json");

        using var response = await apiClient.PostAsync("/internal/aggregation_simple?aggregationField=event.startYear", content, TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result!.Errors.Should().ContainKey("$.geographics.geometries[0]");
        result.Errors["$.geographics.geometries[0]"].Should().Contain("Invalid geometry.");
    }
}
