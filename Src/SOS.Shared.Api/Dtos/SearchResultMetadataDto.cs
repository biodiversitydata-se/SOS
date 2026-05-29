using NetTopologySuite.Geometries;

namespace SOS.Shared.Api.Dtos;

/// <summary>
///     Metadata associated with a search result.
/// </summary>
public class SearchResultMetadataDto
{
    /// <summary>
    ///     Deduplicated polygon location geometries keyed by LocationId.
    ///     Only contains locations where Location.Type is Polygon.
    /// </summary>
    public Dictionary<string, Geometry>? PolygonLocations { get; set; }
}
