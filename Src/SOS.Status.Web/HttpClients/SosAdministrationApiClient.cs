using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Status;

namespace SOS.Status.Web.HttpClients;

public class SosAdministrationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SosAdministrationApiClient> _logger;

    public SosAdministrationApiClient(HttpClient httpClient, ILogger<SosAdministrationApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<ProcessInfoDto>?> GetProcessInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProcessInfoDto>>("StatusInfo/process");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching process information: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<HarvestInfoDto>?> GetHarvestInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HarvestInfoDto>>("StatusInfo/harvest");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching harvest information: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<ActiveInstanceInfoDto?> GetActiveInstance()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ActiveInstanceInfoDto>("StatusInfo/activeinstance");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching active instance: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<HangfireJobDto>?> GetProcessing()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HangfireJobDto>>("StatusInfo/processing");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching processing jobs: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<SearchIndexInfoDto?> GetSearchIndexInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SearchIndexInfoDto>("StatusInfo/searchindex");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching index info: {ErrorMsg}", ex.Message);
            return null;
        }
    }

    public async Task<MongoDbInfoDto?> GetMongoDatabaseInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MongoDbInfoDto>("StatusInfo/mongoinfo");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching MongoDB info: {ErrorMsg}", ex.Message);
            return null;
        }
    }
}