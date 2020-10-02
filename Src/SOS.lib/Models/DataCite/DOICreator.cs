using System.Text.Json.Serialization;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.DataCite
{
    public class DOICreator
    {
        /// <summary>
        /// Name of creator
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Name type of creator
        /// </summary>
        [JsonPropertyName("nameType")]
        public CreatorNameType NameType { get; set; }

        /// <summary>
        /// Given name of creator
        /// </summary>
        [JsonPropertyName("givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Family name of creator
        /// </summary>
        [JsonPropertyName("familyName")]
        public string FamilyName { get; set; }
    }
}
