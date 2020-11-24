using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MongoDB.Bson;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Base filter class
    /// </summary>
    public class FilterBase
    {
        public enum SightingTypeFilter
        {
            DoNotShowMerged,
            ShowOnlyMerged,
            ShowBoth,
            DoNotShowSightingsInMerged
        }

        /// <summary>
        /// Geographical areas to filter by
        /// </summary>
        public IEnumerable<AreaFilter> Areas { get; set; }

        public IEnumerable<string> CountyIds { get; set; }

        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     Field mapping translation culture code.
        ///     Available values.
        ///     sv-SE (Swedish)
        ///     en-GB (English)
        /// </summary>
        public string FieldTranslationCultureCode { get; set; }

        /// <summary>
        ///     Geometry filter 
        /// </summary>
        public GeometryFilter GeometryFilter { get; set; }

        /// <summary>
        ///     Gender to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> GenderIds { get; set; }


        /// <summary>
        ///     Decides whether to search for the exact taxa or
        ///     for the hierarchical underlying taxa.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        ///     True if any filter property is set.
        /// </summary>
        public bool IsFilterActive =>
            (Areas?.Any() ?? false) ||
            (CountyIds?.Any() ?? false) ||
            (DataProviderIds?.Any() ?? false) ||
            EndDate != null ||
            (GeometryFilter?.IsValid ?? false) ||
            (GenderIds?.Any() ?? false) ||
            (MunicipalityIds?.Any() ?? false) ||
            OnlyValidated.HasValue ||
            (ParishIds?.Any() ?? false) || 
            PositiveSightings.HasValue ||
            (ProvinceIds?.Any() ?? false) ||
            (RedListCategories?.Any() ?? false) ||
            StartDate != null ||
            (TaxonIds?.Any() ?? false);

        /// <summary>
        ///     Municipalities to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> MunicipalityIds { get; set; }

        /// <summary>
        ///     True to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        ///     Parish to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> ParishIds { get; set; }

        /// <summary>
        ///     True to return only positive sightings, false to return negative sightings, null to return both positive and
        ///     negative sightings.
        ///     An negative observation is an observation that was expected to be found but wasn't.
        /// </summary>
        public bool? PositiveSightings { get; set; }

        /// <summary>
        ///     Provinces to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> ProvinceIds { get; set; }

        /// <summary>
        ///     Redlist categories to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        ///     If true the whole event timespan must be between StartDate and EndDate
        /// </summary>
        public bool SearchOnlyBetweenDates { get; set; }

        public SightingTypeFilter TypeFilter { get; set; } = SightingTypeFilter.DoNotShowMerged;

        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Taxa to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        public FilterBase Clone()
        {
            var searchFilter = (FilterBase) MemberwiseClone();
            return searchFilter;
        }

        /// <summary>
        /// Convert filter to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}