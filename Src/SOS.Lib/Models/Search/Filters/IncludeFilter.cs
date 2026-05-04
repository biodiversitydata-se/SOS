using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    public class IncludeFilter
    {
        /// <summary>
        /// Include observations with listed occurrence id's
        /// </summary>
        public IEnumerable<string> OccurrenceIds { get; set; }
    }
}
