using Microsoft.AspNetCore.Mvc;

namespace SOS.Shared.Api.Extensions.Controller
{
    public static class ControllerExtensions
    {
        public static void LogObservationCount(this ControllerBase controller, long observationCount)
        {
            if (controller?.HttpContext == null)
            {
                return;
            }

            controller.HttpContext.Items.TryAdd("Observation-count", observationCount);
        }
    }
}
