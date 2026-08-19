using ApiDiagramFormat = SOS.Api.Contracts.Status.Requests.DiagramFormat;
using ApiTaxonIterationMode = SOS.Api.Contracts.Status.Requests.TaxonRelationsTreeIterationMode;
using SOS.Api.Client.Abstractions;
using SOS.Status.Web.Client.Abstractions;
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
            translationCultureCode);
    }
}
