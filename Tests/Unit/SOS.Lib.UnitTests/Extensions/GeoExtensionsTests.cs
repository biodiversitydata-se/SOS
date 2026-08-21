using FluentAssertions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Gis;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class GeoExtensionsTests
{
    [Fact]
    public void ToLatLonBoundingBoxDto_NullBoundingBox_ReturnsNull()
    {
        LatLonBoundingBox? bbox = null;

        var result = bbox.ToLatLonBoundingBoxDto();

        result.Should().BeNull();
    }

    [Fact]
    public void ToXYBoundingBoxDto_NullBoundingBox_ReturnsNull()
    {
        LatLonBoundingBox? bbox = null;

        var result = bbox.ToXYBoundingBoxDto();

        result.Should().BeNull();
    }

    [Fact]
    public void ToLatLonBoundingBoxDto_ValidBoundingBox_MapsCoordinates()
    {
        var bbox = new LatLonBoundingBox
        {
            TopLeft = new LatLonCoordinate(60.0, 15.0),
            BottomRight = new LatLonCoordinate(55.0, 20.0)
        };

        var result = bbox.ToLatLonBoundingBoxDto();

        result.Should().NotBeNull();
        result!.TopLeft.Latitude.Should().Be(60.0);
        result.TopLeft.Longitude.Should().Be(15.0);
        result.BottomRight.Latitude.Should().Be(55.0);
        result.BottomRight.Longitude.Should().Be(20.0);
    }
}
