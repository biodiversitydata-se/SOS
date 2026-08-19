using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Common.Responses;
/// <summary>
///     Metadata associated with a search result.
/// </summary>
public class SearchResultMetadataDto
{
    /// <summary>
    ///     Deduplicated polygon location geometries keyed by LocationId.
    ///     Only contains locations where Location.Type is Polygon.
    /// </summary>
    public Dictionary<string, GeoJSON.Text.Geometry.IGeometryObject>? PolygonLocations { get; set; }
}


