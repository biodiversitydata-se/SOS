using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    public class EventFilter
    {
        /// <summary>
        /// Event id's
        /// </summary>
        public IEnumerable<string> Ids { get; set; }
    }
}
