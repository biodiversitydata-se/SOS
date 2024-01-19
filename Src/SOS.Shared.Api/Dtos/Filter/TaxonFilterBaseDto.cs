
namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Taxon filter.
    /// </summary>
    public class TaxonFilterBaseDto
    {
        /// <summary>
        /// If true, also include the underlying hierarchical taxa in search.
        /// E.g. If ids=[4000104](Aves) and includeUnderlyingTaxa=true, then you search for all birds.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        /// Dyntaxa taxon id's to match.
        /// </summary>
        public IEnumerable<int>? Ids { get; set; }

        /// <summary>
        /// Add (merge) or filter taxa by using taxon lists.
        /// </summary>
        public IEnumerable<int>? TaxonListIds { get; set; }
    }
}