using FluentAssertions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Gis;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class GeoTileHelperTests
{
    [Fact]
    public void CalculateGeotileZoom_NullBoundingBox_ReturnsZoomZero()
    {
        var zoom = GeoTileHelper.CalculateGeotileZoom(null, maxBuckets: 10000, maxZoom: 21);

        zoom.Should().Be(0);
    }

    [Fact]
    public void CalculateGeotileZoom_BoundingBoxMissingCoordinates_ReturnsZoomZero()
    {
        var bbox = new LatLonBoundingBox
        {
            TopLeft = null!,
            BottomRight = new LatLonCoordinate(55.0, 20.0)
        };

        var zoom = GeoTileHelper.CalculateGeotileZoom(bbox, maxBuckets: 10000, maxZoom: 21);

        zoom.Should().Be(0);
    }

    [Fact]
    public void CalculateGeotileZoom_ValidBoundingBox_ReturnsZoomWithinBounds()
    {
        var bbox = new LatLonBoundingBox
        {
            TopLeft = new LatLonCoordinate(60.0, 15.0),
            BottomRight = new LatLonCoordinate(55.0, 20.0)
        };

        var zoom = GeoTileHelper.CalculateGeotileZoom(bbox, maxBuckets: 10000, maxZoom: 21);

        zoom.Should().BeInRange(0, 21);
    }
}
