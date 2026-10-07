namespace SOS.Api.Contracts.Taxonomy.Requests;
/// <summary>Determines which taxon relationships are traversed when creating a diagram.</summary>
public enum TaxonRelationsTreeIterationMode
{
    /// <summary>Traverse parent and child relationships.</summary>
    BothParentsAndChildren,
    /// <summary>Traverse parent relationships only.</summary>
    OnlyParents,
    /// <summary>Traverse child relationships only.</summary>
    OnlyChildren
}


