using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Area filter.
    /// </summary>
    public class TaxonFilter
    {
        /// <summary>
        /// Operator to use when TaxonListIds is specified.
        /// </summary>
        public enum TaxonListOp
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
        ///     Taxa to match. Queryable values are available in Dyntaxa.
        /// </summary>
        public IEnumerable<int> Ids { get; set; }

        /// <summary>
        ///     Decides whether to search for the exact taxa or
        ///     for the hierarchical underlying taxa.
        /// </summary>
        public bool IncludeUnderlyingTaxa { get; set; }

        /// <summary>
        /// Add (merge) or filter taxa by using taxon lists.
        /// </summary>
        public IEnumerable<int> ListIds { get; set; }

        /// <summary>
        ///     Redlist categories to match.
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }

        /// <summary>
        ///     Taxon categories.
        /// </summary>
        public List<int> TaxonCategories { get; set; }

        /// <summary>
        /// Operator to use when TaxonListIds is specified. The operators are Merge or Filter.
        /// </summary>
        public TaxonListOp TaxonListOperator { get; set; } = TaxonListOp.Merge;
    }
}