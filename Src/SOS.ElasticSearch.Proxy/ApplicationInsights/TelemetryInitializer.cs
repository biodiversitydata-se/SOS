using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Primitives;

namespace SOS.ElasticSearch.Proxy.ApplicationInsights
{
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
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        public TelemetryInitializer(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            
        }
    }
}
