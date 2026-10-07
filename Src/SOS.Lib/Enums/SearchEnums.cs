namespace SOS.Lib.Enums;

public enum SightingDeterminationFilter
{
    NoFilter,
    NotUnsureDetermination,
    OnlyUnsureDetermination
}

public enum SightingUnspontaneousFilter
{
    NoFilter,
    NotUnspontaneous,
    Unspontaneous
}

public enum SightingNotRecoveredFilter
{
    NoFilter,
    OnlyNotRecovered,
    DontIncludeNotRecovered,
    IncludeNotRecovered
}

public enum SightingNotPresentFilter
{
    NoFilter,
    DontIncludeNotPresent,
    OnlyNotPresent,
    IncludeNotPresent
}

public enum DateFilterComparison
{
    StartDate,
    EndDate,
    BothStartDateAndEndDate,
    StartDateEndDateMonthRange
}

public enum ProtectionFilter
{
    /// <summary>
    /// Public observations
    /// </summary>
    Public = 0,

    /// <summary>
    /// Sensitive observations
    /// </summary>
    Sensitive,

    /// <summary>
    /// Both public and sensitive observations
    /// </summary>
    BothPublicAndSensitive
}
