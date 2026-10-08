using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Common.Responses;

/// <summary>
/// Result returned by paged query with optional metadata such as polygon location geometries.
/// </summary>
/// <typeparam name="T">The record type.</typeparam>
public class GeoPagedResultDto<T> : PagedResultDto<T>
{
    /// <summary>
    /// Keeps the legacy GeoJSON response shape when records are not included.
    /// </summary>
    public GeoPagedResultDto()
    {
        Records = null!;
    }

    /// <summary>
    /// The records as GeoJSON.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GeoJson { get; set; }

    /// <summary>
    /// Optional metadata associated with the search result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SearchResultMetadataDto? Metadata { get; set; }
}
