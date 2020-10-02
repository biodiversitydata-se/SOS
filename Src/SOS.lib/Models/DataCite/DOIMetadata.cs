
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOIMetadata
    {
        /// <summary>
        /// DOI attributes
        /// </summary>
        [JsonPropertyName("attributes")]
        public DOIAttributes Attributes { get; set; }

        /// <summary>
        /// DOI Id property
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// DOI type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
