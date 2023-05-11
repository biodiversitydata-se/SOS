namespace SOS.Analysis.Api.Dtos.Filter.Enums
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
}
