using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOIIdentifier
    {
        /// <summary>
        /// FIdentifier
        /// </summary>
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Type of identifier
        /// </summary>
        [JsonPropertyName("identifierType")]
        public string IdentifierType { get; set; }
    }
}
