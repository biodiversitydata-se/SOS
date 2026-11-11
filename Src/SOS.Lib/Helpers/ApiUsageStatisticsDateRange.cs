using System;

namespace SOS.Lib.Helpers;

/// <summary>
/// Calendar days used by the statistics harvest, which stores local midnight in MongoDB.
/// </summary>
internal sealed class ApiUsageStatisticsDateRange
{
    public DateTime? FromDate { get; }
    public DateTime? ToDate { get; }

    public ApiUsageStatisticsDateRange(DateTime? fromDate, DateTime? toDate)
    {
        FromDate = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Unspecified) : null;
        ToDate = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Unspecified) : null;
        if (FromDate > ToDate)
        {
            throw new ArgumentException("From date must be less than or equal to toDate.", nameof(fromDate));
        }
    }

    public static DateTime GetReportDate(DateTime storedDate) => DateTime.SpecifyKind(
        (storedDate.Kind == DateTimeKind.Utc ? storedDate.ToLocalTime() : storedDate).Date,
        DateTimeKind.Unspecified);
}
