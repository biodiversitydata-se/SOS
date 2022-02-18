using System;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Date related filter
    /// </summary>
    public class CheckListDateFilter : DateFilter
    {
        /// <summary>
        /// Minimum time spent to look for taxa
        /// </summary>
        public TimeSpan MinEffortTime { get; set; }
    }
}