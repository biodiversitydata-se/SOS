namespace SOS.Api.Contracts.Areas.Responses;
/// <summary>
/// Result returned by paged query
/// </summary>
public class AreaBaseDtoPagedResult
{

    /// <summary>
    /// Paged records
    /// </summary>
    public ICollection<AreaBaseDto> Records { get; set; }

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

}


