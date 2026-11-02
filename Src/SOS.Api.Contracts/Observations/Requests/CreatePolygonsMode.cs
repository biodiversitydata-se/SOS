namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Controls whether location features are represented as points or polygons.</summary>
public enum CreatePolygonsMode
{
    /// <summary>Return polygon locations as centroid points.</summary>
    No = 0,

    /// <summary>Return polygon geometry for unclustered locations, including small polygons.</summary>
    Yes = 1,

    /// <summary>
    /// Return polygons large enough to display at the current map resolution; otherwise return centroid points.
    /// Requires metersPerPixel.
    /// </summary>
    WhenNeeded = 2
}


