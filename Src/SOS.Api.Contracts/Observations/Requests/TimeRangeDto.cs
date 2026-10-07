using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Predefined time-of-day intervals used to filter observations.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TimeRangeDto>))]
public enum TimeRangeDto
{
    /// <summary>04:00-09:00.</summary>
    Morning = 0,
    /// <summary>09:00-13:00.</summary>
    Forenoon = 1,
    /// <summary>13:00-18:00.</summary>
    Afternoon = 2,
    /// <summary>18:00-23:00.</summary>
    Evening = 3,
    /// <summary>23:00-04:00.</summary>
    Night = 4,
}


