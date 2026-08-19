namespace SOS.Api.Contracts.Observations.Requests;
public class ExtendedFilterDto
{

    /// <summary>
    /// Checklist Id
    /// </summary>
    public int? ChecklistId { get; set; }

    /// <summary>
    /// Field diary group Id's
    /// </summary>
    public ICollection<int>? FieldDiaryGroupIds { get; set; }

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
    public ICollection<int>? SexIds { get; set; }

    /// <summary>
    /// Filters observations based on whether an invasive-species treatment is registered.
    /// </summary>
    public bool? HasInvasiveSpeciesTreatment { get; set; }

    /// <summary>
    /// Invasive-species treatment vocabulary identifiers to match.
    /// </summary>
    public ICollection<int>? InvasiveSpeciesTreatmentIds { get; set; }

    /// <summary>
    /// Only include hits with media associated
    /// </summary>
    public bool OnlyWithMedia { get; set; }

    /// <summary>
    /// Only include hits with notes attached to them
    /// </summary>
    public bool OnlyWithNotes { get; set; }

    public bool OnlyWithNotesOfInterest { get; set; }

    /// <summary>
    /// Only include hits that have user comments on them
    /// </summary>
    public bool OnlyWithUserComments { get; set; }

    public bool OnlyWithBarcode { get; set; }

    public System.DateTimeOffset? ReportedDateFrom { get; set; }

    public System.DateTimeOffset? ReportedDateTo { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingTypeFilterDto TypeFilter { get; set; }

    public bool UsePeriodForAllYears { get; set; }

    public ICollection<int>? Months { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto MonthsComparison { get; set; }

    public ICollection<int>? DiscoveryMethodIds { get; set; }

    public ICollection<int>? LifeStageIds { get; set; }

    public ICollection<int>? ActivityIds { get; set; }

    public bool HasTriggeredVerificationRule { get; set; }

    public bool HasTriggeredVerificationRuleWithWarning { get; set; }

    public int? Length { get; set; }

    public string? LengthOperator { get; set; }

    public int? Weight { get; set; }

    public string? WeightOperator { get; set; }

    public int? Quantity { get; set; }

    public string? QuantityOperator { get; set; }

    public ICollection<int>? VerificationStatusIds { get; set; }

    public ICollection<int>? ExcludeVerificationStatusIds { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingUnspontaneousFilterDto UnspontaneousFilter { get; set; }

    public string? SpeciesCollectionLabel { get; set; }

    public string? PublicCollection { get; set; }

    public string? PrivateCollection { get; set; }

    public int? SubstrateSpeciesId { get; set; }

    public int? SubstrateId { get; set; }

    public int? BiotopeId { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotPresentFilterDto NotPresentFilter { get; set; }

    public bool OnlySecondHandInformation { get; set; }

    public ICollection<int>? PublishTypeIdsFilter { get; set; }

    public ICollection<int>? TriggeredObservationRuleFrequencyIds { get; set; }

    public ICollection<int>? TriggeredObservationRuleReproductionIds { get; set; }

    public ICollection<int>? SiteIds { get; set; }

    public ICollection<int>? SiteProjectIds { get; set; }

    public ICollection<int>? SpeciesFactsIds { get; set; }

    public string? InstitutionId { get; set; }

    public ICollection<int>? DatasourceIds { get; set; }

    public ICollection<int>? Years { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto YearsComparison { get; set; }

    public ICollection<int>? SightingTypeSearchGroupIds { get; set; }

}


