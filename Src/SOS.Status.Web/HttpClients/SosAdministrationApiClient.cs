using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Status;

namespace SOS.Status.Web.HttpClients;

public class SosAdministrationApiClient
{
    private readonly HttpClient _httpClient;

    public SosAdministrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ProcessInfoDto>?> GetProcessInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProcessInfoDto>>("StatusInfo/process");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching process information: {ex.Message}");
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
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching harvest information: {ex.Message}");
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
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching active instance: {ex.Message}");
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
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching processing jobs: {ex.Message}");
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
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching search index info: {ex.Message}");
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
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching MongoDB info: {ex.Message}");
            return null;
        }
    }
}
