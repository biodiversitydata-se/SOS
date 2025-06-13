using SOS.Status.Web.Models;

namespace SOS.Status.Web.HttpClients;

public class SosElasticsearchProxyClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SosElasticsearchProxyClient> _logger;
    public SosElasticsearchProxyClient(HttpClient httpClient, ILogger<SosElasticsearchProxyClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthReportDto?> GetHealthAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<HealthReportDto>("health");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching health: {ex.Message}");
            return null;
        }
    }
}