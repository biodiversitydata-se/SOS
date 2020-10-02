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
    }
}
