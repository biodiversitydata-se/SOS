using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOIResponseMeta
    {
        /// <summary>
        /// Total number of hits
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
