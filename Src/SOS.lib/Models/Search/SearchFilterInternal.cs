using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Search filter for the internal advanced search
    /// </summary>
    public class SearchFilterInternal : SearchFilter
    {
        public enum SightingTypeFilter
        {
            DoNotShowMerged,
            ShowOnlyMerged,
            ShowBoth,
            DoNotShowSightingsInMerged
        }
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
        public bool IncludeRealCount { get; set; }
        public List<double> BoundingBox { get; set; }
        /// <summary>
        /// Only include hits with media associated
        /// </summary>
        public bool OnlyWithMedia { get; set; }

        public bool OnlyWithNotes { get; set; }

        public bool OnlyWithNotesOfInterest { get; set; }

        public DateTime? ReportedDateFrom { get; set; }
        public DateTime? ReportedDateTo { get; set; }
        public SightingTypeFilter TypeFilter { get; set; } = SightingTypeFilter.DoNotShowMerged;
        public int? MaxAccuracy { get; set; }
        public bool UsePeriodForAllYears { get; set; }
        public List<int> Months { get; set; }

        public List<int> DiscoveryMethodIds { get; set; }
    }
}