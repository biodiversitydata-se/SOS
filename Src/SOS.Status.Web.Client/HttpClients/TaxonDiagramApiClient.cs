using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos;
using System.Net.Http.Json;

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
        string translationCultureCode = "sv-SE")
    {
        var query = string.Join("&", taxonIds.Select(id => $"taxonIds={id}"));

        var url = $"/api/taxon-diagram?{query}" +
                  $"&treeIterationMode={treeIterationMode}" +
                  $"&includeSecondaryRelations={includeSecondaryRelations.ToString().ToLower()}" +
                  $"&diagramFormat=Mermaid" +
                  $"&translationCultureCode={translationCultureCode}";

        try
        {
            var response = await _http.GetAsync(url);


            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<string>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
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