using SOS.Api.Contracts.Common.Responses;
using System.Collections.Generic;

namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// Observation counts and localities for a calendar date.
/// </summary>
public class YearMonthDayCountResultDto : YearMonthCountResultDto
{
    /// <summary>
    /// The day of the month.
    /// </summary>
    public int Day { get; set; }

    /// <summary>The identifiers and names of localities with observations on this date.</summary>
    public IEnumerable<IdNameDto<string>> Localities { get; set; }
}
