using SOS.Status.Web.Client.Models;

namespace SOS.Status.Web.HttpClients;

public class SosDataStewardshipApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SosDataStewardshipApiClient> _logger;
    public SosDataStewardshipApiClient(HttpClient httpClient, ILogger<SosDataStewardshipApiClient> logger)
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