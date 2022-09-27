using SOS.DataHost.Api.Modules.Interfaces;

namespace SOS.DataHost.Api.Modules;

public class DataHostModule : IModule
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