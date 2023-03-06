using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Models;

public class PagingParameters
{
    [JsonPropertyName("skip")]    
    [SwaggerParameter("Pagination start index.")]
    public int? Skip { get; set; }

    [JsonPropertyName("take")]
    [SwaggerParameter("Number of items to return. 1000 items is the max to return in one call.")]
    public int? Take { get; set; }
}