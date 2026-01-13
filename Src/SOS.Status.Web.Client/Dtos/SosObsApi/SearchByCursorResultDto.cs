namespace SOS.Status.Web.Client.Dtos.SosObsApi;

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