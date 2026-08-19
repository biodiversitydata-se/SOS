namespace SOS.Api.Contracts.Common.Responses;
/// <summary>
/// DTO for SearchByCursor result
/// </summary>
public class SearchByCursorResultDto<T>
{
    public string? NextCursor { get; set; }
    public int Take { get; set; }
    public long TotalCount { get; set; }
    public IEnumerable<T>? Records { get; set; }
}


