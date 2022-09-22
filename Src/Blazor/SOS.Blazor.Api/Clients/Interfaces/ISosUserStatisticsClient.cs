namespace SOS.Blazor.Api.Clients.Interfaces;

public interface ISosUserStatisticsClient
{
    Task<PagedResult<UserStatisticsItem>> GetUserStatisticsAsync(int skip, int take, bool useCache, SpeciesCountUserStatisticsQuery query);
}
