using SOS.Lib.Models.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Represents a paged area
    /// </summary>
    public class PagedAreas
    {
       public IEnumerable<PagedArea> Areas { get; set; }
       public long TotalCount { get; set; }
    }
}
