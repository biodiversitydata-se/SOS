using System;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Filter for observation Modified date
    /// </summary>
    public class ModifiedDateFilter
    {
        /// <summary>
        /// Changed from
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Changed tp
        /// </summary>
        public DateTime? To { get; set; }
    }
}
