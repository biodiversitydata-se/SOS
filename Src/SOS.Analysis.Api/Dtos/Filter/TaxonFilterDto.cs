using SOS.Analysis.Api.Dtos.Filter.Enums;

namespace SOS.Analysis.Api.Dtos.Filter
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
        /// Dyntaxa taxon id's to match.
        /// </summary>
        public IEnumerable<int>? Ids { get; set; }

        /// <summary>
        /// Redlist categories to match.
        /// Possible values are: "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE"
        /// </summary>
        public IEnumerable<string>? RedListCategories { get; set; }

        /// <summary>
        /// Taxon categories to match.
        /// </summary>
        public IEnumerable<int>? TaxonCategories { get; set; }

        /// <summary>
        /// Add (merge) or filter taxa by using taxon lists.
        /// </summary>
        public IEnumerable<int>? TaxonListIds { get; set; }

        /// <summary>
        /// Operator to use when TaxonListIds is specified. The operators are Merge or Filter.
        /// </summary>
        public TaxonListOperatorDto? TaxonListOperator { get; set; } = TaxonListOperatorDto.Merge;
    }
}