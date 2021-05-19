using System;
using System.Collections.Generic;
using System.Text.Json;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;

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
        /// OverlappingStartDateAndEndDate, Start or EndDate of the observation must be within the specified interval    
        /// BetweenStartDateAndEndDate, Start and EndDate of the observation must be within the specified interval    
        /// OnlyStartDate, Only StartDate of the observation must be within the specified interval            
        /// OnlyEndDate, Only EndDate of the observation must be within the specified interval    
        /// </summary>
        public enum DateRangeFilterType
        {
            /// <summary>
            /// Start or EndDate of the observation must be within the specified interval
            /// </summary>
            OverlappingStartDateAndEndDate,
            /// <summary>
            /// Start and EndDate of the observation must be within the specified interval
            /// </summary>
            BetweenStartDateAndEndDate,
            /// <summary>
            /// Only StartDate of the observation must be within the specified interval
            /// </summary>
            OnlyStartDate,
            /// <summary>
            /// Only EndDate of the observation must be within the specified interval
            /// </summary>
            OnlyEndDate
        }

        /// <summary>
        /// Pre defined time ranges
        /// </summary>
        public enum TimeRange
        {
            /// <summary>
            /// 04:00-09:00
            /// </summary>
            Morning,
            /// <summary>
            /// 09:00-13:00
            /// </summary>
            Forenoon,
            /// <summary>
            /// 13:00-18:00
            /// </summary>
            Afternoon,
            /// <summary>
            /// 18:00-23:00
            /// </summary>
            Evening,
            /// <summary>
            /// 23:00-04:00
            /// </summary>
            Night
        }

        /// <summary>
        /// Filter on area geometries 
        /// </summary>
        public GeographicAreasFilter AreaGeographic { get; set; }

        /// <summary>
        /// Geographical areas to filter by
        /// </summary>
        public IEnumerable<AreaFilter> Areas { get; set; }

        /// <summary>
        ///     Only get data from these providers
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        ///     Which type of date filtering that should be used
        /// </summary>
        public DateRangeFilterType DateFilterType { get; set; } = DateRangeFilterType.OverlappingStartDateAndEndDate;

        /// <summary>
        /// Filter by diffuse status
        /// </summary>
        public IEnumerable<DiffusionStatus> DiffusionStatuses { get; set; }

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Filter used to give user extended authorization
        /// </summary>
        public IEnumerable<ExtendedAuthorizationFilter> ExtendedAuthorizations { get; set; }

        /// <summary>
        /// Only include ProtectedObservations
        /// </summary>
        public bool ProtectedObservations { get; set; }

        /// <summary>
        ///     Vocabulary mapping translation culture code.
        ///     Available values.
        ///     sv-SE (Swedish)
        ///     en-GB (English)
        /// </summary>
        public string FieldTranslationCultureCode { get; set; }

        /// <summary>
        /// Geometry filter
        /// </summary>
        public GeographicsFilter Geometries { get; set; }

        /// <summary>
        /// Limit observation accuracy. Only observations with accuracy less than this will be returned
        /// </summary>
        public int? MaxAccuracy { get; set; }

        /// <summary>
        /// Filter for observation not recovered
        /// </summary>
        public SightingNotRecoveredFilter NotRecoveredFilter { get; set; }

        /// <summary>
        ///     True to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

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
        ///     Sex id's to match. Queryable values are available in sex vocabulary.
        /// </summary>
        public IEnumerable<int> SexIds { get; set; }

        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///    Taxon related filter
        /// </summary>
        public TaxonFilter Taxa { get; set; }

        /// <summary>
        /// Predefined time ranges.
        /// </summary>
        public IEnumerable<TimeRange> TimeRanges { get; set; }

        public SightingTypeFilter TypeFilter { get; set; } = SightingTypeFilter.DoNotShowMerged;

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