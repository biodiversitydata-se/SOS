namespace SOS.Api.Contracts.Common.Geo;
/// <summary>A geographic bounding box in latitude and longitude.</summary>
public class LatLonBoundingBoxDto
{
    /// <summary>The southeast corner of the bounding box in latitude and longitude.</summary>
    public LatLonCoordinateDto BottomRight { get; set; } = null!;

    /// <summary>The northwest corner of the bounding box in latitude and longitude.</summary>
    public LatLonCoordinateDto TopLeft { get; set; } = null!;

    /// <summary>Creates a bounding box, using the world extent when any bound is missing.</summary>
    public static LatLonBoundingBoxDto Create(double? left, double? top, double? right, double? bottom)
    {
        return left.HasValue && top.HasValue && right.HasValue && bottom.HasValue
            ? new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(top.Value, left.Value),
                BottomRight = new LatLonCoordinateDto(bottom.Value, right.Value)
            }
            : new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(90, -180),
                BottomRight = new LatLonCoordinateDto(-90, 180)
            };
    }
}


