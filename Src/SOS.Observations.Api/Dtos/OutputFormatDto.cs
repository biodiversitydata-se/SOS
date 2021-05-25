using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace SOS.Observations.Api.Dtos
{
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
}