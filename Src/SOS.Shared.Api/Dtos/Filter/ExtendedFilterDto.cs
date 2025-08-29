namespace SOS.Shared.Api.Dtos.Filter
{
    public class ExtendedFilterDto
    {
        public enum SightingTypeFilterDto
        {
            DoNotShowMerged,
            ShowOnlyMerged,
            ShowBoth,
            DoNotShowSightingsInMerged,
            DoNotShowMergedIncludeReplacementChilds
        }

        public enum SightingUnspontaneousFilterDto
        {
            NoFilter,
            NotUnspontaneous,
            Unspontaneous
        }
        public enum SightingNotPresentFilterDto
        {
            NoFilter,
            DontIncludeNotPresent,
            OnlyNotPresent,
            IncludeNotPresent
        }

        public enum DateFilterComparisonDto
        {
            StartDate,
            EndDate,
            BothStartDateAndEndDate,
            StartDateEndDateMonthRange
        }

        /// <summary>
        /// Checklist Id
        /// </summary>
        public int? ChecklistId { get; set; }

        /// <summary>
        /// Field diary group Id's
        /// </summary>
        public IEnumerable<int>? FieldDiaryGroupIds { get; set; }

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

        /// <summary>
        /// Id of sex to match
        /// </summary>
        public IEnumerable<int>? SexIds { get; set; }

        /// <summary>
        /// Ids of InvasiveSpeciesTreatment to match. 
        /// If null or empty, no filtering is done.
        /// Queryable values are available in InvasiveSpeciesTreatment vocabulary.
        /// </summary>
        public IEnumerable<int>? InvasiveSpeciesTreatmentIds { get; set; }

        /// <summary>
        /// Only include hits with media associated
        /// </summary>
        public bool OnlyWithMedia { get; set; } = false;
        /// <summary>
        /// Only include hits with notes attached to them
        /// </summary>
        public bool OnlyWithNotes { get; set; } = false;

        public bool OnlyWithNotesOfInterest { get; set; } = false;
        /// <summary>
        /// Only include hits that have user comments on them
        /// </summary>
        public bool OnlyWithUserComments { get; set; } = false;

        public bool OnlyWithBarcode { get; set; } = false;

        public DateTime? ReportedDateFrom { get; set; }
        public DateTime? ReportedDateTo { get; set; }
        public SightingTypeFilterDto TypeFilter { get; set; } = SightingTypeFilterDto.DoNotShowMerged;

        public bool UsePeriodForAllYears { get; set; } = false;
        public IEnumerable<int>? Months { get; set; }
        public DateFilterComparisonDto MonthsComparison { get; set; } = DateFilterComparisonDto.StartDate;
        public IEnumerable<int>? DiscoveryMethodIds { get; set; }

        public IEnumerable<int>? LifeStageIds { get; set; }

        public IEnumerable<int>? ActivityIds { get; set; }


        /*[Obsolete]
        public bool? HasTriggerdValidationRule { get; set; }
        [Obsolete]
        public bool? HasTriggerdValidationRuleWithWarning { get; set; }*/
        public bool HasTriggeredVerificationRule { get; set; } = false;
        public bool HasTriggeredVerificationRuleWithWarning { get; set; } = false;

        public int? Length { get; set; }
        public string? LengthOperator { get; set; }
        public int? Weight { get; set; }
        public string? WeightOperator { get; set; }

        public int? Quantity { get; set; }
        public string? QuantityOperator { get; set; }

        /*[Obsolete]
        public IEnumerable<int> ValidationStatusIds { get; set; }
        [Obsolete]
        public IEnumerable<int> ExcludeValidationStatusIds { get; set; }*/

        public IEnumerable<int>? VerificationStatusIds { get; set; }
        public IEnumerable<int>? ExcludeVerificationStatusIds { get; set; }

        public SightingUnspontaneousFilterDto UnspontaneousFilter { get; set; } = SightingUnspontaneousFilterDto.NoFilter;

        public string? SpeciesCollectionLabel { get; set; }

        public string? PublicCollection { get; set; }

        public string? PrivateCollection { get; set; }

        public int? SubstrateSpeciesId { get; set; }
        public int? SubstrateId { get; set; }

        public int? BiotopeId { get; set; }

        public SightingNotPresentFilterDto NotPresentFilter { get; set; } = SightingNotPresentFilterDto.NoFilter;

        public bool OnlySecondHandInformation { get; set; } = false;

        public IEnumerable<int>? PublishTypeIdsFilter { get; set; }

        public IEnumerable<int>? RegionalSightingStateIdsFilter { get; set; }

        public IEnumerable<int>? TriggeredObservationRuleFrequencyIds { get; set; }

        public IEnumerable<int>? TriggeredObservationRuleReproductionIds { get; set; }

        public IEnumerable<int>? SiteIds { get; set; }

        public IEnumerable<int>? SiteProjectIds { get; set; }

        public IEnumerable<int>? SpeciesFactsIds { get; set; }
        public string? InstitutionId { get; set; }

        public IEnumerable<int>? DatasourceIds { get; set; }

        /*[Obsolete("Use geographics.locationNameFilter")]
        public string LocationNameFilter { get; set; }
        */
        public IEnumerable<int>? Years { get; set; }

        public DateFilterComparisonDto YearsComparison { get; set; } = DateFilterComparisonDto.StartDate;

        public IEnumerable<int>? SightingTypeSearchGroupIds { get; set; }
    }
}