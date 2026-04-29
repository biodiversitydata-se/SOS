using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SOS.Lib.Extensions;

public static class ApplicationInsightsExtensions
{
    extension(ITelemetry telemetry)
    {
        /// <summary>
        /// Attempts to add the client's IP address to the telemetry properties if it is not already present.
        /// </summary>
        /// <remarks>The method checks for the client IP address in the 'X-Forwarded-For' header, the
        /// remote connection address, and the telemetry context, in that order. The IP address is added to the
        /// telemetry properties under the key 'client-ip' only if it is not already set.</remarks>
        /// <param name="platformContext">The HTTP context containing the request and connection information used to determine the client's IP
        /// address. Cannot be null.</param>
        public void TryAddClientIP(HttpContext platformContext)
        {
            if (telemetry is ISupportProperties propTelemetry && !propTelemetry.Properties.ContainsKey("client-ip"))
            {
                string clientIp = null;
                if (platformContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
                {
                    clientIp = forwardedFor.ToString().Split(',')[0].Trim();
                }
                else if (platformContext.Connection.RemoteIpAddress != null)
                {
                    clientIp = platformContext.Connection.RemoteIpAddress.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(telemetry.Context.Location.Ip))
                {
                    clientIp = telemetry.Context.Location.Ip;
                }

                if (!string.IsNullOrWhiteSpace(clientIp))
                {
                    propTelemetry.Properties["client-ip"] = clientIp;
                }
            }
        }

        /// <summary>
        /// Attempts to add the requesting system information from the HTTP request headers to the telemetry global
        /// properties if it is not already present.
        /// </summary>
        /// <remarks>This method checks for the 'X-Requesting-System' or 'Requesting-System' headers in
        /// the incoming HTTP request. If neither header is present, the value 'N/A' is used. The requesting system
        /// information is added to the telemetry global properties under the key 'Requesting-System' only if it does
        /// not already exist.</remarks>
        /// <param name="platformContext">The HTTP context containing the request headers from which the requesting system information is retrieved.</param>
        public void TryAddRequestingSystem(HttpContext platformContext)
        {
            if (telemetry is RequestTelemetry requestTelemetry && !requestTelemetry.Context.GlobalProperties.ContainsKey("Requesting-System"))
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
    }
}