namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Processing and harvesting status for one data provider.</summary>
public class DataProviderStatusDto
{
    /// <summary>The numeric data provider identifier.</summary>
    public int Id { get; set; }

    /// <summary>The data provider's string identifier.</summary>
    public string? Identifier { get; set; }

    /// <summary>The data provider's name.</summary>
    public string? Name { get; set; }

    /// <summary>The data provider's Swedish name.</summary>
    public string? SwedishName { get; set; }

    /// <summary>The number of public observations in the active instance.</summary>
    public int PublicActive { get; set; }

    /// <summary>The number of public observations in the inactive instance.</summary>
    public int PublicInactive { get; set; }

    /// <summary>The active public observation count minus the inactive count.</summary>
    public int PublicDiff { get; set; }

    /// <summary>The number of protected observations in the active instance.</summary>
    public int ProtectedActive { get; set; }

    /// <summary>The number of protected observations in the inactive instance.</summary>
    public int ProtectedInactive { get; set; }

    /// <summary>The active protected observation count minus the inactive count.</summary>
    public int ProtectedDiff { get; set; }

    /// <summary>The number of observations that failed processing in the active instance.</summary>
    public int InvalidActive { get; set; }

    /// <summary>The number of observations that failed processing in the inactive instance.</summary>
    public int InvalidInactive { get; set; }

    /// <summary>The active failed-observation count minus the inactive count.</summary>
    public int InvalidDiff { get; set; }

    /// <summary>The harvesting duration for the active instance.</summary>
    public TimeSpan HarvestTimeActive { get; set; }

    /// <summary>The harvesting duration for the inactive instance.</summary>
    public TimeSpan HarvestTimeInactive { get; set; }

    /// <summary>The active harvesting duration minus the inactive duration.</summary>
    public TimeSpan HarvestTimeDiff { get; set; }

    /// <summary>The processing duration for the active instance.</summary>
    public TimeSpan ProcessTimeActive { get; set; }

    /// <summary>The processing duration for the inactive instance.</summary>
    public TimeSpan ProcessTimeInactive { get; set; }

    /// <summary>The active processing duration minus the inactive duration.</summary>
    public TimeSpan ProcessTimeDiff { get; set; }

    /// <summary>The harvesting status for the active instance.</summary>
    public string? HarvestStatusActive { get; set; }

    /// <summary>The harvesting status for the inactive instance.</summary>
    public string? HarvestStatusInactive { get; set; }

    /// <summary>The number of public observations processed by the latest incremental run.</summary>
    public int? LatestIncrementalPublicCount { get; set; }

    /// <summary>The number of protected observations processed by the latest incremental run.</summary>
    public int? LatestIncrementalProtectedCount { get; set; }

    /// <summary>The end time of the latest incremental processing run.</summary>
    public DateTime? LatestIncrementalEnd { get; set; }

    /// <summary>The duration of the latest incremental processing run.</summary>
    public TimeSpan? LatestIncrementalTime { get; set; }
}


