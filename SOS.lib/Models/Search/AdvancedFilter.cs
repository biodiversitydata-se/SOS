using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Search filter for advanced search
    /// </summary>
    public class AdvancedFilter
    {
        /// <summary>
        /// Counties to match
        /// </summary>
        public IEnumerable<string> Counties { get; set; }

        /// <summary>
        /// Sighting last date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// True if any filter property is set
        /// </summary>
        public bool IsFilterActive => 
            (Counties?.Any() ?? false) ||
            EndDate != null ||
            (Municipalities?.Any() ?? false) ||
            (Parishes?.Any() ?? false) ||
            (Provinces?.Any() ?? false) ||
            (Regions?.Any() ?? false) ||
            (Sex?.Any() ?? false) ||
            (StartDate != null) ||
            (TaxonIds?.Any() ?? false);

        /// <summary>
        /// Municipalities to match
        /// </summary>
        public IEnumerable<string> Municipalities { get; set; }

        /// <summary>
        /// Parishes to match
        /// </summary>
        public IEnumerable<string> Parishes { get; set; }

        /// <summary>
        /// Provinces to match
        /// </summary>
        public IEnumerable<string> Provinces { get; set; }

        /// <summary>
        /// Regions to match
        /// </summary>
        public IEnumerable<string> Regions { get; set; }

        /// <summary>
        /// Gender to match
        /// </summary>
        public IEnumerable<string> Sex { get; set; }

        /// <summary>
        /// Sighting first date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Taxon id's to match
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        /// Fields to return (empty = all)
        /// </summary>
        public IEnumerable<string> OutputFields { get; set; }
    }
}
