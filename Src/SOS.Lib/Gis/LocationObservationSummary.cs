namespace SOS.Lib.Gis;

public class LocationObservationSummary
{
    public int ObservationCount { get; set; }

    public int FoundCount { get; set; }

    public int TreatedCount { get; set; }

    public int DiffusedCount { get; set; }

    public int RedlistedCount { get; set; }

    public int InvasiveInSwedenCount { get; set; }

    public int NotRecoveredCount { get; set; }

    public int NeverFoundCount { get; set; }

    public bool IsAllObservationsSensitive { get; set; }

    public bool AllObservationsArePresent { get; set; }

    public int? MinPresentEndYear { get; set; }

    public int? MaxPresentEndYear { get; set; }

    public int? MinNotRecoveredEndYear { get; set; }

    public int? MaxNotRecoveredEndYear { get; set; }

    public List<DebugObservation>? DebugObservations { get; set; }

    public List<PresentationStatusSummary> PresentationStatusSummaries { get; set; } = new();

    public ObservationStatusCounts ToStatusCounts()
    {
        return new ObservationStatusCounts(
            FoundCount,
            TreatedCount,
            DiffusedCount,
            RedlistedCount,
            InvasiveInSwedenCount);
    }

    public class PresentationStatusSummary
    {
        public bool IsPresent { get; set; }

        public bool IsSensitive { get; set; }

        public bool IsTreated { get; set; }

        public bool IsNotRecovered { get; set; }

        public int? EndYear { get; set; }

        public int Count { get; set; }
    }
}