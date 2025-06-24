namespace SOS.Shared.Api.Dtos.Status;
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
}
