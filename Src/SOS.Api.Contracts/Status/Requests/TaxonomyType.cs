namespace SOS.Api.Contracts.Status.Requests;

/// <summary>Determines which taxonomy tree is used when creating a taxon diagram.</summary>
public enum TaxonomyType
{
    /// <summary>Use the Dyntaxa taxonomy tree.</summary>
    Dyntaxa = 0,

    /// <summary>Use the Artportalen taxonomy tree.</summary>
    Artportalen = 1
}
