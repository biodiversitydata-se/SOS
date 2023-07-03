using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Endpoints.ApiInfo;

public class GetApiInfoEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/api_info", GetApiInfoAsync)
            .Produces<ApiInformation>(StatusCodes.Status200OK, "application/json");
    }

    [SwaggerOperation(
        Description = "Get API information",
        OperationId = "GetApiInfo",
        Tags = new[] { "ApiInfo" })]    
    private async Task<IResult> GetApiInfoAsync(IDataStewardshipManager dataStewardshipManager)
    {
        var buildDate = Assembly.GetExecutingAssembly().GetBuildDate();
        string version = Assembly.GetExecutingAssembly().GetVersionNumber();

        var apiInformation = new ApiInformation
        {
            ApiName = "Data Stewardship API",
            ApiStatus = "active",
            ApiVersion = version,
            ApiDocumentation = new Uri("https://github.com/biodiversitydata-se/SOS"),
            ApiReleased = new DateTimeOffset(buildDate.ToLocalTime())            
        };

        return Results.Ok(apiInformation);        
    }
}