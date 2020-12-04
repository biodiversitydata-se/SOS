using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Configuration;

namespace SOS.Observations.Api.IntegrationTests.ApiClients
{
    public class SosApiClient
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;

        public SosApiClient(ApiTestConfiguration apiTestConfiguration)
        {
            _client = new HttpClient();
            _apiUrl = apiTestConfiguration.ApiUrl;
        }

        public async Task<PagedResultDto<Observation>> SearchSos(SearchFilterDto searchFilter, int take, int skip)
        {
            var response = await _client.PostAsync($"{_apiUrl}Observations/Search?take={take}&skip={skip}", new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<PagedResultDto<Observation>>(resultString);
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<PagedResultDto<TaxonAggregationItemDto>> SearchSosTaxonAggregation(SearchFilterDto searchFilter, int take, int skip, double? bboxleft = null, double? bboxtop = null, double? bboxright = null, double? bboxbottom = null)
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
                return JsonConvert.DeserializeObject<PagedResultDto<TaxonAggregationItemDto>>(resultString);
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<GeoGridResultDto> SearchSosGeoAggregation(SearchFilterDto searchFilter)
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