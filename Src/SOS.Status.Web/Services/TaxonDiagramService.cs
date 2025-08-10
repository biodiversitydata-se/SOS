using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.HttpClients;

namespace SOS.Status.Web.Services;

public class TaxonDiagramService : ITaxonDiagramService
{
    private readonly SosObservationsApiClient _apiClient;

    public TaxonDiagramService(SosObservationsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        string translationCultureCode = "sv-SE")
    {
        return await _apiClient.GetTaxonRelationsDiagramAsync(
            taxonIds,
            treeIterationMode,
            includeSecondaryRelations,
            translationCultureCode);
    }
}
