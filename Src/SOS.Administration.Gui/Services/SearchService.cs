using SOS.Lib.Models.Search;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SOS.Administration.Gui.Dtos;

namespace SOS.Administration.Gui.Services
{
    public class SearchService : ISearchService
    {

        private readonly HttpClient _client;
        private readonly string _apiUrl;
        public SearchService(IOptionsMonitor<ApiTestConfiguration> optionsMonitor)
        {
            _client = new HttpClient();
            _apiUrl = optionsMonitor.CurrentValue.ApiUrl;

        }
        public async Task<PagedResult<SOSObservation>> SearchSOS(SearchFilterDto searchFilter, int take, int skip)
        {
            var response = await _client.PostAsync($"{_apiUrl}Observations/Search?take={take}&skip={skip}", new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<PagedResult<SOSObservation>>(resultString);
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
        public async Task<PagedResult<TaxonAggregationItemDto>> SearchSOSTaxonAggregation(SearchFilterDto searchFilter, int take, int skip, double? bboxleft = null, double? bboxtop = null, double? bboxright = null, double? bboxbottom = null)
        {
            var bboxstring = "";
            if (bboxleft.HasValue && bboxtop.HasValue && bboxright.HasValue && bboxbottom.HasValue)
            {
                bboxstring = $"&bboxLeft={bboxleft}&bboxTop={bboxtop}&bboxRight={bboxright}&bboxBottom={bboxbottom}".Replace(',', '.');
            }
            var response = await _client.PostAsync($"{_apiUrl}Observations/TaxonAggregation?take={take}&skip={skip}" + bboxstring, new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<PagedResult<TaxonAggregationItemDto>>(resultString);
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
        public async Task<GeoGridResultDto> SearchSOSGeoAggregation(SearchFilterDto searchFilter)
        {
            var response = await _client.PostAsync($"{_apiUrl}Observations/geogridaggregation?zoom=10", new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<GeoGridResultDto>(resultString);
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }
    }
}
