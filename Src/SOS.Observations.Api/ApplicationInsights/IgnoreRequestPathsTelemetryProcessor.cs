using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SOS.Observations.Api.ApplicationInsights
{
    public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

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
            if (item is RequestTelemetry request &&
                (request.Url.AbsolutePath.StartsWith("/swagger/")))
            {
                return;
            }

            _next.Process(item);
        }
    }
}
