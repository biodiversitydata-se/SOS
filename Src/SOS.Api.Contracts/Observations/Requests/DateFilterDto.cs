using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Date filter.
/// </summary>
public class DateFilterDto
{

    /// <summary>
    /// Observation start date specified in the ISO 8601 standard, including its UTC offset.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Observation end date specified in the ISO 8601 standard, including its UTC offset.
    /// </summary>
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


