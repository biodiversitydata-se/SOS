using System;
using System.Collections.Generic;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    ///     Search filter for the internal advanced search
    /// </summary>
    public class SearchFilterInternal : SearchFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sensitiveObservations"></param>
        public SearchFilterInternal(int userId, bool sensitiveObservations = false) : base(userId, sensitiveObservations)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchFilter"></param>
        public SearchFilterInternal(SearchFilter searchFilter) : base(searchFilter?.ExtendedAuthorization?.UserId ?? 0, searchFilter?.ExtendedAuthorization?.ProtectedObservations ?? false)
        {
            if (searchFilter == null)
            {
                return;
            }

            BirdNestActivityLimit = searchFilter.BirdNestActivityLimit;
            Date = searchFilter.Date;
            DataProviderIds = searchFilter.DataProviderIds;
            DeterminationFilter = searchFilter.DeterminationFilter;
            DiffusionStatuses = searchFilter.DiffusionStatuses;
            ExtendedAuthorization = searchFilter.ExtendedAuthorization;
            FieldTranslationCultureCode = searchFilter.FieldTranslationCultureCode;
            Location = searchFilter.Location;
            NotRecoveredFilter = searchFilter.NotRecoveredFilter;
            OutputFields = searchFilter.OutputFields;
            PositiveSightings = searchFilter.PositiveSightings;
            ProjectIds = searchFilter.ProjectIds;
            Taxa = searchFilter.Taxa;
            VerificationStatus = searchFilter.VerificationStatus;
        }

        /// <summary>
        /// Check list Id
        /// </summary>
        public int? ChecklistId { get; set; }

        /// <summary>
        /// Reported by Artportalen user id.
        /// </summary>
        public int? ReportedByUserId { get; set; }

        /// <summary>
        /// Observed by Artportalen user id.
        /// </summary>
        public int? ObservedByUserId { get; set; }

        /// <summary>
        /// Reported by user service user id.
        /// </summary>
        public int? ReportedByUserServiceUserId { get; set; }

        /// <summary>
        /// Observed by user service user id.
        /// </summary>
        public int? ObservedByUserServiceUserId { get; set; }

        public bool IncludeRealCount { get; set; }

        /// <summary>
        /// Only include hits with media associated
        /// </summary>
        public bool OnlyWithMedia { get; set; }

        public bool OnlyWithNotes { get; set; }

        public bool OnlyWithNotesOfInterest { get; set; }
        public bool OnlyWithUserComments { get; set; }


        public bool OnlyWithBarcode { get; set; }

        public DateTime? ReportedDateFrom { get; set; }
        public DateTime? ReportedDateTo { get; set; }

        public bool UsePeriodForAllYears { get; set; }
        public IEnumerable<int> Months { get; set; }
        public DateFilterComparison MonthsComparison { get; set; } = DateFilterComparison.StartDate;
        public IEnumerable<int> DiscoveryMethodIds { get; set; }

        public IEnumerable<int> LifeStageIds { get; set; }

        public IEnumerable<int> ActivityIds { get; set; }

        public bool HasTriggeredVerificationRule { get; set; }
        public bool HasTriggeredVerificationRuleWithWarning { get; set; }

        public int? Length { get; set; }
        public string LengthOperator { get; set; }
        public int? Weight { get; set; }
        public string WeightOperator { get; set; }

        public int? Quantity { get; set; }
        public string QuantityOperator { get; set; }

        public IEnumerable<int> VerificationStatusIds { get; set; }
        public IEnumerable<int> ExcludeVerificationStatusIds { get; set; }

        public SightingUnspontaneousFilter UnspontaneousFilter { get; set; }

        public string SpeciesCollectionLabel { get; set; }

        public string PublicCollection { get; set; }

        public string PrivateCollection { get; set; }

        public int? SubstrateSpeciesId { get; set; }
        public int? SubstrateId { get; set; }

        public int? BiotopeId { get; set; }

        public SightingNotPresentFilter NotPresentFilter { get; set; }

        public bool OnlySecondHandInformation { get; set; }

        public IEnumerable<int> PublishTypeIdsFilter { get; set; }

        public IEnumerable<int> RegionalSightingStateIdsFilter { get; set; }

        public IEnumerable<int> TriggeredObservationRuleFrequencyIds { get; set; }

        public IEnumerable<int> TriggeredObservationRuleReproductionIds { get; set; }

        public IEnumerable<int> SiteIds { get; set; }

        public IEnumerable<int> SiteProjectIds { get; set; }

        public IEnumerable<int> SpeciesFactsIds { get; set; }
        public string InstitutionId { get; set; }

        public IEnumerable<int> DatasourceIds { get; set; }

        public IEnumerable<int> Years { get; set; }

        public DateFilterComparison YearsComparison { get; set; } = DateFilterComparison.StartDate;
    }
}