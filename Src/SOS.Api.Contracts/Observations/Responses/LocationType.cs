namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>
/// Spatial representation used for an observation location.
/// </summary>
public enum LocationType
{
    /// <summary>The spatial representation is unknown.</summary>
    Unknown = 0,
    /// <summary>The location is represented by a point.</summary>
    Point = 1,
    /// <summary>The location is represented by a polygon.</summary>
    Polygon = 2
}
