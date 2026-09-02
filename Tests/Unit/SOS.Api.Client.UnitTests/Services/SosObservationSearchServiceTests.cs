using FluentAssertions;
using GeoJSON.Text.Geometry;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosObservationSearchServiceTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public async Task HasSensitiveObservationsAsync_CountsOnlySensitiveObservations(
        int count,
        bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new Mock<ISosObservationsClient>();
        string? sentFilter = null;
        client.Setup(value => value.CountInternalJsonAsync(
                It.IsAny<string>(),
                It.Is<CountInternalOptions>(options => options.SensitiveObservations),
                cancellationToken))
            .Callback<string, CountInternalOptions?, CancellationToken>((json, _, _) => sentFilter = json)
            .ReturnsAsync(count);
        var service = new SosObservationSearchService(client.Object);
        var filter = new SearchFilterInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.BothPublicAndSensitive
        };

        var result = await service.HasSensitiveObservationsAsync(filter, cancellationToken: cancellationToken);

        result.Should().Be(expected);
        filter.ProtectionFilter.Should().Be(ProtectionFilterDto.BothPublicAndSensitive);
        using var json = JsonDocument.Parse(sentFilter!);
        json.RootElement.GetProperty("protectionFilter").GetString().Should().Be("Sensitive");
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_ParsesGeoJsonIntoTypedFeatureCollection()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var geoJson = """
        {
          "type": "FeatureCollection",
          "bbox": [10.0, 55.0, 20.0, 65.0],
          "features": [
            {
              "type": "Feature",
              "id": "cluster_1",
              "geometry": { "type": "Point", "coordinates": [15.5, 59.3] },
              "properties": {
                "isCluster": true,
                "observationCount": 42,
                "isSummaryApproximated": true,
                "cluster": { "radiusMeters": 500.0, "radiusMetersMin": 120.5 },
                "presentation": {
                  "status": "Cluster",
                  "statusCounts": { "Present": 40, "Treated": 2 },
                  "cutoffYearStatus": null,
                  "isAllObservationsSensitive": false
                },
                "locations": { "count": 5, "ids": ["loc1", "loc2"], "location": null },
                "observationSummary": {
                  "taxaCount": 3,
                  "statusCounts": {
                    "foundCount": 40,
                    "treatedCount": 2,
                    "diffusedCount": 0,
                    "redlistedCount": 1,
                    "invasiveInSwedenCount": 0
                  }
                }
              }
            }
          ]
        }
        """;
        var expectedResponse = new AdaptiveSearchLocationsResponse { GeoJson = geoJson };
        var client = new Mock<ISosObservationsClient>();
        client.Setup(value => value.AdaptiveSearchLocationsInternalAsync(
                It.IsAny<SearchFilterInternalDto>(),
                It.IsAny<AdaptiveSearchLocationsOptions>(),
                cancellationToken))
            .ReturnsAsync(expectedResponse);
        var service = new SosObservationSearchService(client.Object);
        var filter = new SearchFilterInternalDto();

        var (response, featureCollection) = await service.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
            filter,
            cancellationToken: cancellationToken);

        response.Should().BeSameAs(expectedResponse);
        featureCollection.BoundingBox.Should().Equal(10.0, 55.0, 20.0, 65.0);
        featureCollection.Features.Should().HaveCount(1);

        var feature = featureCollection.Features[0];
        feature.Id.Should().Be("cluster_1");
        feature.Geometry.Should().BeOfType<Point>();
        ((Point)feature.Geometry!).Coordinates.Longitude.Should().Be(15.5);
        ((Point)feature.Geometry!).Coordinates.Latitude.Should().Be(59.3);

        var properties = feature.Properties;
        properties.IsCluster.Should().BeTrue();
        properties.ObservationCount.Should().Be(42);
        properties.IsSummaryApproximated.Should().BeTrue();
        properties.Cluster.Should().NotBeNull();
        properties.Cluster!.RadiusMeters.Should().Be(500.0);
        properties.Presentation.Status.Should().Be(LocationPresentationStatus.Cluster);
        properties.Presentation.StatusCounts[LocationPresentationStatus.Present].Should().Be(40);
        properties.Locations.Count.Should().Be(5);
        properties.Locations.Ids.Should().Equal("loc1", "loc2");
        properties.ObservationSummary.TaxaCount.Should().Be(3);
        properties.ObservationSummary.StatusCounts.FoundCount.Should().Be(40);
        properties.ObservationSummary.StatusCounts.RedlistedCount.Should().Be(1);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_EmptyGeoJson_ReturnsEmptyFeatureCollection()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new Mock<ISosObservationsClient>();
        client.Setup(value => value.AdaptiveSearchLocationsInternalAsync(
                It.IsAny<SearchFilterInternalDto>(),
                It.IsAny<AdaptiveSearchLocationsOptions>(),
                cancellationToken))
            .ReturnsAsync(new AdaptiveSearchLocationsResponse { GeoJson = string.Empty });
        var service = new SosObservationSearchService(client.Object);

        var (_, featureCollection) = await service.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
            new SearchFilterInternalDto(),
            cancellationToken: cancellationToken);

        featureCollection.Features.Should().BeEmpty();
        featureCollection.BoundingBox.Should().BeNull();
    }
}
