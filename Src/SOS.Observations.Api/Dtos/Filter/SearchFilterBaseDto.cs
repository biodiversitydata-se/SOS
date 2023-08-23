using System;
using System.Collections.Generic;
using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterBaseDto
    {
        public enum SightingDeterminationFilterDto
        {
            NoFilter,
            NotUnsureDetermination,
            OnlyUnsureDetermination
        }

        public enum SightingNotRecoveredFilterDto
        {
            NoFilter,
            OnlyNotRecovered,
            DontIncludeNotRecovered
        }

        /*[Obsolete]
        public enum StatusValidationDto
        {
            BothValidatedAndNotValidated,
            Validated,
            NotValidated
        }*/

        public enum StatusVerificationDto
        {
            BothVerifiedAndNotVerified,
            Verified,
            NotVerified
        }

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
        public DataProviderFilterDto DataProvider { get; set; }

        /// <summary>
        ///  DataStewardship filter
        /// </summary>
        public DataStewardshipFilterDto DataStewardship { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto Date { get; set; }

        /// <summary>
        /// Filter by uncertain determination
        /// </summary>
        public SightingDeterminationFilterDto DeterminationFilter { get; set; }

        /// <summary>
        /// Filter by diffusion status.
        /// </summary>
        public IEnumerable<DiffusionStatusDto> DiffusionStatuses { get; set; }

        /// <summary>
        /// Exclude filter
        /// </summary>
        public ExcludeFilterDto ExcludeFilter { get; set; }

        /// <summary>
        /// Geographics filter 
        /// </summary>
        public GeographicsFilterDto Geographics { get; set; }

        /// <summary>
        /// Observation modified filter
        /// </summary>
        public ModifiedDateFilterDto ModifiedDate { get; set; }

        /// <summary>
        /// Filter for observation not recovered
        /// </summary>
        public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

        /// <summary>
        /// Only get observations observed by me
        /// </summary>
        public bool ObservedByMe { get; set; }

        /// <summary>
        /// This property indicates whether to search for present observations and/or absent observations.
        /// If no value is set, this will be set to include only present observations.
        /// </summary>
        public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

        /// <summary>
        /// Project id's to match.
        /// </summary>
        public List<int> ProjectIds { get; set; }

        /// <summary>
        /// Only get observations reported by me
        /// </summary>
        public bool ReportedByMe { get; set; }

        /// <summary>
        /// Taxon filter.
        /// </summary>
        public TaxonFilterDto Taxon { get; set; }

        /// <summary>
        /// Requested verification status.
        /// </summary>
        public StatusVerificationDto VerificationStatus { get; set; }
    }
}