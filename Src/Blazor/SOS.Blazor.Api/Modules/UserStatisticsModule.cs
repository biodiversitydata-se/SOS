namespace SOS.Blazor.Api.Modules;

public class UserStatisticsModule : IModule
{
    public void MapEndpoints(WebApplication application)
    {
        application.MapPost("userstatistics/pagedspeciescountaggregation", async (ISosUserStatisticsClient sosUserStatisticsClient,
            [FromBody] SpeciesCountUserStatisticsQuery query, [FromQuery] int skip, [FromQuery] int take, [FromQuery] bool? useCache) =>
        {
            return await sosUserStatisticsClient.GetUserStatisticsAsync(skip, take, useCache.GetValueOrDefault(true), query);
        });
    }
}
