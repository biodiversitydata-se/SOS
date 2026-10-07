using SOS.Api.Client.Abstractions;
using SOS.Api.Contracts.Taxonomy.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Status.Web.Client.Abstractions;

namespace SOS.Status.Web.Services;

public class TaxonDiagramService : ITaxonDiagramService
{
    private readonly ISosSystemsStatusInfoClient _apiClient;

    public TaxonDiagramService(ISosSystemsStatusInfoClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE")
    {
        return await _apiClient.GetTaxonRelationsDiagramAsync(
            taxonIds,
            Enum.IsDefined(treeIterationMode) ? treeIterationMode : TaxonRelationsTreeIterationMode.BothParentsAndChildren,
            includeSecondaryRelations,
            DiagramFormat.Mermaid,
            Enum.IsDefined(taxonomyType) ? taxonomyType : TaxonomyType.Dyntaxa,
            translationCultureCode);
    }
}
