using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class ClearCacheEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapDelete("/caches/{cache}", ClearCacheAsync)
            .Produces<bool>(StatusCodes.Status200OK, "application/json")            
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
    
    [SwaggerOperation(        
        Description = "Clear Cache",
        OperationId = "ClearCache",
        Tags = new[] { "Caches" })]    
    private async Task<IResult> ClearCacheAsync(ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ILogger<ClearCacheEndpoint> logger,
        [FromRoute, SwaggerParameter("The cache", Required = true)][Required] Cache cache)
    {        
        if (cache == Cache.ProcessedConfiguration)
        {
            processedConfigurationCache.Clear();
            logger.LogInformation($"The {cache} cache was cleared");
        }
        else
        {
            logger.LogInformation($"The {cache} cache was requested to be cleared, but there is no implementation.");
        }
        
        return Results.Ok(true);
    }
}