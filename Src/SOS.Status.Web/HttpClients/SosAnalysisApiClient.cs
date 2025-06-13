using SOS.Status.Web.Models;

namespace SOS.Status.Web.HttpClients;

public class SosAnalysisApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SosAnalysisApiClient> _logger;
    public SosAnalysisApiClient(HttpClient httpClient, ILogger<SosAnalysisApiClient> logger)
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