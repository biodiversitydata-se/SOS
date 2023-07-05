using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    ///     Base filter class
    /// </summary>
    public class SearchFilterBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchFilterBase() : this(0)
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="protectionFilter"></param>
        public SearchFilterBase(int userId, ProtectionFilter protectionFilter = ProtectionFilter.Public)
        {
            ExtendedAuthorization = new ExtendedAuthorizationFilter
            {
                UserId = userId,
                ProtectionFilter = protectionFilter
            };
            Location = new LocationFilter();
        }

        public enum SightingTypeFilter
        {
            DoNotShowMerged,
            ShowOnlyMerged,
            ShowBoth,
            DoNotShowSightingsInMerged,
            DoNotShowMergedIncludeReplacementChilds
        }


        public enum StatusVerification
        {
            BothVerifiedAndNotVerified,
            Verified,
            NotVerified
        }

        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public List<int> DataProviderIds { get; set; }

        /// <summary>
        ///     Which type of date filtering that should be used
        /// </summary>
        public DateFilter Date { get; set; }

        /// <summary>
        /// Filter by diffuse status
        /// </summary>
        public List<DiffusionStatus> DiffusionStatuses { get; set; }

        /// <summary>
        /// Exclude filter
        /// </summary>
        public ExcludeFilter ExcludeFilter { get; set; }

        /// <summary>
        /// Filter used to give user extended authorization
        /// </summary>
        public ExtendedAuthorizationFilter ExtendedAuthorization { get; set; }

        /// <summary>
        ///     Vocabulary mapping translation culture code.
        ///     Available values.
        ///     sv-SE (Swedish)
        ///     en-GB (English)
        /// </summary>
        public string FieldTranslationCultureCode { get; set; }

        /// <summary>
        /// License filter
        /// </summary>
        public IEnumerable<string> Licenses { get; set; }

        /// <summary>
        /// Location related filter
        /// </summary>
        public LocationFilter Location { get; set; }

        /// <summary>
        /// Observation modified filter
        /// </summary>
        public ModifiedDateFilter ModifiedDate { get; set; }

        /// <summary>
        /// Filter for observation not recovered
        /// </summary>
        public SightingNotRecoveredFilter NotRecoveredFilter { get; set; }

        /// <summary>
        /// Project id's to match.
        /// </summary>
        public List<int> ProjectIds { get; set; }

        /// <summary>
        ///     True to return only positive sightings, false to return negative sightings, null to return both positive and
        ///     negative sightings.
        ///     An negative observation is an observation that was expected to be found but wasn't.
        /// </summary>
        public bool? PositiveSightings { get; set; }

        /// <summary>
        /// Sensitivity categories to match
        /// </summary>
        public IEnumerable<int> SensitivityCategories { get; set; }

        /// <summary>
        ///     Sex id's to match. Queryable values are available in sex vocabulary.
        /// </summary>
        public List<int> SexIds { get; set; }

        /// <summary>
        ///    Taxon related filter
        /// </summary>
        public TaxonFilter Taxa { get; set; }

        public SightingTypeFilter TypeFilter { get; set; } = SightingTypeFilter.DoNotShowMerged;

        /// <summary>
        ///     True to return only validated sightings.
        /// </summary>
        public StatusVerification VerificationStatus { get; set; }

        /// <summary>
        /// Filter by uncertain determination
        /// </summary>
        public SightingDeterminationFilter DeterminationFilter { get; set; }

        /// <summary>
        /// Limit returned observations based on bird nest activity level.
        /// Only bird observations in Artportalen are affected
        /// by this search criteria.
        /// Observation of other organism groups (not birds) are
        /// not affected by this search criteria. 
        /// </summary>
        public int? BirdNestActivityLimit { get; set; }

        public List<string> DataStewardshipDatasetIds { get; set; }
        public bool? IsPartOfDataStewardshipDataset { get; set; }

        public List<string> EventIds { get; set; }

        ///// <summary>
        /////     Observation start date specified in the ISO 8601 standard.
        ///// </summary>
        //public DateTime? StartDate { get; set; }

        ///// <summary>
        /////     Observation end date specified in the ISO 8601 standard.
        ///// </summary>
        //public DateTime? EndDate { get; set; }

        ///// <summary>
        ///// Predefined time ranges.
        ///// </summary>
        //public List<TimeRange> TimeRanges { get; set; }        

        ///// <summary>
        /////     Which type of date filtering that should be used
        ///// </summary>
        //public DateRangeFilterType DateFilterType { get; set; } = DateRangeFilterType.OverlappingStartDateAndEndDate;

        /// <summary>
        /// Convert filter to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Check if the filter has any taxon filter set.
        /// </summary>
        /// <returns></returns>
        public bool HasTaxonFilter()
        {
            return (Taxa?.Ids?.Any() ?? false)
                || (Taxa?.TaxonCategories?.Any() ?? false)
                || (Taxa?.IncludeUnderlyingTaxa ?? false)
                || (Taxa?.ListIds?.Any() ?? false)
                || (Taxa?.RedListCategories?.Any() ?? false);
        }
    }
}