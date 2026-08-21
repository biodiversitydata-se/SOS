using GeoJSON.Text.Geometry;

namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>
/// A single feature (a location or a cluster of locations) in an adaptive search locations GeoJSON
/// response, strongly typed instead of a raw GeoJSON string.
/// </summary>
public sealed class AdaptiveSearchLocationsFeatureDto
{
    /// <summary>The feature identifier.</summary>
    public string? Id { get; init; }

    /// <summary>
    /// The geometry of the location or cluster. For a single location, this is a Point or Polygon geometry.
    /// For a cluster, this is the centroid point computed from the member locations.
    /// </summary>
    public IGeometryObject? Geometry { get; init; }

    /// <summary>The feature properties.</summary>
    public required AdaptiveSearchLocationFeaturePropertiesDto Properties { get; init; }
}

/// <summary>
/// A strongly-typed representation of an adaptive search locations GeoJSON response, as an alternative to
/// working with the raw <see cref="AdaptiveSearchLocationsResponse.GeoJson"/> string.
/// </summary>
public sealed class AdaptiveSearchLocationsFeatureCollectionDto
{
    /// <summary>The location and cluster features.</summary>
    public List<AdaptiveSearchLocationsFeatureDto> Features { get; init; } = new();

    /// <summary>
    /// The bounding box of the result, formatted as [minLongitude, minLatitude, maxLongitude, maxLatitude],
    /// when present in the response.
    /// </summary>
    public double[]? BoundingBox { get; init; }
}
