using SOS.Lib.Models.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Represents areas
    /// </summary>
    public class InternalAreas
    {
       public IEnumerable<Area> Areas { get; set; }
       public long TotalCount { get; set; }
    }
}
