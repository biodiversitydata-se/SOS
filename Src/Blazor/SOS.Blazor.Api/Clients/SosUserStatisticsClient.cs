using System.Diagnostics;

namespace SOS.Blazor.Api.Clients;

public class SosUserStatisticsClient : ISosUserStatisticsClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SosUserStatisticsClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PagedResult<UserStatisticsItem>> GetUserStatisticsAsync(int skip, int take, bool useCache, SpeciesCountUserStatisticsQuery query)
    {
        //var response = await _client.PostAsync($"{_apiUrl}UserStatistics/SpeciesCountAggregation?skip={skip}&take={take}&useCache={useCache}", content);
        var url = $"userstatistics/pagedspeciescountaggregation?skip={skip}&take={take}&useCache={useCache}";

        var getUserStatistics = async () =>
        {
            var watch = new Stopwatch();
            watch.Start();
            var response = await _httpClientFactory.CreateClient("SosUserStatisticsClient").PostAsJsonAsync<SpeciesCountUserStatisticsQuery>(url, query);
            var data = await response.Content.ReadFromJsonAsync<PagedResult<UserStatisticsItem>>();
            watch.Stop();
            Debug.WriteLine($"Response time: {watch.Elapsed.TotalMilliseconds}");
            return (response, data);
        };

        var res = await getUserStatistics();
        return res.response.IsSuccessStatusCode? res.data : throw new Exception("Call to API failed, responseCode:" + res.response.StatusCode);
    }
}
