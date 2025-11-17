using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Primitives;

namespace SOS.ElasticSearch.Proxy.ApplicationInsights;

/// <summary>
/// 
/// </summary>
public class TelemetryInitializer : TelemetryInitializerBase
{
    /// <summary>
    /// Initialize event
    /// </summary>
    /// <param name="platformContext"></param>
    /// <param name="requestTelemetry"></param>
    /// <param name="telemetry"></param>
    protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
    {
        if (!telemetry.Context.GlobalProperties.ContainsKey("Requesting-System"))
        {
            var requestingSystem = new StringValues();
            if (!platformContext.Request.Headers.TryGetValue("X-Requesting-System", out requestingSystem))
            {
                if (!platformContext.Request.Headers.TryGetValue("Requesting-System", out requestingSystem))
                {
                    requestingSystem = new StringValues("WFS");
                }
            }
            telemetry.Context.GlobalProperties.Add("Requesting-System", requestingSystem.ToString());
        }

        if (platformContext.Items.TryGetValue("ApiUserType", out object? apiUserType))
        {
            if (apiUserType != null && !telemetry.Context.GlobalProperties.ContainsKey("ApiUserType"))
            {
                telemetry.Context.GlobalProperties.Add("ApiUserType", apiUserType.ToString());
            }
        }

        if (platformContext.Items.TryGetValue("SemaphoreStatus", out object? semaphoreStatus))
        {
            if (semaphoreStatus != null && !telemetry.Context.GlobalProperties.ContainsKey("SemaphoreStatus"))
            {
                telemetry.Context.GlobalProperties.Add("SemaphoreStatus", semaphoreStatus.ToString());
            }
        }

        if (platformContext.Items.TryGetValue("SemaphoreWaitSeconds", out object? semaphoreWaitSeconds))
        {
            if (semaphoreWaitSeconds != null && !telemetry.Context.GlobalProperties.ContainsKey("SemaphoreWaitSeconds"))
            {
                telemetry.Context.GlobalProperties.Add("SemaphoreWaitSeconds", semaphoreWaitSeconds.ToString());
            }
        }

        if (platformContext.Items.TryGetValue("Observation-count", out var observationCount))
        {
            if (int.TryParse(observationCount?.ToString(), out var obsCount) && !requestTelemetry.Context.GlobalProperties.ContainsKey("Observation-count"))
            {
                requestTelemetry.Context.GlobalProperties.Add("Observation-count", obsCount.ToString());
            }
        }

        if (platformContext.Request.ContentLength != null && !requestTelemetry.Context.GlobalProperties.ContainsKey("Request-length"))
        {
            requestTelemetry.Context.GlobalProperties.Add("Request-length", platformContext.Request.ContentLength.ToString());
        }

        if (platformContext.Items.TryGetValue("CacheKey", out object? cacheKey))
        {
            if (cacheKey != null && !telemetry.Context.GlobalProperties.ContainsKey("CacheKey"))
            {
                telemetry.Context.GlobalProperties.Add("CacheKey", cacheKey.ToString());
            }
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public TelemetryInitializer(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {

    }
}
