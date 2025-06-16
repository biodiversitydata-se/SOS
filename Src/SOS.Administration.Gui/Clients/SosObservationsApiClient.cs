using Newtonsoft.Json;
using SOS.Administration.Gui.Services;
using SOS.Administration.Gui.Dtos;
using System.Net.Http;
using System.Text;

namespace SOS.Administration.Gui.Clients;

public class SosObservationsApiClient
{
    private readonly HttpClient _httpClient;

    public SosObservationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SOS.Shared.Api.Dtos.EnvironmentInformationDto> GetEnvironmentAsync()
    {
        var response = await _httpClient.GetAsync($"/environment");
        if (response.IsSuccessStatusCode)
        {
            var resultString = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<SOS.Shared.Api.Dtos.EnvironmentInformationDto>(resultString);
        }
        else
        {
            throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
        }
    }

    public async Task<PagedResult<SOSObservation>> SearchSOS(SearchFilterDto searchFilter, int take, int skip)
    {
        var response = await _httpClient.PostAsync($"/Observations/Search?take={take}&skip={skip}", new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
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
        var response = await _httpClient.PostAsync($"/Observations/TaxonAggregation?take={take}&skip={skip}" + bboxstring, new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
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
        var response = await _httpClient.PostAsync($"/Observations/geogridaggregation?zoom=10", new StringContent(JsonConvert.SerializeObject(searchFilter), Encoding.UTF8, "application/json"));
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

    public async Task<string> GetHealthStatus()
    {
        var response = await _httpClient.GetAsync("/health-json");
        var resultString = await response.Content.ReadAsStringAsync();

        return resultString;
    }
}
