using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Taxon filter.
    /// </summary>
    public class TaxonFilterDto
    {
        /// <summary>
        ///     Taxa to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }

        /// <summary>
        ///     Decides whether to search for the exact taxa or
        ///     for the hierarchical underlying taxa.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        ///     Redlist categories to match. Queryable values are available in Field Mappings.
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }
    }
}