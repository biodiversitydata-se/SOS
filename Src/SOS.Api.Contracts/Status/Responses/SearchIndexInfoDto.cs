namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Disk allocation information for the SOS search index.</summary>
public class SearchIndexInfoDto
{
    /// <summary>Disk allocation for one search-index node.</summary>
    public class AllocationInfo
    {
        /// <summary>The disk-availability value reported for this allocation.</summary>
        public int Percentage { get; set; }

        /// <summary>The node or storage path associated with this allocation.</summary>
        public string? Node { get; set; }

        /// <summary>The available disk space reported for this allocation.</summary>
        public string? DiskAvailable { get; set; }

        /// <summary>The used disk space reported for this allocation.</summary>
        public string? DiskUsed { get; set; }

        /// <summary>The total disk space reported for this allocation.</summary>
        public string? DiskTotal { get; set; }
    }

    /// <summary>The disk allocations reported by the search-index nodes.</summary>
    public IEnumerable<AllocationInfo>? Allocations { get; set; }
}


