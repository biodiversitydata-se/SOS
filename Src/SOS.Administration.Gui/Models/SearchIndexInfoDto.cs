using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
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
