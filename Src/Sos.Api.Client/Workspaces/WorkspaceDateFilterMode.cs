namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Predefined date ranges available to a workspace search.
/// </summary>
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
    /// <summary>Use the current and four preceding Swedish calendar years.</summary>
    LastFiveYears = 4,
    /// <summary>Use the current and twenty-four preceding Swedish calendar years.</summary>
    LastTwentyFiveYears = 5
}
