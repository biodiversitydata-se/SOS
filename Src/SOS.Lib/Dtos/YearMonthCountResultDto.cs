namespace SOS.Lib.Dtos;

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
