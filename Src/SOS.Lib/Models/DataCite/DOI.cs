using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOI<T>
    {
        /// <summary>
        /// Data property
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("meta")]
        public DOIResponseMeta Meta { get; set; }
    }
}
