namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Which taxon tree to use for the search
/// </summary>
public enum TaxonomyTypeDto
{
    /// <summary>
    /// Use Dyntaxa taxonomy tree (default)
    /// </summary>
    Dyntaxa = 0,

    /// <summary>
    /// Use Artportalen taxonomy tree
    /// </summary>
    Artportalen = 1
}