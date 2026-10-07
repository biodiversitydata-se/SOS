using GeoJSON.Text.Geometry;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class LocationExtensionsTests
{
    [Theory]
    [InlineData("County")]
    [InlineData("Province")]
    [InlineData("Municipality")]
    [InlineData("Parish")]
    public void ToDsLocationDto_MissingAreaDoesNotDiscardOtherLocationData(string area)
    {
        var location = CreateLocation();
        typeof(Location).GetProperty(area)!.SetValue(location, null);

        var result = location.ToDsLocationDto(CoordinateSys.WGS84);

        Assert.Null(typeof(DsLocationDto).GetProperty(area)!.GetValue(result));
        foreach (var otherArea in new[] { "County", "Province", "Municipality", "Parish" }.Except([area]))
            Assert.NotNull(typeof(DsLocationDto).GetProperty(otherArea)!.GetValue(result));
        Assert.Equal("site-1", result.LocationID);
        Assert.Equal("Survey site", result.Locality);
        Assert.Equal("Remarks", result.LocationRemarks);
        var point = Assert.IsType<Point>(result.Emplacement);
        Assert.Equal(18, point.Coordinates.Longitude);
        Assert.Equal(59, point.Coordinates.Latitude);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-id")]
    public void ToDsLocationDto_MissingOrUnparseableAreaIdsProduceNull(string? featureId)
    {
        var location = CreateLocation();
        location.County.FeatureId = featureId!;
        location.Province.FeatureId = featureId!;
        location.Municipality.FeatureId = featureId!;
        location.Parish.FeatureId = featureId!;

        var result = location.ToDsLocationDto(CoordinateSys.WGS84);

        Assert.Null((object?)result.County);
        Assert.Null((object?)result.Province);
        Assert.Null((object?)result.Municipality);
        Assert.Null((object?)result.Parish);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        foreach (var area in new[] { "county", "province", "municipality", "parish" })
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty(area).ValueKind);
    }

    [Fact]
    public void ToDsLocationDto_NullLocationRemainsNull()
    {
        Location location = null!;
        Assert.Null(location.ToDsLocationDto(CoordinateSys.WGS84));
    }

    private static Location CreateLocation() => new()
    {
        LocationId = "site-1",
        Locality = "Survey site",
        LocationRemarks = "Remarks",
        County = new Area { FeatureId = "1" },
        Province = new Area { FeatureId = "1" },
        Municipality = new Area { FeatureId = "180" },
        Parish = new Area { FeatureId = "18001" },
        Point = new NetTopologySuite.Geometries.Point(18, 59)
    };
}
