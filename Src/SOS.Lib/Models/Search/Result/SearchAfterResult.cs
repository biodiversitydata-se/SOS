using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class SearchAfterResult<TRecord, TSearchAfter>
{
    /// <summary>
    /// Point in time id
    /// </summary>
    public string PointInTimeId { get; set; }

    /// <summary>
    /// Returned records
    /// </summary>
    public IEnumerable<TRecord> Records { get; set; }

    /// <summary>
    /// Search after objects
    /// </summary>
    public TSearchAfter SearchAfter { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }
}
