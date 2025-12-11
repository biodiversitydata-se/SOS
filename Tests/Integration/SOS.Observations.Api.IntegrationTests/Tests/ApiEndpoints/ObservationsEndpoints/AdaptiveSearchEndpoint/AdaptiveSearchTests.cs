using FizzWare.NBuilder;
using NetTopologySuite.Features;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class AdaptiveSearchTests : TestBase
{
    public AdaptiveSearchTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsObservations_WhenTotalCountBelowObservationsLimit()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()            
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearch?observationsLimit=100",
            JsonContent.Create(searchFilter));        
        var strGeoJson = await response.Content.ReadAsStringAsync();
        FeatureCollection featureCollection = GeoJsonHelper.ReadGeoJsonFromString(strGeoJson);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);        
        featureCollection.Count.Should().Be(100);
        response.GetHeaderValue("X-Result-Type").Should().Be("observations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("100");
        response.GetHeaderValue("X-Result-Count").Should().Be("100");
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsGridAggregation_WhenTotalCountExceedsObservationsLimit()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearch?observationsLimit=50",
            JsonContent.Create(searchFilter));
        var strGeoJson = await response.Content.ReadAsStringAsync();
        FeatureCollection featureCollection = GeoJsonHelper.ReadGeoJsonFromString(strGeoJson);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        featureCollection.Count.Should().BeGreaterThanOrEqualTo(1, because: "at least one grid cell is returned");
        response.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("100");
        Convert.ToInt32(response.GetHeaderValue("X-Zoom")).Should().BeGreaterThanOrEqualTo(1, because: "zoom should be greater than or equal to 1");
        Convert.ToInt32(response.GetHeaderValue("X-Result-Count")).Should().BeGreaterThanOrEqualTo(1, because: "at least one grid cell is returned");
    }
}