namespace SOS.Lib.Models.Search;

/// <summary>
/// Technical options that control how an observation search is executed.
/// </summary>
public sealed record SearchExecutionOptions
{
    /// <summary>If true, all observation fields are retrieved.</summary>
    public bool GetAllFields { get; init; }

    /// <summary>A previously obtained total count that can be reused instead of counting again.</summary>
    public long? KnownTotalCount { get; init; }

    /// <summary>If false, the total-count cache is bypassed for this search.</summary>
    public bool UseSearchTotalCountCache { get; init; } = true;
}
