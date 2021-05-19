using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Operator to use when TaxonListIds is specified.
    /// </summary>
    public enum TaxonListOperatorDto
    {
        /// <summary>
        /// The taxon ids in the specified taxon lists is merged with the taxa
        /// specified in the taxon filter.
        /// </summary>
        Merge,

        /// <summary>
        /// The specified taxa in the taxon filter is filtered to include only
        /// those who exists in the specified taxon lists.
        /// </summary>
        Filter
    }

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
        public IEnumerable<int> Ids { get; set; }

        /// <summary>
        /// Add (merge) or filter taxa by using taxon lists.
        /// </summary>
        public IEnumerable<int> TaxonListIds { get; set; }

        /// <summary>
        /// Operator to use when TaxonListIds is specified. The operators are Merge or Filter.
        /// </summary>
        public TaxonListOperatorDto TaxonListOperator { get; set; } = TaxonListOperatorDto.Merge;
    }
}