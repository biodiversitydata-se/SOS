using SOS.DataStewardship.Api.Modules.Interfaces;

namespace SOS.DataStewardship.Api.Modules;

public class DataStewardshipModule : IModule
{
    public void MapEndpoints(WebApplication application)
    {
        application.MapGet("datahost/test", GetTestResult)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    internal IResult GetTestResult()
    {
        return Results.Ok("ok");
    }
}