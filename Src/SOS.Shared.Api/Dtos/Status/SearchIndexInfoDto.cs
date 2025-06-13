using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Status
{
    public class SearchIndexInfoDto
    {
        public class AllocationInfo
        {
            public int Percentage { get; set; }
            public string Node { get; set; }
            public string DiskAvailable { get; set; }
            public string DiskUsed { get; set; }
            public string DiskTotal { get; set; }
        }
        public IEnumerable<AllocationInfo> Allocations { get; set; }
    }
}
