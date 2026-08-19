namespace SOS.Api.Client.Workspaces;

/// <summary>
/// Taxon metadata required when resolving a workspace taxon selection.
/// </summary>
/// <param name="Id">The taxon identifier.</param>
/// <param name="TaxonCategoryId">The taxon category identifier.</param>
public sealed record SosResolvedTaxon(int Id, int TaxonCategoryId);
