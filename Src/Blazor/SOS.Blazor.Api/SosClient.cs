using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SOS.Blazor.Shared.Models;

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

        public async Task<PagedResult<UserStatisticsItem>?> GetUserStatisticsAsync(int skip, int take, bool useCache, SpeciesCountUserStatisticsQuery query)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            //var response = await _client.PostAsync($"{_apiUrl}UserStatistics/SpeciesCountAggregation?skip={skip}&take={take}&useCache={useCache}", content);
            var response = await _client.PostAsync($"{_apiUrl}UserStatistics/PagedSpeciesCountAggregation?skip={skip}&take={take}&useCache={useCache}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var pagedResult = JsonSerializer.Deserialize<PagedResult<UserStatisticsItem>>(resultString, _jsonSerializerOptions);
                return pagedResult;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<PagedResult<Area>?> GetAreas(int skip, int take, AreaType areaType)
        {
            var response = await _client.GetAsync($"{_apiUrl}Areas?take={take}&skip={skip}&areaTypes={areaType}");
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<PagedResult<Area>>(resultString, _jsonSerializerOptions);
                return pagedResult;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<DataProvider[]> GetDataProviders()
        {
            var response = await _client.GetAsync($"{_apiUrl}DataProviders?cultureCode=sv-SE&includeProvidersWithNoObservations=false");
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DataProvider[]>(resultString, _jsonSerializerOptions);
                return result;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<ProcessInfo> GetProcessInfo(bool active)
        {
            var response = await _client.GetAsync($"{_apiUrl}Systems/ProcessInformation?active={active}");
            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ProcessInfo>(resultString, _jsonSerializerOptions);
                return result;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
    }
}
