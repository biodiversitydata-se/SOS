using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Models;

public class PagingParameters
{
    [JsonPropertyName("skip")]
    public int? Skip { get; set; }

    [JsonPropertyName("take")]
    public int? Take { get; set; }
}