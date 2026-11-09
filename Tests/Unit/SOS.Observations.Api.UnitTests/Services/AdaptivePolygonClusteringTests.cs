using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Extensions;
using SOS.Lib.Gis;
using SOS.Lib.Models.Gis;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json.Nodes;
using Xunit;
using LocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptivePolygonClusteringTests
{
    private static readonly LatLonBoundingBoxDto Bbox = LatLonBoundingBoxDto.Create(14, 60, 16, 58);

    public static IEnumerable<object[]> RenderingCases()
    {
        foreach (var rawRecords in new[] { false, true })
        foreach (var stage in new[] { 1, 2, 3 }) // metres, pixels, both
        foreach (var (mode, size, separate) in new[]
                 {
                     (CreatePolygonsMode.No, 30d, false),
                     (CreatePolygonsMode.Yes, 2d, false),
                     (CreatePolygonsMode.Yes, 30d, true),
                     (CreatePolygonsMode.WhenNeeded, 2d, false),
                     (CreatePolygonsMode.WhenNeeded, 30d, true)
                 })
            yield return new object[] { rawRecords, stage, mode, size, separate };
    }

    [Theory]
    [MemberData(nameof(RenderingCases))]
    public void DisplayedPolygonsStayOutsideBothClusteringStages(
        bool rawRecords, int stage, CreatePolygonsMode mode, double size, bool separate)
    {
        var records = new[] { Record("polygon", 0, true), Record("polygon", 0, true, taxon: 2), Record("a", 2), Record("b", 4) };
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, size, size) };

        var result = Cluster(records, polygons, mode, rawRecords, stage);

        result.Sum(feature => feature.ObservationCount).Should().Be(4);
        var properties = result.Select(feature => LocationFeatureResponseMapper.ToProperties(feature)).ToList();
        properties.SelectMany(p => p.Locations.Ids).Should().BeEquivalentTo("polygon", "a", "b");
        properties.Should().OnlyContain(p => !p.IsSummaryApproximated && !p.IsPresentationStatusApproximated);
        if (separate)
        {
            result.Should().HaveCount(2);
            var polygon = result.Single(feature => !feature.IsCluster);
            polygon.Geometry.Should().BeOfType<Polygon>();
            polygon.ObservationCount.Should().Be(2);
            polygon.TaxaCount.Should().Be(2);
            var cluster = LocationFeatureResponseMapper.ToProperties(result.Single(feature => feature.IsCluster));
            cluster.Locations.Ids.Should().BeEquivalentTo("a", "b");
            cluster.GroupingType.Should().Be(stage == 2 ? LocationGroupingType.PixelCluster : LocationGroupingType.MeterCluster);
        }
        else
        {
            result.Should().ContainSingle().Which.IsCluster.Should().BeTrue();
            result[0].Geometry.Should().BeOfType<Point>();
        }
    }

    [Theory]
    [InlineData(9, 12, false)]
    [InlineData(12, 9, false)]
    [InlineData(10, 10, true)]
    [InlineData(11, 11, true)]
    [InlineData(200, 5, false)]
    public void YesAndWhenNeededUseTheSameTenPixelClusteringThreshold(double width, double height, bool separate)
    {
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, width, height) };
        foreach (var mode in new[] { CreatePolygonsMode.Yes, CreatePolygonsMode.WhenNeeded })
        {
            var result = Cluster(new[] { Record("polygon", 0, true), Record("point", 2) }, polygons, mode);
            result.Any(feature => !feature.IsCluster && feature.Geometry is Polygon).Should().Be(separate, $"mode is {mode}");
        }
    }

    [Theory]
    [InlineData(CreatePolygonsMode.Yes)]
    [InlineData(CreatePolygonsMode.WhenNeeded)]
    public void PolygonClusteringUsesActualAreaIncludingHoles(CreatePolygonsMode mode)
    {
        var shell = (Polygon)new GeometryFactory().ToGeometry(new Envelope(500000, 500012, 6500000, 6500012));
        var hole = (Polygon)new GeometryFactory().ToGeometry(new Envelope(500001, 500011, 6500001, 6500011));
        var polygon = new Polygon(shell.Shell, new[] { hole.Shell }) { SRID = CoordinateSys.SWEREF99_TM.Srid() };
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = ToWgs84(polygon) };

        var result = Cluster(new[] { Record("polygon", 0, true), Record("point", 2) }, polygons, mode);

        result.Should().ContainSingle().Which.IsCluster.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void YesWithoutMapResolutionKeepsEvenSmallPolygonsOutsideClustering(bool rawRecords)
    {
        var records = new[] { Record("polygon", 0, true), Record("a", 2), Record("b", 4) };
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, 2, 2) };

        var result = Cluster(records, polygons, CreatePolygonsMode.Yes, rawRecords, stage: 1, metersPerPixel: null);

        result.Should().HaveCount(2);
        result.Single(feature => !feature.IsCluster).Geometry.Should().BeOfType<Polygon>();
        LocationFeatureResponseMapper.ToProperties(result.Single(feature => feature.IsCluster))
            .Locations.Ids.Should().BeEquivalentTo("a", "b");
    }

    [Theory]
    [InlineData(false, false, CreatePolygonsMode.Yes)]
    [InlineData(false, true, CreatePolygonsMode.Yes)]
    [InlineData(true, false, CreatePolygonsMode.Yes)]
    [InlineData(true, true, CreatePolygonsMode.Yes)]
    [InlineData(false, false, CreatePolygonsMode.WhenNeeded)]
    [InlineData(false, true, CreatePolygonsMode.WhenNeeded)]
    [InlineData(true, false, CreatePolygonsMode.WhenNeeded)]
    [InlineData(true, true, CreatePolygonsMode.WhenNeeded)]
    public void SmallUnclusteredPolygonRetainsGeometryOnlyWithYes(bool rawRecords, bool clusteringEnabled, CreatePolygonsMode mode)
    {
        var records = new[] { Record("polygon", 0, true), Record("distant-point", 1000) };
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, 2, 2) };
        var aggregator = new AdaptiveSearchLocationAggregator();
        var factory = new AdaptiveSearchLocationFeatureFactory(aggregator);

        var result = clusteringEnabled ? Cluster(records, polygons, mode, rawRecords)
            : rawRecords ? factory.CreateItems(records, new LocationFeatureOptions { PolygonGeometries = polygons, CreatePolygons = mode, MetersPerPixel = 1 })
            : factory.CreateItems(aggregator.CreateAggregates(records), new LocationFeatureOptions { PolygonGeometries = polygons, CreatePolygons = mode, MetersPerPixel = 1 });

        result.Should().HaveCount(2).And.OnlyContain(feature => !feature.IsCluster);
        var polygonFeature = result.Single(feature => feature.Location.LocationId == "polygon");
        if (mode == CreatePolygonsMode.Yes)
            polygonFeature.Geometry!.EqualsTopologically(polygons["polygon"]).Should().BeTrue();
        else
            polygonFeature.Geometry.Should().BeOfType<Point>();
        result.Sum(feature => feature.ObservationCount).Should().Be(2);
    }

    [Theory]
    [InlineData(false, CreatePolygonsMode.Yes)]
    [InlineData(true, CreatePolygonsMode.Yes)]
    [InlineData(false, CreatePolygonsMode.WhenNeeded)]
    [InlineData(true, CreatePolygonsMode.WhenNeeded)]
    public void CoincidentPolygonsUseTheirWholeUnionBeforeClustering(bool rawRecords, CreatePolygonsMode mode)
    {
        var locationCount = AdaptiveSearchFeatureLocationsDto.MaxLocationItems + 2;
        var records = Enumerable.Range(0, locationCount).Select(i => Record($"polygon-{i}", 0, true))
            .Append(Record("a", 2)).Append(Record("b", 4)).ToArray();
        var polygons = Enumerable.Range(0, locationCount).ToDictionary(i => $"polygon-{i}", i => Rectangle(i * 2, 2, 12));

        var result = Cluster(records, polygons, mode, rawRecords);

        result.Should().HaveCount(2);
        var polygonFeature = result.Single(feature => !feature.IsCluster);
        var dto = LocationFeatureResponseMapper.ToProperties(polygonFeature);
        dto.GroupingType.Should().Be(LocationGroupingType.CoLocatedLocations);
        dto.Locations.Ids.Should().HaveCount(locationCount);
        dto.Locations.Items.Should().HaveCount(AdaptiveSearchFeatureLocationsDto.MaxLocationItems);
        polygonFeature.ObservationCount.Should().Be(locationCount);
        var expectedUnion = NetTopologySuite.Operation.Union.UnaryUnionOp.Union(polygons.Values.ToList());
        polygonFeature.Geometry!.EqualsTopologically(expectedUnion).Should().BeTrue();
    }

    [Fact]
    public void MultiPolygonCanStayOutsideClustering()
    {
        var geometry = new MultiPolygon(new[] { (Polygon)Rectangle(0, 8, 12), (Polygon)Rectangle(20, 8, 12) });
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = geometry };

        var result = Cluster(new[] { Record("polygon", 0, true), Record("a", 2), Record("b", 4) }, polygons, CreatePolygonsMode.WhenNeeded);

        result.Should().HaveCount(2);
        result.Single(feature => !feature.IsCluster).Geometry.Should().BeOfType<MultiPolygon>();
    }

    [Theory]
    [InlineData(CreatePolygonsMode.Yes)]
    [InlineData(CreatePolygonsMode.WhenNeeded)]
    public void MissingPolygonGeometryStillAllowsCentroidClustering(CreatePolygonsMode mode)
    {
        var result = Cluster(new[] { Record("polygon", 0, true), Record("point", 2) }, new(), mode);
        result.Should().ContainSingle().Which.IsCluster.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GeneralizedObservationsKeepTheirExistingClusteringBehavior(bool rawRecords)
    {
        var records = new[] { Record("polygon", 0, true, generalized: true), Record("point", 2) };
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, 100, 100) };

        var result = Cluster(records, polygons, CreatePolygonsMode.WhenNeeded, rawRecords);

        result.Should().ContainSingle().Which.IsCluster.Should().BeTrue();
    }

    [Theory]
    [InlineData(CreatePolygonsMode.Yes)]
    [InlineData(CreatePolygonsMode.WhenNeeded)]
    public void CachedAggregatesAreReevaluatedForEachResolutionWithoutChangingSummaries(CreatePolygonsMode mode)
    {
        var records = new[] { Record("polygon", 0, true), Record("point", 2) };
        var aggregator = new AdaptiveSearchLocationAggregator();
        var aggregates = aggregator.CreateAggregates(records, new Dictionary<string, long> { ["polygon"] = 356, ["point"] = 1 }, true, false);
        var factory = new AdaptiveSearchLocationFeatureFactory(aggregator);
        var geometry = Rectangle(0, 30, 30);
        var originalCoordinates = geometry.AsText();
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = geometry };

        List<LocationFeature> AtResolution(double metresPerPixel) => factory.CreateItems(aggregates, new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = 100, ClusterPixels = true, MetersPerPixel = metresPerPixel, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 0, PolygonGeometries = polygons, CreatePolygons = mode });
        var close = AtResolution(1);
        var distant = AtResolution(10);
        var closeAgain = AtResolution(1);

        close.Should().HaveCount(2);
        distant.Should().ContainSingle().Which.IsCluster.Should().BeTrue();
        closeAgain.Should().HaveCount(2);
        var polygon = LocationFeatureResponseMapper.ToProperties(close.Single(feature => feature.Geometry is Polygon));
        polygon.ObservationCount.Should().Be(356);
        polygon.IsSummaryApproximated.Should().BeTrue();
        polygon.IsPresentationStatusApproximated.Should().BeFalse();
        polygon.Presentation.Status.Should().Be(LocationPresentationStatusDto.Present);
        geometry.AsText().Should().Be(originalCoordinates);
        aggregates[0].ObservationSummary.ObservationCount.Should().Be(356);
    }

    [Fact]
    public void OnlyClusterableLocationsCountTowardsTheClusteringThreshold()
    {
        var records = new[] { Record("polygon", 0, true), Record("a", 2), Record("b", 4) };
        var aggregator = new AdaptiveSearchLocationAggregator();
        var factory = new AdaptiveSearchLocationFeatureFactory(aggregator);
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, 30, 30) };

        var result = factory.CreateItems(aggregator.CreateAggregates(records), new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = null, ClusterPixels = true, MetersPerPixel = 1, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 2, PolygonGeometries = polygons, CreatePolygons = CreatePolygonsMode.WhenNeeded });

        result.Should().HaveCount(3).And.OnlyContain(feature => !feature.IsCluster);
    }

    [Theory]
    [InlineData(false, CreatePolygonsMode.Yes, false)]
    [InlineData(false, CreatePolygonsMode.Yes, true)]
    [InlineData(false, CreatePolygonsMode.WhenNeeded, false)]
    [InlineData(false, CreatePolygonsMode.WhenNeeded, true)]
    [InlineData(true, CreatePolygonsMode.Yes, false)]
    [InlineData(true, CreatePolygonsMode.Yes, true)]
    [InlineData(true, CreatePolygonsMode.WhenNeeded, false)]
    [InlineData(true, CreatePolygonsMode.WhenNeeded, true)]
    public void ProximityGrouping_KeepsLargePolygonsSeparateAndUsesOnlyClusterableCount(
        bool rawRecords, CreatePolygonsMode mode, bool large)
    {
        var records = new[] { Record("polygon", 0, true), Record("a", 1), Record("b", 2) };
        var aggregator = new AdaptiveSearchLocationAggregator();
        var factory = new AdaptiveSearchLocationFeatureFactory(aggregator);
        var size = large ? 30 : 2;
        var polygons = new Dictionary<string, Geometry> { ["polygon"] = Rectangle(0, size, size) };
        var result = rawRecords
            ? factory.CreateItems(records, new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = null, ClusterPixels = large, MetersPerPixel = 1, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 2, PolygonGeometries = polygons, CreatePolygons = mode, DetailClusterRadiusPx = 2 })
            : factory.CreateItems(aggregator.CreateAggregates(records), new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = null, ClusterPixels = large, MetersPerPixel = 1, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 2, PolygonGeometries = polygons, CreatePolygons = mode, DetailClusterRadiusPx = 2 });

        result.Should().HaveCount(large ? 2 : 1);
        result.Sum(feature => feature.ObservationCount).Should().Be(3);
        var cluster = result.Single(feature => feature.IsCluster);
        cluster.GroupingType.Should().Be(LocationGroupingType.PixelCluster);
        cluster.ClusterRadiusMeters.Should().Be(2);
        if (large)
            result.Single(feature => !feature.IsCluster).Geometry.Should().BeOfType<Polygon>();
        else
            cluster.Geometry.Should().BeOfType<Point>();
    }

    private static List<LocationFeature> Cluster(JsonObject[] records, Dictionary<string, Geometry> polygons,
        CreatePolygonsMode mode, bool rawRecords = false, int stage = 3, double? metersPerPixel = 1)
    {
        var aggregator = new AdaptiveSearchLocationAggregator();
        var factory = new AdaptiveSearchLocationFeatureFactory(aggregator);
        var metres = stage is 1 or 3 ? 100d : (double?)null;
        var pixels = stage is 2 or 3;
        return rawRecords
            ? factory.CreateItems(records, new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = metres, ClusterPixels = pixels, MetersPerPixel = metersPerPixel, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 0, PolygonGeometries = polygons, CreatePolygons = mode })
            : factory.CreateItems(aggregator.CreateAggregates(records), new LocationFeatureOptions { BoundingBox = Bbox, ClusterRadiusMeters = metres, ClusterPixels = pixels, MetersPerPixel = metersPerPixel, ClusterRadiusPx = 40, MinLocationsPerCluster = 2, MinLocationCountBeforeClustering = 0, PolygonGeometries = polygons, CreatePolygons = mode });
    }

    private static JsonObject Record(string id, double offset, bool polygon = false, bool generalized = false, int taxon = 1)
    {
        var point = (Point)ToWgs84(new Point(500000 + offset, 6500000));
        return new JsonObject
        {
            ["isGeneralized"] = generalized,
            ["location"] = new JsonObject
            {
                ["locationId"] = id, ["type"] = (int)(polygon ? LocationType.Polygon : LocationType.Point),
                ["decimalLongitude"] = point.X, ["decimalLatitude"] = point.Y,
                ["sweref99TmX"] = 500000d + offset, ["sweref99TmY"] = 6500000d,
                ["coordinateUncertaintyInMeters"] = generalized ? 1000 : 10
            },
            ["occurrence"] = new JsonObject { ["occurrenceId"] = $"{id}-{taxon}", ["isPositiveObservation"] = true },
            ["taxon"] = new JsonObject { ["id"] = taxon }
        };
    }

    private static Geometry Rectangle(double offset, double width, double height) => ToWgs84(
        new GeometryFactory().ToGeometry(new Envelope(500000 + offset, 500000 + offset + width, 6500000, 6500000 + height)));

    private static Geometry ToWgs84(Geometry geometry) =>
        geometry.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, usePointCache: false);
}
