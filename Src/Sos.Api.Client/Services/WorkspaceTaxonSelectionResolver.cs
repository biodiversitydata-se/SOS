using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Workspaces;

namespace SOS.Api.Client.Services;

internal sealed class WorkspaceTaxonSelectionResolver
{
    private readonly ISosWorkspaceTaxonSource? _taxonSource;

    public WorkspaceTaxonSelectionResolver(IEnumerable<ISosWorkspaceTaxonSource> taxonSources)
    {
        var sources = taxonSources.Take(2).ToArray();
        if (sources.Length > 1)
        {
            throw new InvalidOperationException(
                $"Only one {nameof(ISosWorkspaceTaxonSource)} may be registered.");
        }

        _taxonSource = sources.SingleOrDefault();
    }

    public async Task<IReadOnlyCollection<int>> ResolveAsync(
        WorkspaceTaxonSelection selection,
        CancellationToken cancellationToken)
    {
        var includedTaxa = await ResolveSelectedTaxaAsync(selection, false, cancellationToken);
        var excludedTaxa = await ResolveSelectedTaxaAsync(selection, true, cancellationToken);

        var includedFromLists = new List<int>();
        foreach (var taxonListId in selection.TaxonListIds ?? [])
        {
            includedFromLists.AddRange(await GetSource().GetTaxonListIdsAsync(
                taxonListId,
                selection.IncludeChildTaxaForRedlists,
                cancellationToken));
        }

        var hasIncludedSelection = selection.Taxa?.Any(taxon => !taxon.Exclude) == true ||
            selection.SpeciesGroups?.Any(group => !group.Exclude) == true;
        IEnumerable<int> result = hasIncludedSelection
            ? selection.TaxonListIds?.Count > 0
                ? includedTaxa.Intersect(includedFromLists)
                : includedTaxa
            : includedFromLists;

        return result.Except(excludedTaxa).Distinct().ToArray();
    }

    private async Task<List<int>> ResolveSelectedTaxaAsync(
        WorkspaceTaxonSelection selection,
        bool excluded,
        CancellationToken cancellationToken)
    {
        var taxa = selection.Taxa?.Where(taxon => taxon.Exclude == excluded).ToArray() ?? [];
        var speciesGroups = selection.SpeciesGroups?
            .Where(group => group.Exclude == excluded)
            .Select(group => group.TaxonListId)
            .ToArray() ?? [];

        if (taxa.Length == 0 && speciesGroups.Length == 0)
        {
            return [];
        }

        var result = taxa.Select(taxon => taxon.TaxonId).ToList();
        var categories = selection.TaxonCategoryIds?.ToHashSet();

        foreach (var selectedTaxon in taxa.Where(taxon => taxon.IncludeChildren))
        {
            var source = GetSource();
            var descendants = await source.GetDescendantsAsync(selectedTaxon.TaxonId, cancellationToken);
            result.AddRange(descendants
                .Where(taxon => categories is null || categories.Count == 0 || categories.Contains(taxon.TaxonCategoryId))
                .Select(taxon => taxon.Id));

            if (categories?.Count > 0)
            {
                var taxon = await source.GetTaxonAsync(selectedTaxon.TaxonId, cancellationToken)
                    ?? throw new SosWorkspaceFilterValidationException(nameof(WorkspaceSelectedTaxon.TaxonId),
                        $"Taxon {selectedTaxon.TaxonId} could not be found for category filtering.");
                if (!categories.Contains(taxon.TaxonCategoryId))
                {
                    result.Remove(selectedTaxon.TaxonId);
                }
            }
        }

        foreach (var speciesGroupId in speciesGroups)
        {
            result.AddRange(await GetSource().GetTaxonListIdsAsync(
                speciesGroupId,
                false,
                cancellationToken));
        }

        return result;
    }

    private ISosWorkspaceTaxonSource GetSource() =>
        _taxonSource ?? throw new SosWorkspaceFilterValidationException(
            nameof(ISosWorkspaceTaxonSource),
            "A workspace taxon source must be registered when the workspace contains child taxa, taxon lists or species groups.");
}
