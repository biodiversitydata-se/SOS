using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

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

        public static void LogUserType(this ControllerBase controller, ApiUserType userType)
        {
            if (controller?.HttpContext == null)
            {
                return;
            }

            controller.HttpContext.Items.TryAdd("ApiUserType", userType);
        }

        public static string? GetEndpointName(this ControllerBase controller, ControllerContext controllerContext)
        {
            return $"/{controllerContext?.ActionDescriptor?.AttributeRouteInfo?.Template}";
        }

        public static ApiUserType GetApiUserType(this ControllerBase controller)
        {
            if (controller.HttpContext.Items.TryGetValue("ApiUserType", out var userTypeObj) && userTypeObj is ApiUserType userType)
            {
                return userType;
            }

            return ApiUserType.Unknown;
        }
    }
}
