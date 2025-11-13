namespace SOS.DataStewardship.Api.Endpoints.DataStewardship;

public class ClearCacheEndpoint : IEndpointDefinition
{
    public void DefineEndpoint(WebApplication app)
    {
        app.MapDelete("/caches/{cache}", ClearCacheAsync)
            .WithName("ClearCache")
            .WithTags("Caches")
            .Produces<bool>(StatusCodes.Status200OK, "application/json")            
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }       

    /// <summary>
    /// Clear cache
    /// </summary>
    /// <param name="processedConfigurationCache"></param>
    /// <param name="logger"></param>
    /// <param name="cache">The cache to clear</param>
    /// <returns></returns>
    private async Task<IResult> ClearCacheAsync(ICache<string, ProcessedConfiguration> processedConfigurationCache,
        ILogger<ClearCacheEndpoint> logger,
        [FromRoute] Cache cache)
    {        
        // if (cache == Cache.ProcessedConfiguration)
        // {
        //     processedConfigurationCache.Clear();
        //     logger.LogInformation($"The {cache} cache was cleared");
        // }
        // else
        // {
        //     logger.LogInformation($"The {cache} cache was requested to be cleared, but there is no implementation.");
        // }
        
        // return Results.Ok(true);

        return Results.Ok("Disabled");
    }
}