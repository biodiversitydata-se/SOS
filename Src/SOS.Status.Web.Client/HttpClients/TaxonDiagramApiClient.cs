using SOS.Api.Contracts.Observations.Requests;
using SOS.Status.Web.Client.Abstractions;
using SOS.Api.Contracts.Taxonomy.Requests;
using System.Net;

namespace SOS.Status.Web.Client.HttpClients;

public class TaxonDiagramApiClient : ITaxonDiagramService
{
    private readonly HttpClient _http;

    public TaxonDiagramApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = true,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE")
    {
        var query = string.Join("&", taxonIds.Select(id => $"taxonIds={id}"));

        var url = $"/api/taxon-diagram?{query}" +
                  $"&treeIterationMode={treeIterationMode}" +
                  $"&includeSecondaryRelations={includeSecondaryRelations.ToString().ToLower()}" +
                  $"&diagramFormat=Mermaid" +
                  $"&taxonomyType={taxonomyType}" +
                  $"&translationCultureCode={translationCultureCode}";

        try
        {
            var response = await _http.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error: {error}";
            }
            else
            {
                return $"Unknown error: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
