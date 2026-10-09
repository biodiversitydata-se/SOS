using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Date filter.
/// </summary>
public class DateFilterDto
{

    /// <summary>
    /// Observation start date/time specified in ISO 8601 format.
    /// <br/>An explicit UTC offset (including Z) is respected. Values without an offset are interpreted
    /// as Swedish time (Europe/Stockholm), with +01:00 or +02:00 depending on the date.
    /// A date-only value such as 2026-01-15 is interpreted as 00:00:00 in Swedish time.
    /// </summary>
    /// <example>2026-01-15T11:00:00+01:00</example>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Observation end date/time specified in ISO 8601 format.
    /// <br/>An explicit UTC offset (including Z) is respected. Values without an offset are interpreted
    /// as Swedish time (Europe/Stockholm), with +01:00 or +02:00 depending on the date.
    /// A date-only value such as 2026-07-15 includes the day through 23:59:59 in Swedish time.
    /// An explicit time is preserved, including midnight; it is not extended to the end of the day.
    /// </summary>
    /// <example>2026-07-15T12:00:00+02:00</example>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableEndDateTimeOffsetConverter))]
    public System.DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// The matching rule applied to the observation's start and end dates.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterTypeDto? DateFilterType { get; set; }

    /// <summary>
    /// Predefined time-of-day ranges included in the search.
    /// </summary>
    public ICollection<TimeRangeDto>? TimeRanges { get; set; }

}


