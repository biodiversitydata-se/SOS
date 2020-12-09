using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterBaseDto
    {
        /// <summary>
        ///     Only get data from these providers.
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto Date { get; set; }

        /// <summary>
        /// Taxon filter.
        /// </summary>
        public TaxonFilterDto Taxon { get; set; }

        /// <summary>
        /// Area filter
        /// </summary>
        public IEnumerable<AreaFilterDto> Areas { get; set; }

        /// <summary>
        /// Geometry filter 
        /// </summary>
        public GeometryFilterDto Geometry { get; set; }

        /// <summary>
        /// Set to true to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        /// This property indicates whether to search for present observations and/or absent observations.
        /// If no value is set, this will be set to include only present observations.
        /// </summary>
        public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }
    }
}