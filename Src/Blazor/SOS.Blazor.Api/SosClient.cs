using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SOS.Blazor.Shared;

namespace SOS.Blazor.Api
{
    public class SosClient
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, 
            Converters = { new JsonStringEnumConverter() }
        };
        
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
                var pagedResult = JsonSerializer.Deserialize<PagedResultDto<UserStatisticsItem>>(resultString, _jsonSerializerOptions);
                return pagedResult;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<PagedResultDto<Area>?> GetAreas(int skip, int take, AreaType areaType)
        {
            var response = await _client.GetAsync($"{_apiUrl}Areas?take={take}&skip={skip}&areaTypes={areaType}");
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<PagedResultDto<Area>>(resultString, _jsonSerializerOptions);
                return pagedResult;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
    }
}
