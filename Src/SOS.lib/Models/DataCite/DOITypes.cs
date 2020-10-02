using System.Text.Json.Serialization;
namespace SOS.Lib.Models.DataCite
{
    public class DOITypes
    {
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
    }
}
