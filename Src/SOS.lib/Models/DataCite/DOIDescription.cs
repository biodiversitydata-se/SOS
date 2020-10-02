using System.Text.Json.Serialization;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.DataCite
{
    public class DOIDescription
    {
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Type of description
        /// </summary>
        [JsonPropertyName("descriptionType")]
        public DescriptionType DescriptionType { get; set; }
    }
}
