using System.Text.Json.Serialization;

namespace SOS.Shared.Api.Dtos;

/// <summary>
///     Result returned by paged query with optional metadata such as polygon location geometries.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GeoPagedResultWithMetadataDto<T> : GeoPagedResultDto<T>
{
    /// <summary>
    ///     Optional metadata associated with the search result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SearchResultMetadataDto? Metadata { get; set; }
}
