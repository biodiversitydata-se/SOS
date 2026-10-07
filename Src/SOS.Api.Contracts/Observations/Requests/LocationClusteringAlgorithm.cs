namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>The algorithm used to cluster location features in adaptive location search.</summary>
public enum LocationClusteringAlgorithm
{
    /// <summary>Greedy clustering in SWEREF 99 TM using a spatial grid index.</summary>
    Sweref99TmFastGreedy = 1,
    /// <summary>Greedy clustering using SWEREF 99 TM coordinates.</summary>
    Sweref99TmGreedy = 2,
    /// <summary>Density-based clustering using a map-bounded DBSCAN variant in SWEREF 99 TM.</summary>
    DbScan = 3
}
