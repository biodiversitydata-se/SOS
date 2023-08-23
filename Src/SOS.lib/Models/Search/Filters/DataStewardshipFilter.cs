using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Filters
{
    public class DataStewardshipFilter
    {
        /// <summary>
        /// Dataset filter
        /// </summary>
        public IEnumerable<string>? DatasetIdentifiers { get; set; }
    }
}
