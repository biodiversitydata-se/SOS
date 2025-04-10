﻿using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

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
            if (telemetry is not RequestTelemetry)
            {
                return;
            }

            if (new[] { "get", "post", "put" }.Contains(platformContext.Request.Method, StringComparer.CurrentCultureIgnoreCase))
            {

                if (platformContext.Request.Query.TryGetValue("protectedObservations", out var value) && !requestTelemetry.Context.GlobalProperties.ContainsKey("Protected-observations"))
                {
                    requestTelemetry.Context.GlobalProperties.Add("Protected-observations", value);
                }              

                if (platformContext.Items.TryGetValue("ApiUserType", out object apiUserType))
                {
                    if (apiUserType != null && !telemetry.Context.GlobalProperties.ContainsKey("ApiUserType"))
                    {
                        telemetry.Context.GlobalProperties.Add("ApiUserType", apiUserType.ToString());
                    }
                }

                if (platformContext.Items.TryGetValue("SemaphoreLimitUsed", out object semaphoreLimitUsed))
                {
                    if (semaphoreLimitUsed != null && !telemetry.Context.GlobalProperties.ContainsKey("SemaphoreLimitUsed"))
                    {
                        telemetry.Context.GlobalProperties.Add("SemaphoreLimitUsed", semaphoreLimitUsed.ToString());
                    }
                }

                if (platformContext.Items.TryGetValue("SemaphoreWaitSeconds", out object? semaphoreWaitSeconds))
                {
                    if (semaphoreWaitSeconds != null && !telemetry.Context.GlobalProperties.ContainsKey("SemaphoreWaitSeconds"))
                    {
                        telemetry.Context.GlobalProperties.Add("SemaphoreWaitSeconds", semaphoreWaitSeconds.ToString());
                    }
                }

                if (_loggRequestBody && platformContext.Items.TryGetValue("Request-body", out var requestBody))
                {
                    if (!requestTelemetry.Context.GlobalProperties.ContainsKey("Request-body"))
                    {
                        var body = requestBody?.ToString();
                        requestTelemetry.Context.GlobalProperties.Add("Request-body", body);
                    }
                }

                if (_loggSearchResponseCount && platformContext.Items.TryGetValue("Observation-count", out var observationCount))
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

                if (platformContext.Items.TryGetValue("CacheKey", out object cacheKey))
                {
                    if (cacheKey != null && !telemetry.Context.GlobalProperties.ContainsKey("CacheKey"))
                    {
                        telemetry.Context.GlobalProperties.Add("CacheKey", cacheKey.ToString());
                    }
                }
            }

            var nameidentifier = platformContext.User?.Claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value;

            // If we have a logged in user, use nameidentifier
            if (!string.IsNullOrEmpty(nameidentifier))
            {
                requestTelemetry.Context.User.AuthenticatedUserId = nameidentifier;
            }

            // If it's a call from Azure API management, we should have Azure user id in header 
            if (platformContext.Request.Headers.TryGetValue("request-user-id", out var accountId))
            {
                requestTelemetry.Context.User.AccountId = accountId;
            }

            if (!requestTelemetry.Context.GlobalProperties.ContainsKey("Requesting-System"))
            {
                var requestingSystem = new StringValues();
                if (!platformContext.Request.Headers.TryGetValue("X-Requesting-System", out requestingSystem))
                {
                    if (!platformContext.Request.Headers.TryGetValue("Requesting-System", out requestingSystem))
                    {
                        requestingSystem = new StringValues("N/A");
                    }
                }
                requestTelemetry.Context.GlobalProperties.Add("Requesting-System", requestingSystem.ToString());
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
}
