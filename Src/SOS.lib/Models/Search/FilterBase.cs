using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Base filter class
    /// </summary>
    public class FilterBase
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
            (Gender?.Any() ?? false) ||
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
        public IEnumerable<int> Gender { get; set; }

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

        /// <summary>
        /// Decides whether field mapped fields, in addition to its Id value, also should return its associated value.
        /// </summary>
        public bool TranslateFieldMappedValues { get; set; } = false;

        /// <summary>
        /// Field mapping translation culture code.
        /// sv-SE (Swedish)
        /// en-GB (English)
        /// </summary>
        public string TranslationCultureCode { get; set; } = "en-GB";

        public SearchFilter Clone()
        {
            var searchFilter = (SearchFilter)MemberwiseClone();
            return searchFilter;
        }
    }
}
