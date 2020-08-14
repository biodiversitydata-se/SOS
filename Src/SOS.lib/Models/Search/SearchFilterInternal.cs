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

        public enum SightingDeterminationFilter
        {
            NoFilter,
            NotUnsureDetermination,
            OnlyUnsureDetermination
        }

        public enum SightingUnspontaneousFilter
        {
            NoFilter,
            NotUnspontaneous,
            Unspontaneous
        }

        public enum SightingNotRecoveredFilter
        {
            NoFilter,
            OnlyNotRecovered,
            DontIncludeNotRecovered
        }

        public enum SightingNotPresentFilter
        {
            DontIncludeNotPresent,
            OnlyNotPresent,
            IncludeNotPresent
        }

        public int? ReportedByUserId { get; set; }
        public int? ObservedByUserId { get; set; }
        public List<int> ProjectIds { get; set; }
        public bool IncludeRealCount { get; set; }
        public List<double> BoundingBox { get; set; }
        /// <summary>
        /// Only include hits with media associated
        /// </summary>
        public bool OnlyWithMedia { get; set; }

        public bool OnlyWithNotes { get; set; }

        public bool OnlyWithNotesOfInterest { get; set; }

        public bool OnlyWithBarcode { get; set; }

        public DateTime? ReportedDateFrom { get; set; }
        public DateTime? ReportedDateTo { get; set; }
        public SightingTypeFilter TypeFilter { get; set; } = SightingTypeFilter.DoNotShowMerged;
        public int? MaxAccuracy { get; set; }
        public bool UsePeriodForAllYears { get; set; }
        public List<int> Months { get; set; }

        public List<int> DiscoveryMethodIds { get; set; }

        public List<int> LifeStageIds { get; set; }

        public List<int> ActivityIds { get; set; }

        public bool HasTriggerdValidationRule { get; set; }
        public bool HasTriggerdValidationRuleWithWarning { get; set; }

        public int? Length { get; set; }
        public string LengthOperator { get; set; }
        public int? Weight { get; set; }
        public string WeightOperator { get; set; }

        public int? Quantity { get; set; }
        public string QuantityOperator { get; set; }

        public List<int> ValidationStatusIds { get; set; }
        public List<int> ExcludeValidationStatusIds { get; set; }

        public SightingDeterminationFilter DeterminationFilter { get; set; }

        public SightingUnspontaneousFilter UnspontaneousFilter { get; set; }

        public SightingNotRecoveredFilter NotRecoveredFilter { get; set; }

        public string SpeciesCollectionLabel { get; set; }

        public string PublicCollection { get; set; }

        public string PrivateCollection { get; set; }
        
        public int? SubstrateSpeciesId { get; set; }
        public int? SubstrateId { get; set; }

        public int?  BiotopeId { get; set; }

        public SightingNotPresentFilter NotPresentFilter { get; set; }

        public bool OnlySecondHandInformation { get; set; }

        public List<int> PublishTypeIdsFilter { get; set; }

        public List<int> RegionalSightingStateIdsFilter { get; set; }

        public List<int> SiteIds { get; set; }

        public List<int> SpeciesFactsIds { get; set; }
    }
}