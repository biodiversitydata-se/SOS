using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace SOS.Shared.Api.Dtos;

/// <summary>
/// Output format
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OutputFormatDto
{
    /// <summary>
    /// JSON format (default)
    /// </summary>
    Json = 0,

    /// <summary>
    /// GeoJSON (hierarchical)
    /// </summary>
    GeoJson = 1,

    /// <summary>
    /// GeoJSON (flat)
    /// </summary>
    GeoJsonFlat = 2,

    /// <summary>
    /// GeoJSON grouped by observation locations (polygons will be used for locations of type Polygon)
    /// </summary>
    GeoJsonByLocation = 3,

    /// <summary>
    /// GeoJSON grouped by observation locations (polygons will be used for locations of type Polygon). Flat Observation structure.
    /// </summary>
    GeoJsonByLocationFlat = 4,

    /// <summary>
    /// JSON grouped by observation locations (polygons will be used for locations of type Polygon)
    /// </summary>
    JsonByLocation = 5
}