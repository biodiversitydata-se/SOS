using System;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter for signal search.
    /// </summary>
    public class SignalFilterDto 
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
        public DataProviderFilterDto DataProvider { get; set; }

        /// <summary>
        /// Geometry filter 
        /// </summary>
        public GeographicsFilterDto Geographics { get; set; }

        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Taxon filter.
        /// </summary>
        public TaxonFilterBaseDto Taxon { get; set; }
    }
}