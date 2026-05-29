namespace SOS.Lib.Gis;

public record ObservationStatusCounts(
    int FoundCount,
    int TreatedCount,
    int DiffusedCount,
    int RedlistedCount,
    int InvasiveInSwedenCount
);
