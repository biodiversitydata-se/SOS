using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Taxon filter.
    /// </summary>
    public class TaxonFilterDto
    {
        /// <summary>
        /// If true, also include the underlying hierarchical taxa in search.
        /// E.g. If ids=[4000104](Aves) and includeUnderlyingTaxa=true, then you search for all birds.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        /// Redlist categories to match.
        /// Possible values are: "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE"
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        /// Dyntaxa taxon id's to match.
        /// </summary>
        public IEnumerable<int> Ids { get; set; }
    }
}