using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOISubject
    {
        /// <summary>
        /// DOI title
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Scheme URI
        /// </summary>
        [JsonPropertyName("schemeUri")]
        public string SchemeUri { get; set; }

        /// <summary>
        /// Subject scheme
        /// </summary>
        [JsonPropertyName("subjectScheme")]
        public string SubjectScheme { get; set; }


    }
}
