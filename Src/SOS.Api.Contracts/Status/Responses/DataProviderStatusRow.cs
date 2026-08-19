namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Flattened observation counts for one data provider.</summary>
public sealed class DataProviderStatusRow
{
    public string Datasource { get; init; } = string.Empty;

    public int DataProviderId { get; init; }

    public int PublicActive { get; init; }

    public int PublicInactive { get; init; }

    public int PublicDiff { get; init; }

    public int ProtectedActive { get; init; }

    public int ProtectedInactive { get; init; }

    public int ProtectedDiff { get; init; }

    public int InvalidActive { get; init; }

    public int InvalidInactive { get; init; }

    public int InvalidDiff { get; init; }
}


