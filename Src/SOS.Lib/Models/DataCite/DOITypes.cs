using System.Text.Json.Serialization;
namespace SOS.Lib.Models.DataCite
{
    public class DOITypes
    {
        /// <summary>
        /// Bibtex
        /// </summary>
        [JsonPropertyName("bibtex")]
        public string Bibtex { get; set; }

        /// <summary>
        /// Citeproc
        /// </summary>
        [JsonPropertyName("citeproc")]
        public string Citeproc { get; set; }

        /// <summary>
        /// Resource type 
        /// </summary>
        [JsonPropertyName("resourceType")]
        public string ResourceType { get; set; }

        /// <summary>
        /// Resource type general
        /// </summary>
        [JsonPropertyName("resourceTypeGeneral")]
        public string ResourceTypeGeneral { get; set; }

        /// <summary>
        /// RIS
        /// </summary>
        [JsonPropertyName("ris")]
        public string Ris { get; set; }

        /// <summary>
        /// SchemaOrg
        /// </summary>
        [JsonPropertyName("schemaOrg")]
        public string SchemaOrg { get; set; }
    }
}
