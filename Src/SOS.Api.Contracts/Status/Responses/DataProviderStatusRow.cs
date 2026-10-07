namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Flattened observation counts for one data provider.</summary>
public sealed class DataProviderStatusRow
{
    /// <summary>The data provider's string identifier.</summary>
    public string Datasource { get; init; } = string.Empty;

    /// <summary>The numeric data provider identifier.</summary>
    public int DataProviderId { get; init; }

    /// <summary>The number of public observations in the active instance.</summary>
    public int PublicActive { get; init; }

    /// <summary>The number of public observations in the inactive instance.</summary>
    public int PublicInactive { get; init; }

    /// <summary>The active public observation count minus the inactive count.</summary>
    public int PublicDiff { get; init; }

    /// <summary>The number of protected observations in the active instance.</summary>
    public int ProtectedActive { get; init; }

    /// <summary>The number of protected observations in the inactive instance.</summary>
    public int ProtectedInactive { get; init; }

    /// <summary>The active protected observation count minus the inactive count.</summary>
    public int ProtectedDiff { get; init; }

    /// <summary>The number of observations that failed processing in the active instance.</summary>
    public int InvalidActive { get; init; }

    /// <summary>The number of observations that failed processing in the inactive instance.</summary>
    public int InvalidInactive { get; init; }

    /// <summary>The active failed-observation count minus the inactive count.</summary>
    public int InvalidDiff { get; init; }
}


