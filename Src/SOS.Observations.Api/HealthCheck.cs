using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;


public class HealthCheck : IHealthCheck
{

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO - add a real health check here! perhaps a SELECT 1 query as in the example below?
            // await _dbConnFactory.CreateDbConnection().QueryAsync("SELECT 1"); 
            // return HealthCheckResult.Healthy("SELECT 1 Query succeeded (postgres db)");

            // for now...
            return Task.FromResult(HealthCheckResult.Healthy("void health check gives thumbs up"));            
        }
        catch
        {
            // return new HealthCheckResult(
            //     context.Registration.FailureStatus, "SELECT 1 Query FAILED (postgres db)");
            return Task.FromResult(HealthCheckResult.Unhealthy("void health check gives thumbs down"));            
        }
    }
}
