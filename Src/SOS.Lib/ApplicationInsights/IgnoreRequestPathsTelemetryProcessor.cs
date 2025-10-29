using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Linq;

namespace SOS.Lib.ApplicationInsights
{
    public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly string[] _ignorePaths = { "swagger", ".", "healthz", "/metrics" };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        public IgnoreRequestPathsTelemetryProcessor(ITelemetryProcessor next)
        {
            _next = next;
        }

        public void Process(ITelemetry item)
        {
            var requestTelemetry = item as RequestTelemetry;

            if (requestTelemetry != null)
            {
                if (_ignorePaths.Any(ignorePath => requestTelemetry.Url.AbsolutePath.Contains(ignorePath, StringComparison.CurrentCultureIgnoreCase)))
                { 
                    return;
                }
            }

            _next.Process(item);
        }
    }
}