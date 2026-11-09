using SOS.Api.Contracts.Areas.Responses;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>A lightweight reference to a geographic search filter, without its coordinates.</summary>
public sealed class FilterAreaReferenceDto
{
    /// <summary>FilterArea.Area, FilterArea.OwnGeometries (all original request geometries), or FilterArea.Polygon (the first request point with a search radius).</summary>
    public required string FeatureType { get; init; }

    /// <summary>The area type argument for the area geometry endpoint, or null for request geometries.</summary>
    public AreaTypeDto? AreaType { get; init; }

    /// <summary>The featureId argument for the area geometry endpoint, or null for request geometries.</summary>
    public string? FeatureId { get; init; }

    /// <summary>Geometry GET URL relative to the API origin, including its PathBase. Null for request geometries.</summary>
    public string? GeometryUrl { get; init; }

    /// <summary>The original maxDistanceFromPoint argument when this reference represents a search circle.</summary>
    public double? MaxDistanceFromPoint { get; init; }
}
