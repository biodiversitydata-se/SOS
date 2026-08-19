namespace SOS.Api.Contracts.Aggregations.Requests;

/// <summary>
/// Time series aggregation type.
/// </summary>
public enum TimeSeriesTypeDto
{
    /// <summary>
    /// Year.
    /// </summary>
    Year = 0,

    /// <summary>
    /// Month.
    /// </summary>
    Month = 1,

    /// <summary>
    /// Week 48.
    /// </summary>
    Week48 = 2,

    /// <summary>
    /// Day.
    /// </summary>
    Day = 4
}
