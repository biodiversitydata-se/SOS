using Swashbuckle.AspNetCore.Annotations;
using SOS.DataStewardship.Api.Application.Managers.Interfaces;
using SOS.DataStewardship.Api.Contracts.Models;
using System.Globalization;

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
        var buildDate = GetBuildDate(Assembly.GetExecutingAssembly());
        string version = GetVersionNumber(Assembly.GetExecutingAssembly());

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

    private static string GetVersionNumber(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(0, index);
                return value;
            }
        }

        return default;
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }

        return default;
    }
}