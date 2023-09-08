using Swashbuckle.AspNetCore.Annotations;
using SOS.Lib.Models.ApiInfo;

namespace SOS.DataStewardship.Api.Endpoints.ApiInfo;

/// <summary>
/// 
/// </summary>
public class GetApiInfoEndpoint : IEndpointDefinition
{
    /// <summary>
    /// Define end point
    /// </summary>
    /// <param name="app"></param>
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/api_info", GetApiInfo)
            .Produces<ApiInformation>(StatusCodes.Status200OK, "application/json");
    }

    [SwaggerOperation(
        Description = "Get API information",
        OperationId = "GetApiInfo",
        Tags = new[] { "ApiInfo" })]    
    private IResult GetApiInfo()
    {
        var buildDate = Assembly.GetExecutingAssembly().GetBuildDate();
        string version = Assembly.GetExecutingAssembly().GetVersionNumber();

        var apiInformation = new ApiInformation
        {
            ApiName = "Nature data - systematic species observations",
            ApiStatus = "active",
            ApiVersion = version,
            ApiDocumentation = new Uri("https://github.com/biodiversitydata-se/SOS/blob/master/Docs/DataStewardship/README.md"),
            ApiChangelog = new Uri("https://github.com/biodiversitydata-se/SOS/blob/master/Docs/DataStewardship/CHANGELOG.md"),
            ApiReleased = new DateTimeOffset(buildDate.ToLocalTime())            
        };

        return Results.Ok(apiInformation);        
    }
}