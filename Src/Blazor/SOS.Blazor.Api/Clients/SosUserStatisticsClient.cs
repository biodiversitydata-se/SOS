using System.Diagnostics;

namespace SOS.Blazor.Api.Clients;

public class SosUserStatisticsClient : ISosUserStatisticsClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SosUserStatisticsClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PagedResult<UserStatisticsItem>> GetUserStatisticsAsync(int skip, int take, bool useCache, SpeciesCountUserStatisticsQuery query)
    {
        //var response = await _client.PostAsync($"{_apiUrl}UserStatistics/SpeciesCountAggregation?skip={skip}&take={take}&useCache={useCache}", content);
        var url = $"userstatistics/pagedspeciescountaggregation?skip={skip}&take={take}&useCache={useCache}";
        var response = await _httpClientFactory.CreateClient("SosUserStatisticsClient").PostAsJsonAsync<SpeciesCountUserStatisticsQuery>(url, query);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PagedResult<UserStatisticsItem>>()
            : throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
    }
}
