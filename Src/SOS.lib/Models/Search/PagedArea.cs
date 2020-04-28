using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Represents a paged area
    /// </summary>
    public class PagedArea
    {
        /// <summary>
        /// Area Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of area
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of area
        /// </summary>
        public string AreaType { get; set; }
    }
}
