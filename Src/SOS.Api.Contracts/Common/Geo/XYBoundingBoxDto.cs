namespace SOS.Api.Contracts.Common.Geo;
/// <summary>A bounding box in a projected coordinate system.</summary>
public class XYBoundingBoxDto
{

    /// <summary>The bottom-right corner in the projected coordinate system.</summary>
    public XYCoordinateDto BottomRight { get; set; }

    /// <summary>The top-left corner in the projected coordinate system.</summary>
    public XYCoordinateDto TopLeft { get; set; }

}


