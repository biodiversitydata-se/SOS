namespace SOS.Api.Contracts.Common.Responses;
public class GeoPagedResult<T> : PagedResult<T>
{
    public string? GeoJson { get; init; }
    public SearchResultMetadataDto? Metadata { get; init; }
}


