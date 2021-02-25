using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using SOS.Lib.Extensions;

namespace SOS.Observations.Api.ApplicationInsights
{
    /// <summary>
    /// 
    /// </summary>
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
            if (new[] { "post", "put" }.Contains(platformContext.Request.Method, StringComparer.CurrentCultureIgnoreCase))
            {
                if (_loggRequestBody && platformContext.Request.Body.CanRead && platformContext.Request.Body.CanSeek)
                {
                    try
                    {
                        platformContext.Request.Body.Seek(0, SeekOrigin.Begin);

                        using var streamReader = new StreamReader(platformContext.Request.Body, Encoding.UTF8, true, 1024, true);

                        var body = streamReader.ReadToEnd();
                        // Rewind, so the core is not lost when it looks the body for the request
                        platformContext.Request.Body.Position = 0;

                        telemetry.Context.Properties.Add("Request-body", body);
                    }
                    catch
                    {

                    }
                }

                if (_loggSearchResponseCount)
                {
                    platformContext.Items.TryGetValue("Response-count", out var count);
                    if (int.TryParse(count?.ToString(), out var responseCount))
                    {
                        telemetry.Context.Properties.Add("Response-count", responseCount.ToString());
                    }
                }
            }

            var nameidentifier = platformContext.User?.Claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value;

            // If we have a logged in user, use nameidentifier
            if (!string.IsNullOrEmpty(nameidentifier))
            {
                telemetry.Context.User.AuthenticatedUserId = $"U-ID:{nameidentifier}";
            }

            // If it's a call from Azure API management, we should have APU user id in header 
            if (platformContext.Request.Headers.ContainsKey("request-user-id"))
            {
                telemetry.Context.User.AuthenticatedUserId = $"API-U-ID:{platformContext.Request.Headers["request-user-id"]}";
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        public TelemetryInitializer(IHttpContextAccessor httpContextAccessor, Lib.Configuration.ObservationApi.ApplicationInsights applicationInsightsConfiguration) : base(httpContextAccessor)
        {
            _loggRequestBody = applicationInsightsConfiguration.EnableRequestBodyLogging;
            _loggSearchResponseCount = applicationInsightsConfiguration.EnableSearchResponseCountLogging;
        }
    }
}
