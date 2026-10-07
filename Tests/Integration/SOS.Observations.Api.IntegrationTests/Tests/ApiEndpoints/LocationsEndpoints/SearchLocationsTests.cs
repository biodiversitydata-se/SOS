using FizzWare.NBuilder;
using SOS.Api.Contracts.Locations.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.LocationsEndpoints;

[Collection(TestCollection.Name)]
public class SearchLocationsTests(TestFixture testFixture, ITestOutputHelper output) : TestBase(testFixture, output)
{
    [Fact]
    public async Task SearchLocations_ReturnsLocationDetailsAndPaging()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(observation => observation.Site.Id = 4060923)
                .With(observation => observation.Site.Name = "First locality")
            .TheLast(1).With(observation => observation.Site.Id = 4060924)
                .With(observation => observation.Site.Name = "Second locality")
            .Build();
        var processed = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        // Location search returns the normalized keyword values from Elasticsearch.
        var expected = processed.OrderBy(observation => observation.Location.LocationId)
            .Select(observation => new LocationSearchResultDto
            {
                Id = observation.Location.LocationId,
                Name = observation.Location.Locality.ToLowerInvariant(),
                County = observation.Location.County.Name.ToLowerInvariant(),
                Municipality = observation.Location.Municipality.Name.ToLowerInvariant(),
                Parish = observation.Location.Parish.Name.ToLowerInvariant(),
                Latitude = observation.Location.DecimalLatitude ?? 0,
                Longitude = observation.Location.DecimalLongitude ?? 0
            }).ToList();
        using var apiClient = TestFixture.CreateApiClient();
        var filter = new GeographicsFilterDto();

        using var response = await apiClient.PostAsync("/Locations/search", JsonContent.Create(filter), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var locations = await response.Content.ReadFromJsonAsync<List<LocationSearchResultDto>>(
            JsonSerializerOptions, TestContext.Current.CancellationToken);
        locations.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());

        using var pageResponse = await apiClient.PostAsync("/Locations/search?skip=1&take=1", JsonContent.Create(filter), TestContext.Current.CancellationToken);
        pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await pageResponse.Content.ReadFromJsonAsync<List<LocationSearchResultDto>>(
            JsonSerializerOptions, TestContext.Current.CancellationToken);
        page.Should().BeEquivalentTo(new[] { expected[1] });
    }
}
