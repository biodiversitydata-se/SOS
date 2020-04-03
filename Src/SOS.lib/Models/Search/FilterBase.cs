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
        public IEnumerable<int> CountyIds { get; set; }


        public GeometryFilter GeometryFilter { get; set; }

        /// <summary>
        /// Sighting last date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// True if any filter property is set
        /// </summary>
        public bool IsFilterActive =>
            (CountyIds?.Any() ?? false) ||
            (GeometryFilter?.IsValid ?? false) ||
            EndDate != null ||
            (MunicipalityIds?.Any() ?? false) ||
            OnlyValidated.HasValue ||
            PositiveSightings.HasValue ||
            (ProvinceIds?.Any() ?? false) ||
            (RedListCategories?.Any() ?? false) ||
            (GenderIds?.Any() ?? false) ||
            (StartDate != null) ||
            (TaxonIds?.Any() ?? false);

        /// <summary>
        /// Municipalities to match
        /// </summary>
        public IEnumerable<int> MunicipalityIds { get; set; }

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
        public IEnumerable<int> ProvinceIds { get; set; }

        /// <summary>
        /// Redlist categories to match
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        /// Gender to match
        /// </summary>
        public IEnumerable<int> GenderIds { get; set; }

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
        public bool IncludeUnderlyingTaxa { get; set; }

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

        public FilterBase Clone()
        {
            var searchFilter = (FilterBase)MemberwiseClone();
            return searchFilter;
        }
    }
}