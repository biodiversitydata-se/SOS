namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Operator to use when TaxonListIds is specified.
/// </summary>
public enum TaxonListOperatorDto
{
    /// <summary>
    /// Merge taxon IDs in the specified taxon lists with the taxa in the taxon filter.
    /// </summary>
    Merge = 0,
    /// <summary>
    /// Restrict the taxa in the taxon filter to those in the specified taxon lists.
    /// </summary>
    Filter = 1,
}


