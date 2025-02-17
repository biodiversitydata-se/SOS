using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.LocationsEndpoints;

/// <summary>
/// Integration tests for GetLocationsByIds.
/// </summary>
[Collection(TestCollection.Name)]
public class GetLocationsByIds : TestBase
{
    public GetLocationsByIds(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }   

    [Fact]
    public async Task GetLocationsByIds_ReturnsExpectedLocations()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(m => m.Site.Id = 4060923)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var locationIds = new List<string> { "urn:lsid:artportalen.se:site:4060923" };

        // Act
        var response = await apiClient.PostAsync($"/Locations", JsonContent.Create(locationIds));        
        var result = await response.Content.ReadFromJsonAsync<List<LocationDto>>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Count.Should().Be(1);
    }
}