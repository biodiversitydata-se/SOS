using System.Collections.Generic;
using SOS.Observations.Api.Dtos.Enum;

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
        public DataProviderFilterDto DataProvider { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto Date { get; set; }

        /// <summary>
        /// Filter by diffusion status.
        /// </summary>
        public IEnumerable<DiffusionStatusDto> DiffusionStatuses { get; set; }

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
        /// If true, only validated observations will be returned.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        /// This property indicates whether to search for present observations and/or absent observations.
        /// If no value is set, this will be set to include only present observations.
        /// </summary>
        public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

        /// <summary>
        /// Project id's to match.
        /// </summary>
        public List<int> ProjectIds { get; set; }
    }
}