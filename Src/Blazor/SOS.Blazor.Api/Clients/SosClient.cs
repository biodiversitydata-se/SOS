namespace SOS.Blazor.Api.Clients;

public class SosClient : ISosClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SosClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PagedResult<Area>> GetAreasAsync(int skip, int take, AreaType areaType)
    {
        var url = $"areas?take={take}&skip={skip}&areaTypes={areaType}";
        var res = await _httpClientFactory.CreateClient("SosClient").GetAsync(url);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<PagedResult<Area>>()
            : throw new Exception("Call to API failed, responseCode: " + res.ToString());
    }

    public async Task<DataProvider[]> GetDataProvidersAsync()
    {
        var url = $"DataProviders?cultureCode=sv-SE&includeProvidersWithNoObservations=false";
        var res = await _httpClientFactory.CreateClient("SosClient").GetAsync(url);

        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<DataProvider[]>()
            : throw new Exception("Call to API failed, responseCode:" + res.StatusCode);
    }

    public async Task<ProcessInfo> GetProcessInfoAsync(bool active)
    {
        var url = $"systems/processinformation?active={active}";
        var res = await _httpClientFactory.CreateClient("SosClient").GetAsync(url);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ProcessInfo>()
            : throw new Exception("Call to API failed, responseCode:" + res.StatusCode);
    }
}
