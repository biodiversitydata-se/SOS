namespace SOS.Shared.Api.Dtos;

/// <summary>
///     Result returned by search_after query for deep pagination.
///     This enables pagination beyond the 10,000 document limit of standard Elasticsearch pagination.
/// </summary>
/// <typeparam name="T">The type of records returned.</typeparam>
public class SearchByCursorResultDto<T>
{
    /// <summary>
    ///     The cursor value for the next page.
    ///     Pass this value in the next request to get the next page of results.
    ///     Will be null when there are no more results.
    ///     This value is base64-encoded string representation of the sort values.
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    ///     Returns a sequence containing up to the specified number of items.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    ///     Total number of records matching the query.
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    ///     The records for the current page.
    /// </summary>
    public IEnumerable<T>? Records { get; set; }
}
