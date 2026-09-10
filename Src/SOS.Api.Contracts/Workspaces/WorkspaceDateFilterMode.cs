using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Workspaces;

/// <summary>
/// Predefined date ranges available to a workspace search.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WorkspaceDateFilterMode>))]
public enum WorkspaceDateFilterMode
{
    /// <summary>Do not apply a date filter.</summary>
    AllYears = 0,
    /// <summary>Use the current Swedish calendar year.</summary>
    CurrentYear = 1,
    /// <summary>Use the configured start and end dates.</summary>
    Custom = 2,
    /// <summary>Use the previous Swedish calendar year.</summary>
    PreviousYear = 3,
    /// <summary>Use the current and five preceding Swedish calendar years (the existing six-year interval).</summary>
    LastFiveYears = 4,
    /// <summary>Use the current and twenty-five preceding Swedish calendar years (the existing twenty-six-year interval).</summary>
    LastTwentyFiveYears = 5,
    /// <summary>Use exactly five Swedish calendar years: the current year and four preceding years.</summary>
    LastFiveCalendarYears = 6,
    /// <summary>Use exactly twenty-five Swedish calendar years: the current year and twenty-four preceding years.</summary>
    LastTwentyFiveCalendarYears = 7
}
