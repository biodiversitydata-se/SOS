namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// Result returned year month aggregation
/// </summary>
public class YearMonthCountResultDto : YearCountResultDto
{
    /// <summary>
    ///     Month
    /// </summary>
    public int Month { get; set; }
}
