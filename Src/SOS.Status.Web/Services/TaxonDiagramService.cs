using ApiDiagramFormat = SOS.Api.Contracts.Status.Requests.DiagramFormat;
using ApiTaxonomyType = SOS.Api.Contracts.Status.Requests.TaxonomyType;
using ApiTaxonIterationMode = SOS.Api.Contracts.Status.Requests.TaxonRelationsTreeIterationMode;
using SOS.Api.Client.Abstractions;
using SOS.Status.Web.Client.Abstractions;
using WebTaxonomyType = SOS.Status.Web.Client.Dtos.TaxonomyType;
using WebTaxonIterationMode = SOS.Status.Web.Client.Dtos.TaxonRelationsTreeIterationMode;

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
        WebTaxonIterationMode treeIterationMode = WebTaxonIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        WebTaxonomyType taxonomyType = WebTaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE")
    {
        return await _apiClient.GetTaxonRelationsDiagramAsync(
            taxonIds,
            treeIterationMode switch
            {
                WebTaxonIterationMode.BothParentsAndChildren => ApiTaxonIterationMode.BothParentsAndChildren,
                WebTaxonIterationMode.OnlyParents => ApiTaxonIterationMode.OnlyParents,
                WebTaxonIterationMode.OnlyChildren => ApiTaxonIterationMode.OnlyChildren,
                _ => ApiTaxonIterationMode.BothParentsAndChildren
            },
            includeSecondaryRelations,
            ApiDiagramFormat.Mermaid,
            taxonomyType switch
            {
                WebTaxonomyType.Dyntaxa => ApiTaxonomyType.Dyntaxa,
                WebTaxonomyType.Artportalen => ApiTaxonomyType.Artportalen,
                _ => ApiTaxonomyType.Dyntaxa
            },
            translationCultureCode);
    }
}
