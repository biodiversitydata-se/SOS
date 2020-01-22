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
        public IEnumerable<int> Counties { get; set; }


        public GeometryFilter Delimitation { get; set; }

        /// <summary>
        /// Sighting last date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// True if any filter property is set
        /// </summary>
        public bool IsFilterActive =>
            (Counties?.Any() ?? false) ||
            (Delimitation?.IsValid ?? false) ||
            EndDate != null ||
            (Municipalities?.Any() ?? false) ||
            OnlyValidated.HasValue ||
            PositiveSightings.HasValue ||
            (Provinces?.Any() ?? false) ||
            (RedListCategories?.Any() ?? false) ||
            (Sex?.Any() ?? false) ||
            (StartDate != null) ||
            (TaxonIds?.Any() ?? false);

        /// <summary>
        /// Municipalities to match
        /// </summary>
        public IEnumerable<int> Municipalities { get; set; }

        /// <summary>
        /// True to return only validated sightings
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        /// Fields to return (empty = all)
        /// </summary>
        public IEnumerable<string> OutputFields { get; set; }

        /// <summary>
        /// True to return only positive sightings, false to return negative sightings, null to return both positive and negative sightings
        /// </summary>
        public bool? PositiveSightings { get; set; }

        /// <summary>
        /// Provinces to match
        /// </summary>
        public IEnumerable<int> Provinces { get; set; }

        /// <summary>
        /// Redlist categories to match
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        /// Gender to match
        /// </summary>
        public IEnumerable<int> Sex { get; set; }

        /// <summary>
        /// Sighting first date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Taxon id's to match
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        /// Decides whether to search for exact taxonIds or
        /// for the hierarchical underlying taxa.
        /// </summary>
        public bool SearchUnderlyingTaxa { get; set; }
    }
}
