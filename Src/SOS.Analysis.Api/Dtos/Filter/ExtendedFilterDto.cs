namespace SOS.Analysis.Api.Dtos.Filter
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
        /// Activity id filter
        /// </summary>
        public IEnumerable<int>? ActivityIds { get; set; }

        /// <summary>
        /// Biotope filter
        /// </summary>
        public int? BiotopeId { get; set; }

        /// <summary>
        /// Checklist Id
        /// </summary>
        public int? ChecklistId { get; set; }

        /// <summary>
        /// Data source id's
        /// </summary>
        public IEnumerable<int>? DatasourceIds { get; set; }

        /// <summary>
        /// Discovery method filter
        /// </summary>
        public IEnumerable<int>? DiscoveryMethodIds { get; set; }

        /// <summary>
        /// Exclude verification status id's
        /// </summary>
        public IEnumerable<int>? ExcludeVerificationStatusIds { get; set; }

        /// <summary>
        /// Field diary group Id's
        /// </summary>
        public IEnumerable<int>? FieldDiaryGroupIds { get; set; }

        /// <summary>
        /// Institution filter
        /// </summary>
        public string? InstitutionId { get; set; }

        /// <summary>
        /// Length filter
        /// </summary>
        public int? Length { get; set; }
        
        /// <summary>
        /// Length operator
        /// </summary>
        public string? LengthOperator { get; set; }

        /// <summary>
        /// Life stage filter
        /// </summary>
        public IEnumerable<int>? LifeStageIds { get; set; }

        /// <summary>
        /// Months filter
        /// </summary>
        public IEnumerable<int>? Months { get; set; }
        
        /// <summary>
        /// Months comparsion 
        /// </summary>
        public DateFilterComparisonDto MonthsComparison { get; set; } = DateFilterComparisonDto.StartDate;

        /// <summary>
        /// Not present filter
        /// </summary>
        public SightingNotPresentFilterDto NotPresentFilter { get; set; }

        /// <summary>
        /// Observed by Artportalen user id.
        /// </summary>
        public int? ObservedByUserId { get; set; }

        /// <summary>
        /// Observed by user service user id.
        /// </summary>
        public int? ObservedByUserServiceUserId { get; set; }

        /// <summary>
        /// Only second hand information filter
        /// </summary>
        public bool OnlySecondHandInformation { get; set; }

        /// <summary>
        /// Only with barcode filter
        /// </summary>
        public bool OnlyWithBarcode { get; set; }

        /// <summary>
        /// Only include hits with media associated
        /// </summary>
        public bool OnlyWithMedia { get; set; }
        /// <summary>
        /// Only include hits with notes attached to them
        /// </summary>
        public bool OnlyWithNotes { get; set; }

        /// <summary>
        /// Only include hits that have user comments on them
        /// </summary>
        public bool OnlyWithUserComments { get; set; } = false;

        /// <summary>
        /// Only with notes filter
        /// </summary>
        public bool OnlyWithNotesOfInterest { get; set; }

        /// <summary>
        /// Quantity filter
        /// </summary>
        public int? Quantity { get; set; }
        
        /// <summary>
        /// Operator used for quantity filter
        /// </summary>
        public string? QuantityOperator { get; set; }

        /// <summary>
        /// Private collection filter
        /// </summary>
        public string? PrivateCollection { get; set; }

        /// <summary>
        /// Public collection filter
        /// </summary>
        public string? PublicCollection { get; set; }

        /// <summary>
        /// Publish type filter
        /// </summary>
        public IEnumerable<int>? PublishTypeIdsFilter { get; set; }

        /// <summary>
        /// Regional sighting state filter
        /// </summary>
        public IEnumerable<int>? RegionalSightingStateIdsFilter { get; set; }

        /// <summary>
        /// Reported by Artportalen user id.
        /// </summary>
        public int? ReportedByUserId { get; set; }

        /// <summary>
        /// Reported by user service user id.
        /// </summary>
        public int? ReportedByUserServiceUserId { get; set; }

        /// <summary>
        /// Reported from date
        /// </summary>
        public DateTime? ReportedDateFrom { get; set; }

        /// <summary>
        /// Reported to date
        /// </summary>
        public DateTime? ReportedDateTo { get; set; }

        /// <summary>
        /// Id of sex to match
        /// </summary>
        public IEnumerable<int>? SexIds { get; set; }

        /// <summary>
        /// Sighting type search group id's
        /// </summary>
        public IEnumerable<int>? SightingTypeSearchGroupIds { get; set; }

        /// <summary>
        /// Site id filter
        /// </summary>
        public IEnumerable<int>? SiteIds { get; set; }

        /// <summary>
        /// Site projects filter
        /// </summary>
        public IEnumerable<int>? SiteProjectIds { get; set; }

        /// <summary>
        /// Species collection label
        /// </summary>
        public string? SpeciesCollectionLabel { get; set; }

        /// <summary>
        /// Species fact filter
        /// </summary>
        public IEnumerable<int>? SpeciesFactsIds { get; set; }

        /// <summary>
        /// Substrate filter
        /// </summary>
        public int? SubstrateId { get; set; }
        
        /// <summary>
        /// Substrate species filter
        /// </summary>
        public int? SubstrateSpeciesId { get; set; }
       

        /// <summary>
        /// Triggered observation rule frequency id's
        /// </summary>
        public IEnumerable<int>? TriggeredObservationRuleFrequencyIds { get; set; }

        /// <summary>
        ///  Triggered observation rule reproduction id's
        /// </summary>
        public IEnumerable<int>? TriggeredObservationRuleReproductionIds { get; set; }

        /// <summary>
        /// Sighting type filter
        /// </summary>
        public SightingTypeFilterDto TypeFilter { get; set; } = SightingTypeFilterDto.DoNotShowMerged;

        /// <summary>
        /// Unspontaneous filter
        /// </summary>
        public SightingUnspontaneousFilterDto UnspontaneousFilter { get; set; }

        /// <summary>
        /// Use same period for all years if true
        /// </summary>
        public bool UsePeriodForAllYears { get; set; }

        /// <summary>
        /// Verification status filter
        /// </summary>
        public IEnumerable<int>? VerificationStatusIds { get; set; }

        /// <summary>
        /// Weight filter
        /// </summary>
        public int? Weight { get; set; }
        
        /// <summary>
        /// Operator used for weight filter
        /// </summary>
        public string? WeightOperator { get; set; }

        /// <summary>
        /// Year filter
        /// </summary>
        public IEnumerable<int>? Years { get; set; }

        /// <summary>
        /// Year comparison
        /// </summary>
        public DateFilterComparisonDto YearsComparison { get; set; } = DateFilterComparisonDto.StartDate;
    }
}