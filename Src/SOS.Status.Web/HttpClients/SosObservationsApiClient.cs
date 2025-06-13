using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Dtos.Status;
using SOS.Status.Web.Models;

namespace SOS.Status.Web.HttpClients;

public class SosObservationsApiClient
{
    private readonly HttpClient _httpClient;
    public SosObservationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResultDto<Observation>?> SearchObservationsAsync(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false,
        string outputFormat = "Json")
    {
        var url = $"/observations/internal/search?skip={skip}&take={take}&sortBy={sortBy}&sortOrder={sortOrder}&validateSearchFilter={validateSearchFilter}&translationCultureCode={translationCultureCode}&sensitiveObservations={sensitiveObservations}&outputFormat={outputFormat}";
        var response = await _httpClient.PostAsJsonAsync(url, filter);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();
    }

    public async Task<ProcessSummaryDto?> GetProcessSummary()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProcessSummaryDto>($"systems/process-summary");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching process summary: {ex.Message}");
            return null;
        }
    }

    public async Task<ProcessInfoDto?> GetProcessInfo(bool active)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProcessInfoDto>($"Systems/ProcessInformation?active={active}");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching process information: {ex.Message}");
            return null;
        }
    }

    public async Task<List<DataProviderStatusDto>?> GetDataProviderStatus()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<DataProviderStatusDto>>("Systems/dataproviderstatus");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching data provider status: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<MongoDbProcessInfoDto>?> GetProcessInfo()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MongoDbProcessInfoDto>>("systems/processmongodb");
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
            return await _httpClient.GetFromJsonAsync<IEnumerable<HarvestInfoDto>>("systems/harvest");
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
            return await _httpClient.GetFromJsonAsync<ActiveInstanceInfoDto>("systems/activeinstance");
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
            return await _httpClient.GetFromJsonAsync<IEnumerable<HangfireJobDto>>("systems/processing");
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
            return await _httpClient.GetFromJsonAsync<SearchIndexInfoDto>("systems/searchindex");
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
            return await _httpClient.GetFromJsonAsync<MongoDbInfoDto>("systems/mongoinfo");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching MongoDB info: {ex.Message}");
            return null;
        }
    }

    public async Task<HealthReportDto?> GetHealthAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<HealthReportDto>("health");
        }
        catch (Exception ex)
        {
            // In a production environment, you would want to handle this error appropriately
            Console.WriteLine($"Error fetching health: {ex.Message}");
            return null;
        }
    }
}