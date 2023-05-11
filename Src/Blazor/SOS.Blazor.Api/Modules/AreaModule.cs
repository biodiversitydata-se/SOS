namespace SOS.Blazor.Api.Modules;

public class AreaModule : IModule
{
    public void MapEndpoints(WebApplication application)
    {
        application.MapGet("areas", async (ISosClient sosClient, 
            [FromQuery] int skip, 
            [FromQuery] int take, 
            [FromQuery] AreaType areaType) =>
        {
            return await sosClient.GetAreasAsync(skip, take, areaType);
        });
    }
}
