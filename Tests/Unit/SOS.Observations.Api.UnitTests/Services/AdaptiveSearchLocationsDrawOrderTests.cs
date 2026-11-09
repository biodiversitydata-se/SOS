using Microsoft.AspNetCore.Http;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Gis;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json;
using Xunit;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptiveSearchLocationsDrawOrderTests
{
    [Fact]
    public async Task ResponseOrdersPolygonsLargestFirstThenLinesAndPointsIncludingClusters()
    {
        var cluster = Feature("cluster", new Point(15, 59));
        cluster.IsCluster = true;
        cluster.GroupingType = LocationGroupingType.PixelCluster;
        var input = new[]
        {
            Feature("point", new Point(15, 59)),
            Feature("small", Read("POLYGON ((15 59, 15 59.1, 15.1 59.1, 15.1 59, 15 59))")),
            Feature("line", Read("LINESTRING (15 59, 15.1 59.1)")),
            cluster,
            Feature("large", Read("POLYGON ((15 59, 15 60, 16 60, 16 59, 15 59))")),
            Feature("multiline", Read("MULTILINESTRING ((15 59, 15.1 59.1), (15.2 59, 15.3 59.1))")),
            Feature("multipoint", Read("MULTIPOINT ((15 59), (15.1 59.1))"))
        };

        var result = await Write(input);

        Assert.Equal(new[] { "large", "small", "line", "multiline", "point", "cluster", "multipoint" }, Ids(result));
        Assert.Equal(input.Sum(f => f.ObservationCount), result.Sum(f => f.GetProperty("properties").GetProperty("observationCount").GetInt32()));
        Assert.Equal("Point", result[5].GetProperty("geometry").GetProperty("type").GetString());
        Assert.StartsWith("cluster-", result[5].GetProperty("id").GetString());
    }

    [Fact]
    public async Task PolygonOrderUsesActualAreaSubtractingHolesAndSummingMultiPolygonParts()
    {
        var input = new[]
        {
            // A large envelope with a hole leaves less area than either other feature.
            Feature("hole", Read("POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (0.1 0.1, 9.9 0.1, 9.9 9.9, 0.1 9.9, 0.1 0.1))")),
            Feature("solid", Read("POLYGON ((0 0, 0 3, 3 3, 3 0, 0 0))")),
            // Each part is smaller than 'solid', but their combined area is larger.
            Feature("multi", Read("MULTIPOLYGON (((0 0, 0 2, 3 2, 3 0, 0 0)), ((4 0, 4 2, 7 2, 7 0, 4 0)))"))
        };
        var originalGeometries = input.Select(f => f.Geometry!.AsText()).ToArray();

        var result = await Write(input);

        Assert.Equal(new[] { "multi", "solid", "hole" }, Ids(result));
        Assert.Equal("MultiPolygon", result[0].GetProperty("geometry").GetProperty("type").GetString());
        Assert.Equal(2, result[2].GetProperty("geometry").GetProperty("coordinates").GetArrayLength());
        Assert.Equal(originalGeometries, input.Select(f => f.Geometry!.AsText()));
        Assert.Equal(new[] { "hole", "solid", "multi" }, input.Select(f => f.LocationIds.Single()));
    }

    [Fact]
    public async Task EqualAreasRetainInputOrderRegardlessOfRingOrientation()
    {
        var input = new[]
        {
            Feature("first", Read("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))")),
            Feature("second", Read("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))"))
        };

        Assert.Equal(new[] { "first", "second" }, Ids(await Write(input)));
        Assert.Equal(new[] { "second", "first" }, Ids(await Write(input.Reverse())));
    }

    [Fact]
    public async Task InlineFilterAreasStayBeforeLocationsAndMissingGeometryIsOmitted()
    {
        var filterArea = new Feature(Read("POLYGON ((0 0, 0 1, 1 1, 0 0))"),
            new AttributesTable { { "FeatureType", "FilterArea.Area" } });
        var result = await Write(new[]
        {
            Feature("point", new Point(15, 59)),
            Feature("missing", null),
            Feature("polygon", Read("POLYGON ((0 0, 0 10, 10 10, 0 0))"))
        }, [filterArea]);

        Assert.Equal(3, result.Length);
        Assert.Equal("FilterArea.Area", result[0].GetProperty("properties").GetProperty("FeatureType").GetString());
        Assert.Equal(new[] { "polygon", "point" }, Ids(result.Skip(1)));
    }

    private static Geometry Read(string wkt) => new WKTReader().Read(wkt);

    private static LocationFeature Feature(string id, Geometry? geometry) => new()
    {
        Geometry = geometry,
        Location = new Location { LocationId = id, DecimalLongitude = 15, DecimalLatitude = 59 },
        LocationIds = [id],
        ObservationCount = 1
    };

    private static string?[] Ids(IEnumerable<JsonElement> features) => features
        .Select(f => f.GetProperty("properties").GetProperty("locations").GetProperty("ids")[0].GetString()).ToArray();

    private static async Task<JsonElement[]> Write(IEnumerable<LocationFeature> features, List<Feature>? areas = null)
    {
        var context = new DefaultHttpContext();
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        await AdaptiveSearchLocationsGeoJsonWriter.WriteFeatureCollectionAsync(features, null, context.Response,
            geographicAreas: areas, cancellationToken: TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.GetProperty("features").EnumerateArray().Select(f => f.Clone()).ToArray();
    }
}
