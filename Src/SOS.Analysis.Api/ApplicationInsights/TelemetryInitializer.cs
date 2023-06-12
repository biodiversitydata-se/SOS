using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Primitives;

namespace SOS.Analysis.Api.ApplicationInsights;

public class TelemetryInitializer : TelemetryInitializerBase
{
    private readonly bool _loggRequestBody;
    private readonly bool _loggSearchResponseCount;
    /// <summary>
    /// Initialize event
    /// </summary>
    /// <param name="platformContext"></param>
    /// <param name="requestTelemetry"></param>
    /// <param name="telemetry"></param>
    protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
    {
        if (new[] { "get", "post", "put" }.Contains(platformContext.Request.Method, StringComparer.CurrentCultureIgnoreCase))
        {
            if (platformContext.Request.Query.TryGetValue("protectedObservations", out var value) && !telemetry.Context.GlobalProperties.ContainsKey("Protected-observations"))
            {
                telemetry.Context.GlobalProperties.Add("Protected-observations", value);
            }

            if (_loggRequestBody && platformContext.Items.TryGetValue("Request-body", out var requestBody))
            {
                if (!telemetry.Context.GlobalProperties.ContainsKey("Request-body"))
                {
                    var body = requestBody?.ToString();
                    telemetry.Context.GlobalProperties.Add("Request-body", body);
                }
            }

            if (_loggSearchResponseCount && platformContext.Items.TryGetValue("Observation-count", out var observationCount))
            {
                if (int.TryParse(observationCount?.ToString(), out var obsCount) && !telemetry.Context.GlobalProperties.ContainsKey("Observation-count"))
                {
                    telemetry.Context.GlobalProperties.Add("Observation-count", obsCount.ToString());
                }
            }

            if (platformContext.Request.ContentLength != null && !telemetry.Context.GlobalProperties.ContainsKey("Request-length"))
            {
                telemetry.Context.GlobalProperties.Add("Request-length", platformContext.Request.ContentLength.ToString());
            }
        }

        var nameidentifier = platformContext.User?.Claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value;

        // If we have a logged in user, use nameidentifier
        if (!string.IsNullOrEmpty(nameidentifier))
        {
            telemetry.Context.User.AuthenticatedUserId = nameidentifier;
        }

        // If it's a call from Azure API management, we should have Azure user id in header 
        if (platformContext.Request.Headers.TryGetValue("request-user-id", out var accountId))
        {
            telemetry.Context.User.AccountId = accountId;
        }

        if (!telemetry.Context.GlobalProperties.ContainsKey("Requesting-System"))
        {
            var requestingSystem = new StringValues();
            if (!platformContext.Request.Headers.TryGetValue("X-Requesting-System", out requestingSystem))
            {
                if (!platformContext.Request.Headers.TryGetValue("Requesting-System", out requestingSystem))
                {
                    requestingSystem = new StringValues("N/A");
                }
            }
            telemetry.Context.GlobalProperties.Add("Requesting-System", requestingSystem.ToString());
        }


    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="applicationInsightsConfiguration"></param>
    public TelemetryInitializer(IHttpContextAccessor httpContextAccessor, Lib.Configuration.Shared.ApplicationInsights applicationInsightsConfiguration) : base(httpContextAccessor)
    {
        _loggRequestBody = applicationInsightsConfiguration.EnableRequestBodyLogging;
        _loggSearchResponseCount = applicationInsightsConfiguration.EnableSearchResponseCountLogging;
    }
}

