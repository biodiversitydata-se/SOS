namespace SOS.Lib.Gis;

public enum CreatePolygonsMode
{
    /// <summary>
    /// Never create polygons, only points.
    /// </summary>
    No = 0,

    /// <summary>
    /// If the location is of polygon type then create polygon.
    /// </summary>
    Yes = 1,

    /// <summary>
    /// If the location is of polygon type, create polygon if the polygon is larger than the rendered point size
    /// at the current zoom level, otherwise render as point.
    /// </summary>
    WhenNeeded = 2
}
