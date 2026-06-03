using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Dtos;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchInternalEndpoint;

[Collection(TestCollection.Name)]
public class PolygonLocationTests : TestBase
{
    public PolygonLocationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchInternal_ReturnsPolygonLocationsMetadata_WhenIncludePolygonLocationsIsTrue()
    {
        // Arrange
        const int polygonSiteId1 = 10;
        const int polygonSiteId2 = 20;
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(30)
                .With(o => o.Site.HasGeometry = true)
                .With(o => o.Site.Id = polygonSiteId1)
            .TheNext(30)
                .With(o => o.Site.HasGeometry = true)
                .With(o => o.Site.Id = polygonSiteId2)
            .TheNext(40)
                .With(o => o.Site.HasGeometry = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/search?includePolygonLocations=true",
            JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<GeoPagedResultWithMetadataDto<Observation>>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100);
        result.Metadata.Should().NotBeNull();
        result.Metadata!.PolygonLocations.Should().NotBeNull();
        result.Metadata.PolygonLocations.Should().ContainKey($"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        result.Metadata.PolygonLocations.Should().ContainKey($"urn:lsid:artportalen.se:site:{polygonSiteId2}");
    }

    [Fact]
    public async Task ObservationsBySearchInternal_ReturnsNoMetadata_WhenIncludePolygonLocationsIsFalse()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(60)
                .With(o => o.Site.HasGeometry = true)
            .TheNext(40)
                .With(o => o.Site.HasGeometry = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/search",
            JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<GeoPagedResultWithMetadataDto<Observation>>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100);
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task ObservationsBySearchInternal_ReturnsNoMetadata_WhenNoPolygonObservationsExist()
    {
        // Arrange - all observations are point-type locations
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .All()
                .With(o => o.Site.HasGeometry = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/search?includePolygonLocations=true",
            JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<GeoPagedResultWithMetadataDto<Observation>>(JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(100);
        result.Metadata?.PolygonLocations?.Should().BeNull();
    }
}
