namespace SOS.Blazor.Api.Clients.Interfaces;

public interface ISosClient
{
    Task<PagedResult<Area>> GetAreasAsync(int skip, int take, AreaType areaType);
    Task<DataProvider[]> GetDataProvidersAsync();
    Task<ProcessInfo> GetProcessInfoAsync(bool active);
}
