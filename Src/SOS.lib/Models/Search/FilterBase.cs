using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Base filter class
    /// </summary>
    public class FilterBase
    {
        /// <summary>
        /// Convert filter to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public IEnumerable<int> CountyIds { get; set; }

        /// <summary>
        ///     Geometry filter 
        /// </summary>
        public GeometryFilter GeometryFilter { get; set; }

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     True if any filter property is set.
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
            StartDate != null ||
            (TaxonIds?.Any() ?? false) ||
            (AreaIds?.Any() ?? false);

        /// <summary>
        ///     Municipalities to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> MunicipalityIds { get; set; }

        /// <summary>
        ///     True to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        ///     True to return only positive sightings, false to return negative sightings, null to return both positive and
        ///     negative sightings.
        ///     An negative observation is an observation that was expected to be found but wasn't.
        /// </summary>
        public bool? PositiveSightings { get; set; }

        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        ///     Provinces to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> ProvinceIds { get; set; }

        /// <summary>
        ///     Redlist categories to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        ///     Gender to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> GenderIds { get; set; }

        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Taxa to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        ///     Decides whether to search for the exact taxa or
        ///     for the hierarchical underlying taxa.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        ///     Field mapping translation culture code.
        ///     Available values.
        ///     sv-SE (Swedish)
        ///     en-GB (English)
        /// </summary>
        public string FieldTranslationCultureCode { get; set; }

        public IEnumerable<int> AreaIds { get; set; }

        public FilterBase Clone()
        {
            var searchFilter = (FilterBase) MemberwiseClone();
            return searchFilter;
        }
    }
}