using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Contracts.Models;

/// <summary>
/// Paging parameters
/// </summary>
public class PagingParameters
{
    /// <summary>
    /// Pagination start index.
    /// </summary>
    [JsonPropertyName("skip")]
    [SwaggerParameter("Pagination start index.")]    
    public int? Skip { get; set; }

    /// <summary>
    /// Number of items to return. 1000 items is the max to return in one call.
    /// </summary>
    [JsonPropertyName("take")]
    [SwaggerParameter("Number of items to return. 1000 items is the max to return in one call.")]
    public int? Take { get; set; }
}