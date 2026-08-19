namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Disk allocation information for the SOS search index.</summary>
public class SearchIndexInfoDto
{
    /// <summary>Disk allocation for one search-index node.</summary>
    public class AllocationInfo
    {
        public int Percentage { get; set; }

        public string? Node { get; set; }

        public string? DiskAvailable { get; set; }

        public string? DiskUsed { get; set; }

        public string? DiskTotal { get; set; }
    }

    public IEnumerable<AllocationInfo>? Allocations { get; set; }
}


