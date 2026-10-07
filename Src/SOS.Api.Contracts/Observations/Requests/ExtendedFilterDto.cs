namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Additional criteria for filtering observations.</summary>
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

    /// <summary>Only include observations with notes of interest.</summary>
    public bool OnlyWithNotesOfInterest { get; set; }

    /// <summary>
    /// Only include hits that have user comments on them
    /// </summary>
    public bool OnlyWithUserComments { get; set; }

    /// <summary>Only include observations with a barcode.</summary>
    public bool OnlyWithBarcode { get; set; }

    /// <summary>Earliest report date to include.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? ReportedDateFrom { get; set; }

    /// <summary>Latest report date to include.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(SOS.Api.Contracts.Common.FlexibleNullableDateTimeOffsetConverter))]
    public System.DateTimeOffset? ReportedDateTo { get; set; }

    /// <summary>Controls which Artportalen observation types, including merged observations, are included.</summary>
    public SightingTypeFilterDto TypeFilter { get; set; }

    /// <summary>Apply the date filter's calendar period to every year, ignoring the specified years.</summary>
    public bool UsePeriodForAllYears { get; set; }

    /// <summary>The observation months to match, numbered 1 through 12.</summary>
    public ICollection<int>? Months { get; set; }

    /// <summary>Selects how the start and end dates are compared with Months.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto MonthsComparison { get; set; }

    /// <summary>Discovery-method vocabulary identifiers to match.</summary>
    public ICollection<int>? DiscoveryMethodIds { get; set; }

    /// <summary>Life-stage vocabulary identifiers to match.</summary>
    public ICollection<int>? LifeStageIds { get; set; }

    /// <summary>Activity vocabulary identifiers to match.</summary>
    public ICollection<int>? ActivityIds { get; set; }

    /// <summary>Only include observations that have triggered a verification rule.</summary>
    public bool HasTriggeredVerificationRule { get; set; }

    /// <summary>Only include observations that have triggered a verification rule with a warning.</summary>
    public bool HasTriggeredVerificationRuleWithWarning { get; set; }

    /// <summary>The reported organism length to compare using LengthOperator.</summary>
    public int? Length { get; set; }

    /// <summary>The length comparison operator: eq, gte, lt or lte.</summary>
    public string? LengthOperator { get; set; }

    /// <summary>The reported organism weight to compare using WeightOperator.</summary>
    public int? Weight { get; set; }

    /// <summary>The weight comparison operator: eq, gte, lt or lte.</summary>
    public string? WeightOperator { get; set; }

    /// <summary>The reported organism quantity to compare using QuantityOperator.</summary>
    public int? Quantity { get; set; }

    /// <summary>The quantity comparison operator: eq, gte, lt or lte. Use missing to find observations without a quantity.</summary>
    public string? QuantityOperator { get; set; }

    /// <summary>Verification-status vocabulary identifiers to include.</summary>
    public ICollection<int>? VerificationStatusIds { get; set; }

    /// <summary>Verification-status vocabulary identifiers to exclude.</summary>
    public ICollection<int>? ExcludeVerificationStatusIds { get; set; }

    /// <summary>Filters observations by whether the organism occurs naturally at the location.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingUnspontaneousFilterDto UnspontaneousFilter { get; set; }

    /// <summary>The species collection label to match.</summary>
    public string? SpeciesCollectionLabel { get; set; }

    /// <summary>The public collection name to match.</summary>
    public string? PublicCollection { get; set; }

    /// <summary>The private collection name to match.</summary>
    public string? PrivateCollection { get; set; }

    /// <summary>The taxon identifier of the substrate species to match.</summary>
    public int? SubstrateSpeciesId { get; set; }

    /// <summary>The substrate identifier to match.</summary>
    public int? SubstrateId { get; set; }

    /// <summary>The biotope vocabulary identifier to match.</summary>
    public int? BiotopeId { get; set; }

    /// <summary>Filters observations marked as never found at the location.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotPresentFilterDto NotPresentFilter { get; set; }

    /// <summary>Only include observations based on second-hand information.</summary>
    public bool OnlySecondHandInformation { get; set; }

    /// <summary>Artportalen publication-type identifiers to match.</summary>
    public ICollection<int>? PublishTypeIdsFilter { get; set; }

    /// <summary>Identifiers of triggered observation frequency rules to match.</summary>
    public ICollection<int>? TriggeredObservationRuleFrequencyIds { get; set; }

    /// <summary>Identifiers of triggered observation reproduction rules to match.</summary>
    public ICollection<int>? TriggeredObservationRuleReproductionIds { get; set; }

    /// <summary>Artportalen site identifiers to match, including observations at child sites.</summary>
    public ICollection<int>? SiteIds { get; set; }

    /// <summary>Location project identifiers to match.</summary>
    public ICollection<int>? SiteProjectIds { get; set; }

    /// <summary>Species-fact identifiers to match.</summary>
    public ICollection<int>? SpeciesFactsIds { get; set; }

    /// <summary>The identifier of the institution associated with the observation.</summary>
    public string? InstitutionId { get; set; }

    /// <summary>Artportalen data-source identifiers to match.</summary>
    public ICollection<int>? DatasourceIds { get; set; }

    /// <summary>The observation years to match.</summary>
    public ICollection<int>? Years { get; set; }

    /// <summary>Selects whether Years is compared with the start date, end date or both. StartDateEndDateMonthRange uses the start year.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto YearsComparison { get; set; }

    /// <summary>Explicit Artportalen sighting-type search-group identifiers to match.</summary>
    public ICollection<int>? SightingTypeSearchGroupIds { get; set; }

}


