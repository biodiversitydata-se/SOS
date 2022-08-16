using System.Text;
using System.Text.Json;
using SOS.Blazor.Shared;

namespace SOS.Blazor.Api
{
    public class SosClient
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;
        public SosClient(string apiUrl)
        {
            _client = new HttpClient();
            _apiUrl = apiUrl;
        }

        public async Task<PagedResultDto<UserStatisticsItem>?> GetUserStatisticsAsync(int skip, int take, SpeciesCountUserStatisticsQuery query)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_apiUrl}UserStatistics/SpeciesCountAggregation?skip={skip}&take={take}", content);
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var jsonSerializerOptions = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
                var pagedResult = JsonSerializer.Deserialize<PagedResultDto<UserStatisticsItem>>(resultString, jsonSerializerOptions);
                return pagedResult;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
    }
}
