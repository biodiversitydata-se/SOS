namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>Observation counts by status within a location.</summary>
/// <param name="FoundCount">The number of found observations.</param>
/// <param name="TreatedCount">The number of treated observations.</param>
/// <param name="DiffusedCount">The number of observations with diffused coordinates.</param>
/// <param name="RedlistedCount">The number of red-listed observations.</param>
/// <param name="InvasiveInSwedenCount">The number of observations of species invasive in Sweden.</param>
public record ObservationStatusCounts(
    int FoundCount,
    int TreatedCount,
    int DiffusedCount,
    int RedlistedCount,
    int InvasiveInSwedenCount);
