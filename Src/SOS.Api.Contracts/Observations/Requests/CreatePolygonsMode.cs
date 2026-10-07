namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Controls whether location features are represented as points or polygons.</summary>
public enum CreatePolygonsMode
{
    /// <summary>Never create polygons; return points.</summary>
    No = 0,

    /// <summary>Create a polygon when the location has polygon geometry.</summary>
    Yes = 1,

    /// <summary>Create a polygon only when it is larger than the rendered point at the current zoom level.</summary>
    WhenNeeded = 2
}


