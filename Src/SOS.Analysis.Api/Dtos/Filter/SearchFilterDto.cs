using SOS.Analysis.Api.Dtos.Filter.Enums;

namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterDto
    {
        /// <summary>
        /// Limit returned observations based on bird nest activity level.
        /// Only bird observations in Artportalen are affected
        /// by this search criteria.
        /// Observation of other organism groups (not birds) are
        /// not affected by this search criteria. 
        /// </summary>
        public int? BirdNestActivityLimit { get; set; }

        /// <summary>
        ///     Only get data from these providers.
        /// </summary>
        public DataProviderFilterDto? DataProvider { get; set; }

        /// <summary>
        /// Data stewardship filter
        /// </summary>
        public DataStewardshipFilterDto? DataStewardship { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto? Date { get; set; }

        /// <summary>
        /// Filter by uncertain determination
        /// </summary>
        public SightingDeterminationFilterDto DeterminationFilter { get; set; } = SightingDeterminationFilterDto.NoFilter;

        /// <summary>
        /// Filter by diffusion status.
        /// </summary>
        public IEnumerable<DiffusionStatusDto>? DiffusionStatuses { get; set; }

        /// <summary>
        /// Event related filter
        /// </summary>
        public EventFilterDto? Event { get; set; }

        /// <summary>
        /// Exclude filter
        /// </summary>
        public ExcludeFilterDto? ExcludeFilter { get; set; }

        /// <summary>
        /// Geographics filter 
        /// </summary>
        public GeographicsFilterDto? Geographics { get; set; }

        /// <summary>
        /// Observation modified filter
        /// </summary>
        public ModifiedDateFilterDto? ModifiedDate { get; set; }

        /// <summary>
        /// Filter for observation not recovered
        /// </summary>
        public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; } = SightingNotRecoveredFilterDto.DontIncludeNotRecovered;

        /// <summary>
        /// Only get observations observed by me
        /// </summary>
        public bool? ObservedByMe { get; set; }

        /// <summary>
        /// This property indicates whether to search for present observations and/or absent observations.
        /// If no value is set, this will be set to include only present observations.
        /// </summary>
        public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

        /// <summary>
        /// Project id's to match.
        /// </summary>
        public IEnumerable<int>? ProjectIds { get; set; }

        /// <summary>
        /// Observation protection filter
        /// </summary>
        public ProtectionFilterDto ProtectionFilter { get; set; }

        /// <summary>
        /// Only get observations reported by me
        /// </summary>
        public bool? ReportedByMe { get; set; }

        /// <summary>
        /// Taxon filter.
        /// </summary>
        public TaxonFilterDto? Taxon { get; set; }

        /// <summary>
        /// Requested verification status.
        /// </summary>
        public StatusVerificationDto VerificationStatus { get; set; } = StatusVerificationDto.BothVerifiedAndNotVerified;
    }
}