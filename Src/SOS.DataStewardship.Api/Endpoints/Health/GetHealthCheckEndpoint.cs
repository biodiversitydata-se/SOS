using SOS.DataStewardship.Api.Contracts.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SOS.DataStewardship.Api.Endpoints.External;

public class GetHealthCheckEndpoint : IEndpointDefinition
{
    /// <summary>
    /// Define end point
    /// </summary>
    /// <param name="app"></param>
    public void DefineEndpoint(WebApplication app)
    {
        app.MapGet("/health/external", GetExternalHealthCheckAsync)
            .WithName("GetHealthCheckExternal")
            .WithTags("Health")
            .Produces<Dataset>(StatusCodes.Status200OK, "application/json")
            .Produces<Dataset>(StatusCodes.Status200OK, "text/csv")
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Get current system health status.
    /// </summary>
    /// <param name="healthCheckService"></param>
    /// <returns></returns>
    private async Task<IResult> GetExternalHealthCheckAsync(HealthCheckService healthCheckService)
    {
        try
        {
            var start = DateTime.Now;
            var healthCheck = await healthCheckService.CheckHealthAsync(new CancellationToken());
            var entries = new Dictionary<string, HealthReportEntry>();

            var healthChecks = new[]
            {
                healthCheck.Entries?.Where(e => e.Key.Equals("Dataset"))?.Select(d => d.Value)?.FirstOrDefault(),
                healthCheck.Entries?.Where(e => e.Key.Equals("Event"))?.Select(d => d.Value)?.FirstOrDefault(),
                healthCheck.Entries?.Where(e => e.Key.Equals("Occurrence"))?.Select(d => d.Value)?.FirstOrDefault()
            };

            var healthy = healthChecks.All(hc => hc
                .Value
                    .Status
                        .Equals(HealthStatus.Healthy)
            );
            var azureStatus = healthy ? "Running" : "Not running";

            
            entries.Add("DataStewartship", new HealthReportEntry(
                healthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                azureStatus,
                start - DateTime.Now,
                null,
                new Dictionary<string, object>()
                {
                    { "Status", azureStatus }
                })
            );
            
           

            return Results.Ok(new HealthReport(entries, DateTime.Now - start));
        }
        catch (Exception)
        {
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }    
}