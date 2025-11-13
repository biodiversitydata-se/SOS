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
            .WithName("GetApiInfo")
            .WithTags("ApiInfo")
            .Produces<ApiInformation>(StatusCodes.Status200OK, "application/json");
    }

    /// <summary>
    /// Get API information
    /// </summary>
    /// <remarks>Get information about the API: Name, Status, Version, Documentation, Change log, Release date.</remarks>
    /// <returns></returns>
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