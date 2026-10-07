using GeoJSON.Text.Feature;
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
        featureCollection.Should().BeOfType<FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto>>();
        featureCollection.BoundingBoxes.Should().Equal(10.0, 55.0, 20.0, 65.0);
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
        properties.Presentation.Status.Should().Be(LocationPresentationStatusDto.Cluster);
        properties.Presentation.StatusCounts[LocationPresentationStatusDto.Present].Should().Be(40);
        properties.Locations.Count.Should().Be(5);
        properties.Locations.Ids.Should().Equal("loc1", "loc2");
        properties.ObservationSummary.TaxaCount.Should().Be(3);
        properties.ObservationSummary.StatusCounts.FoundCount.Should().Be(40);
        properties.ObservationSummary.StatusCounts.RedlistedCount.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    [InlineData("{\"type\":\"FeatureCollection\",\"features\":[]}")]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_EmptyGeoJson_ReturnsEmptyFeatureCollection(string? geoJson)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new Mock<ISosObservationsClient>();
        client.Setup(value => value.AdaptiveSearchLocationsInternalAsync(
                It.IsAny<SearchFilterInternalDto>(),
                It.IsAny<AdaptiveSearchLocationsOptions>(),
                cancellationToken))
            .ReturnsAsync(new AdaptiveSearchLocationsResponse { GeoJson = geoJson! });
        var service = new SosObservationSearchService(client.Object);

        var (_, featureCollection) = await service.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
            new SearchFilterInternalDto(),
            cancellationToken: cancellationToken);

        featureCollection.Features.Should().BeEmpty();
        featureCollection.BoundingBoxes.Should().BeNull();
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_PreservesDistinctLocationId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const string geoJson = """
        {"type":"FeatureCollection","features":[{"type":"Feature","id":"site-1",
         "geometry":{"type":"Point","coordinates":[18.12345,59.12345]},
         "properties":{"isCluster":false,"locations":{"count":1,"ids":["site-1"],
         "location":{"locationId":"site-1","distinctLocationId":"loc_1_5912345_1812345_un","type":"Point"}}}}]}
        """;
        var client = new Mock<ISosObservationsClient>();
        client.Setup(value => value.AdaptiveSearchLocationsInternalAsync(
                It.IsAny<SearchFilterInternalDto>(),
                It.IsAny<AdaptiveSearchLocationsOptions>(),
                cancellationToken))
            .ReturnsAsync(new AdaptiveSearchLocationsResponse { GeoJson = geoJson });
        var service = new SosObservationSearchService(client.Object);

        var (_, featureCollection) = await service.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
            new SearchFilterInternalDto(), cancellationToken: cancellationToken);

        featureCollection.Features.Should().ContainSingle();
        featureCollection.Features[0].Properties.Locations.Location!.DistinctLocationId
            .Should().Be("loc_1_5912345_1812345_un");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("{}")]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_OptionalProperties_KeepDefaults(string? propertiesJson)
    {
        var properties = propertiesJson is null ? "" : ",\"properties\":" + propertiesJson;
        var geoJson = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":null" + properties + "}]}";

        var featureCollection = await ParseGeoJsonAsync(geoJson);

        var feature = featureCollection.Features.Should().ContainSingle().Which;
        feature.Id.Should().BeNull();
        feature.Geometry.Should().BeNull();
        feature.Properties.Should().BeEquivalentTo(new AdaptiveSearchLocationFeaturePropertiesDto());
        featureCollection.BoundingBoxes.Should().BeNull();
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_PreservesFeatureBoundingBoxWithDefaultProperties()
    {
        const string geoJson = """
        {"type":"FeatureCollection","features":[{"type":"Feature","id":"site:1",
         "bbox":[15,59,17,61],"geometry":null,"properties":null}]}
        """;

        var featureCollection = await ParseGeoJsonAsync(geoJson);

        var feature = featureCollection.Features.Should().ContainSingle().Which;
        feature.Id.Should().Be("site:1");
        feature.BoundingBoxes.Should().Equal(15, 59, 17, 61);
        feature.Geometry.Should().BeNull();
        feature.Properties.Should().BeEquivalentTo(new AdaptiveSearchLocationFeaturePropertiesDto());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_DefaultProperties_PreserveGeometryAndOtherFeatures(string? propertiesJson)
    {
        var properties = propertiesJson is null ? "" : ",\"properties\":" + propertiesJson;
        var geoJson = $$$"""
        {"type":"FeatureCollection","features":[
          {"type":"Feature","id":"first","geometry":null,"properties":{"observationCount":4}},
          {"type":"Feature","id":"default","bbox":[15,59,17,61],
           "geometry":{"type":"Point","coordinates":[16,60,125]}
           {{{properties}}}
          },
          {"type":"Feature","id":"last","geometry":null,"properties":{"observationCount":7}}
        ]}
        """;

        var featureCollection = await ParseGeoJsonAsync(geoJson);

        featureCollection.Features.Select(feature => feature.Id).Should().Equal("first", "default", "last");
        featureCollection.Features[0].Properties.ObservationCount.Should().Be(4);
        featureCollection.Features[2].Properties.ObservationCount.Should().Be(7);
        var defaultFeature = featureCollection.Features[1];
        defaultFeature.Properties.Should().BeEquivalentTo(new AdaptiveSearchLocationFeaturePropertiesDto());
        defaultFeature.BoundingBoxes.Should().Equal(15, 59, 17, 61);
        var point = defaultFeature.Geometry.Should().BeOfType<Point>().Subject;
        point.Coordinates.Longitude.Should().Be(16);
        point.Coordinates.Latitude.Should().Be(60);
        point.Coordinates.Altitude.Should().Be(125);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_PreservesMixedGeometriesAndFilterAreas()
    {
        const string geoJson = """
        {"type":"FeatureCollection","bbox":[15,59,17,61],"features":[
          {"type":"Feature","id":"point","geometry":{"type":"Point","coordinates":[16,60,125]},
           "properties":{"observationCount":4,"unknownFutureProperty":{"value":true}}},
          {"type":"Feature","geometry":{"type":"Polygon","coordinates":[
            [[15,59,10],[17,59,20],[17,61,30],[15,59,10]],
            [[15.5,59.5,40],[16.5,59.5,50],[16.5,60.5,60],[15.5,59.5,40]]]},
           "properties":{"FeatureType":"FilterArea.OwnPolygon","Name":"My area"}},
          {"type":"Feature","id":"grid","geometry":{"type":"MultiPolygon","coordinates":[
            [[[15,59],[16,59],[16,60],[15,59]]],
            [[[16,60],[17,60],[17,61],[16,60]]]]},
           "properties":{"observationCount":7}}
        ]}
        """;

        var featureCollection = await ParseGeoJsonAsync(geoJson);

        featureCollection.BoundingBoxes.Should().Equal(15, 59, 17, 61);
        featureCollection.Features.Should().HaveCount(3);
        var pointFeature = featureCollection.Features[0];
        pointFeature.Id.Should().Be("point");
        pointFeature.Properties.ObservationCount.Should().Be(4);
        var point = pointFeature.Geometry.Should().BeOfType<Point>().Subject;
        point.Coordinates.Longitude.Should().Be(16);
        point.Coordinates.Latitude.Should().Be(60);
        point.Coordinates.Altitude.Should().Be(125);

        var area = featureCollection.Features[1];
        area.Id.Should().BeNull();
        area.Properties.Should().BeEquivalentTo(new AdaptiveSearchLocationFeaturePropertiesDto());
        var polygon = area.Geometry.Should().BeOfType<Polygon>().Subject;
        polygon.Coordinates.Should().HaveCount(2);
        polygon.Coordinates[0].Coordinates[0].Longitude.Should().Be(15);
        polygon.Coordinates[0].Coordinates[0].Latitude.Should().Be(59);
        polygon.Coordinates[0].Coordinates[0].Altitude.Should().Be(10);
        polygon.Coordinates[1].Coordinates[0].Longitude.Should().Be(15.5);
        polygon.Coordinates[1].Coordinates[0].Latitude.Should().Be(59.5);
        polygon.Coordinates[1].Coordinates[0].Altitude.Should().Be(40);

        var grid = featureCollection.Features[2];
        grid.Id.Should().Be("grid");
        grid.Properties.ObservationCount.Should().Be(7);
        grid.Geometry.Should().BeOfType<MultiPolygon>().Subject.Coordinates.Should().HaveCount(2);
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_ReadsPropertyNamesCaseInsensitively()
    {
        const string geoJson = """
        {"Type":"FeatureCollection","BBox":[15,59,17,61],"Features":[
          {"Type":"Feature","Id":"point","Geometry":{"type":"Point","coordinates":[16,60]},
           "Properties":{"ObservationCount":4,"Presentation":{"Status":"Present","StatusCounts":{"Present":4}}}}
        ]}
        """;

        var featureCollection = await ParseGeoJsonAsync(geoJson);

        featureCollection.BoundingBoxes.Should().Equal(15, 59, 17, 61);
        var feature = featureCollection.Features.Should().ContainSingle().Which;
        feature.Id.Should().Be("point");
        feature.Properties.ObservationCount.Should().Be(4);
        feature.Properties.Presentation.StatusCounts[LocationPresentationStatusDto.Present].Should().Be(4);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":null,\"properties\":{\"observationCount\":\"invalid\"}}]}")]
    public async Task AdaptiveSearchLocationsInternalAsFeatureCollectionAsync_InvalidJson_StillThrows(string geoJson)
    {
        var action = () => ParseGeoJsonAsync(geoJson);

        await action.Should().ThrowAsync<JsonException>();
    }

    private static async Task<FeatureCollection<AdaptiveSearchLocationFeaturePropertiesDto>> ParseGeoJsonAsync(string geoJson)
    {
        var client = new Mock<ISosObservationsClient>();
        client.Setup(value => value.AdaptiveSearchLocationsInternalAsync(
                It.IsAny<SearchFilterInternalDto>(),
                It.IsAny<AdaptiveSearchLocationsOptions>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync(new AdaptiveSearchLocationsResponse { GeoJson = geoJson });
        var service = new SosObservationSearchService(client.Object);

        var (_, featureCollection) = await service.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync(
            new SearchFilterInternalDto(), cancellationToken: TestContext.Current.CancellationToken);
        return featureCollection;
    }
}
