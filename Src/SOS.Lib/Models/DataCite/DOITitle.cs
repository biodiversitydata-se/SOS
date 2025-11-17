using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite;

public class DOITitle
{
    /// <summary>
    /// DOI title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }
}
