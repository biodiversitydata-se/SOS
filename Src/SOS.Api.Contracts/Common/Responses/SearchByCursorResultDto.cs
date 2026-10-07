namespace SOS.Api.Contracts.Common.Responses;
/// <summary>
/// A page of results retrieved with cursor-based pagination.
/// </summary>
public class SearchByCursorResultDto<T>
{
    /// <summary>Cursor to request the next page, when another page is available.</summary>
    public string? NextCursor { get; set; }

    /// <summary>Maximum number of records requested for this page.</summary>
    public int Take { get; set; }

    /// <summary>Total number of matching records.</summary>
    public long TotalCount { get; set; }

    /// <summary>Records returned in this page.</summary>
    public IEnumerable<T>? Records { get; set; }
}


