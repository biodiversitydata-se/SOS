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
    GeoJsonFlat = 2
}