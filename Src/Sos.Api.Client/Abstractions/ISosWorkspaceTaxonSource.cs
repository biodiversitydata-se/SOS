using SOS.Api.Client.Workspaces;

namespace SOS.Api.Client.Abstractions;

/// <summary>
/// Provides the taxon and taxon-list data required to resolve a workspace taxon selection.
/// </summary>
public interface ISosWorkspaceTaxonSource
{
    /// <summary>Gets metadata for one taxon.</summary>
    Task<SosResolvedTaxon?> GetTaxonAsync(int taxonId, CancellationToken cancellationToken = default);

    /// <summary>Gets all descendants of one taxon.</summary>
    Task<IReadOnlyCollection<SosResolvedTaxon>> GetDescendantsAsync(
        int taxonId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the taxon identifiers represented by a taxon-list selection.</summary>
    Task<IReadOnlyCollection<int>> GetTaxonListIdsAsync(
        int taxonListId,
        bool includeChildTaxaForRedlists,
        CancellationToken cancellationToken = default);
}
