using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.Client.Models;
using System.Net.Http.Json;

namespace SOS.Status.Web.Client.HttpClients;

public class StatusInfoApiClient : IStatusInfoService
{
    private readonly HttpClient _http;

    public StatusInfoApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<DataProviderStatusRow>?> GetDataProviderStatusRowsAsync()
    {
        return await _http.GetFromJsonAsync<List<DataProviderStatusRow>>("api/StatusInfo/data-provider-status-rows");
    }

    public async Task<ProcessSummaryDto?> GetProcessSummaryAsync()
    {
        return await _http.GetFromJsonAsync<ProcessSummaryDto>("api/status-info/process-summary");
    }

    public async Task<IEnumerable<MongoDbProcessInfoDto>?> GetProcessInfoAsync()
    {
        return await _http.GetFromJsonAsync<IEnumerable<MongoDbProcessInfoDto>>("api/status-info/process-info");
    }

    public async Task<ProcessInfoDto?> GetProcessInfoAsync(bool active)
    {
        return await _http.GetFromJsonAsync<ProcessInfoDto>($"api/status-info/process-info/{active.ToString().ToLower()}");
    }

    public async Task<List<DataProviderStatusDto>?> GetDataProviderStatusAsync()
    {
        return await _http.GetFromJsonAsync<List<DataProviderStatusDto>>("api/status-info/data-provider-status");
    }

    public async Task<HealthReportDto?> GetObservationsApiHealthAsync()
    {
        return await _http.GetFromJsonAsync<HealthReportDto>("api/status-info/health/observations");
    }

    public async Task<HealthReportDto?> GetAnalysisApiHealthAsync()
    {
        return await _http.GetFromJsonAsync<HealthReportDto>("api/status-info/health/analysis");
    }

    public async Task<HealthReportDto?> GetElasticsearchProxyHealthAsync()
    {
        return await _http.GetFromJsonAsync<HealthReportDto>("api/status-info/health/elasticsearch-proxy");
    }

    public async Task<HealthReportDto?> GetDataStewardshipApiHealthAsync()
    {
        return await _http.GetFromJsonAsync<HealthReportDto>("api/status-info/health/data-stewardship");
    }
}