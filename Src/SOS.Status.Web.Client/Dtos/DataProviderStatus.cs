namespace SOS.Status.Web.Client.Dtos;

public class DataProviderStatusDto
{
    public int Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string SwedishName { get; set; }
    public int PublicActive { get; set; }
    public int PublicInactive { get; set; }
    public int PublicDiff { get; set; }
    public int ProtectedActive { get; set; }
    public int ProtectedInactive { get; set; }
    public int ProtectedDiff { get; set; }
    public int InvalidActive { get; set; }
    public int InvalidInactive { get; set; }
    public int InvalidDiff { get; set; }
    public TimeSpan HarvestTimeActive { get; set; }
    public TimeSpan HarvestTimeInactive { get; set; }
    public TimeSpan HarvestTimeDiff { get; set; }
    public TimeSpan ProcessTimeActive { get; set; }
    public TimeSpan ProcessTimeInactive { get; set; }
    public TimeSpan ProcessTimeDiff { get; set; }
    public string HarvestStatusActive { get; set; }
    public string HarvestStatusInactive { get; set; }
    public int? LatestIncrementalPublicCount { get; set; }
    public int? LatestIncrementalProtectedCount { get; set; }
    public DateTime? LatestIncrementalEnd { get; set; }
    public TimeSpan? LatestIncrementalTime { get; set; }
    public int ProcessedPerSecondActive =>
        ProcessTimeActive.TotalSeconds > 0
            ? (int)((PublicActive + ProtectedActive) / ProcessTimeActive.TotalSeconds)
            : 0;
    public int ProcessedPerSecondInactive =>
        ProcessTimeInactive.TotalSeconds > 0
            ? (int)((PublicInactive + ProtectedInactive) / ProcessTimeInactive.TotalSeconds)
            : 0;
    public int ProcessedPerSecondDiff =>
        ProcessedPerSecondActive - ProcessedPerSecondInactive;
    public string PublicActiveWithDiff
    {
        get
        {
            string FormatDiff(int value)
            {
                if (Math.Abs(value) >= 1_000_000)
                    return $"{value / 1_000_000.0:#.#}M"; // 1.2M
                if (Math.Abs(value) >= 1_000)
                    return $"{value / 1_000.0:#.#}k";   // 1.2k
                return value.ToString("N0");
            }

            string current = PublicActive.ToString("N0");

            if (PublicDiff == 0)
                return current;

            string diffSign = PublicDiff > 0 ? "+" : "";
            string diff = $"{diffSign}{FormatDiff(PublicDiff)}";

            return $"{current} ({diff})";
        }
    }
}
