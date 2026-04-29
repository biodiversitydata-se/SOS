using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using SOS.Lib.Extensions;

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
        telemetry.TryAddClientIP(platformContext);

        if (telemetry is not RequestTelemetry)
        {
            return;
        }

        requestTelemetry.TryAddRequestingSystem(platformContext);

        if (platformContext.Items.TryGetValue("ApiUserType", out object? apiUserType))
        {
            if (apiUserType != null && !requestTelemetry.Context.GlobalProperties.ContainsKey("ApiUserType"))
            {
                requestTelemetry.Context.GlobalProperties.Add("ApiUserType", apiUserType.ToString());
            }
        }

        if (platformContext.Items.TryGetValue("SemaphoreStatus", out object? semaphoreStatus))
        {
            if (semaphoreStatus != null && !requestTelemetry.Context.GlobalProperties.ContainsKey("SemaphoreStatus"))
            {
                requestTelemetry.Context.GlobalProperties.Add("SemaphoreStatus", semaphoreStatus.ToString());
            }
        }

        if (platformContext.Items.TryGetValue("SemaphoreWaitSeconds", out object? semaphoreWaitSeconds))
        {
            if (semaphoreWaitSeconds != null && !requestTelemetry.Context.GlobalProperties.ContainsKey("SemaphoreWaitSeconds"))
            {
                requestTelemetry.Context.GlobalProperties.Add("SemaphoreWaitSeconds", semaphoreWaitSeconds.ToString());
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
            if (cacheKey != null && !requestTelemetry.Context.GlobalProperties.ContainsKey("CacheKey"))
            {
                requestTelemetry.Context.GlobalProperties.Add("CacheKey", cacheKey.ToString());
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
